using System.Xml.Linq;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using ProjectAnalyzer.Core.Helpers;
using ProjectAnalyzer.Core.Models;

namespace ProjectAnalyzer.Core.Managers
{
    public class NugetManager
    {
        private readonly List<string> _sources;
        private readonly Dictionary<string, NuGetVersion> _versionCache = new();
        private readonly VulnerabilityHelpers _vulnerabilityHelpers;

        public NugetManager(
            List<string> sources,
            string gitHubToken)
        {
            _sources = sources;
            _vulnerabilityHelpers = new VulnerabilityHelpers(gitHubToken);
        }
        public async Task<NuGetVersion?> GetLatestNuGetVersion(string packageName, bool preRelease)
        {
            if (_versionCache.TryGetValue(packageName, out var cachedVersion))
            {
                return cachedVersion;
            }

            NuGetVersion? latestVersion = null;
            foreach (var source in _sources)
            {
                var repository = Repository.Factory.GetCoreV3(source);
                var metadataResource = await repository.GetResourceAsync<MetadataResource>();

                var versions = await metadataResource.GetVersions(packageName, new SourceCacheContext(), NullLogger.Instance, CancellationToken.None);
                var sourceLatestVersion = versions
                    .Where(r => r.IsPrerelease == preRelease)
                    .MaxBy(v => v);
                if (sourceLatestVersion != null)
                {
                    latestVersion = sourceLatestVersion;
                    break;
                }
            }

            if (latestVersion != null)
            {
                _versionCache[packageName] = latestVersion;
            }

            return latestVersion;
        }
        public async Task<List<NuGetPackage>> ReadPackagesFromConfigFile(string packagesConfigPath)
        {
            var nugetPackages = new List<NuGetPackage>();

            if (File.Exists(packagesConfigPath))
            {
                var doc = XDocument.Load(packagesConfigPath);

                var packages = doc.Element("packages")?.Elements("package");
                if (packages != null)
                {
                    foreach (var package in packages)
                    {
                        var packageName = package.Attribute("id")?.Value;
                        var packageVersion = package.Attribute("version")?.Value;
                        await AddNugetPackage(nugetPackages, packageName, packageVersion);
                    }
                }
            }

            return nugetPackages;
        }
        public async Task<List<NuGetPackage>> ReadPackagesFromProjectFile(string projectPath)
        {
            var nugetPackages = new List<NuGetPackage>();
            var projectFile = XDocument.Load(projectPath);

            var packageReferences = projectFile.Descendants("PackageReference");
            foreach (var packageReference in packageReferences)
            {
                var packageName = packageReference.Attribute("Include")?.Value;
                var packageVersion = packageReference.Attribute("Version")?.Value;

                await AddNugetPackage(nugetPackages, packageName, packageVersion);
            }

            return nugetPackages;
        }
        private async Task AddNugetPackage(List<NuGetPackage> nugetPackages, string packageName, string packageVersion)
        {
            var currentVersion = VersionHelpers.TryParseNugetVersion(packageVersion, packageName, _sources);
            var latestVersion = await GetLatestNuGetVersion(packageName, currentVersion.IsPrerelease);
            var vulnerabilities = await _vulnerabilityHelpers.GetVulnerabilitiesAsync(packageName, currentVersion.ToNormalizedString());

            nugetPackages.Add(new NuGetPackage
            {
                Name = packageName,
                CurrentVersion = currentVersion,
                LatestVersion = latestVersion,
                Vulnerabilities = vulnerabilities
            });
        }
    }
}
