using System.Xml.Linq;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using ProjectAnalyzer.Core.Models;

namespace ProjectAnalyzer.Core.Managers
{
    public class NugetManager
    {
        private readonly List<string> _sources;
        private readonly Dictionary<string, string> _versionCache = new Dictionary<string, string>();

        public NugetManager(List<string> sources)
        {
            _sources = sources;
        }
        public async Task<string?> GetLatestNuGetVersion(string packageName)
        {
            if (_versionCache.TryGetValue(packageName, out var cachedVersion))
            {
                return cachedVersion;
            }

            string? latestVersion = null;
            foreach (var source in _sources)
            {
                var repository = Repository.Factory.GetCoreV3(source);
                var metadataResource = await repository.GetResourceAsync<MetadataResource>();

                var versions = await metadataResource.GetVersions(packageName, new SourceCacheContext(), NullLogger.Instance, CancellationToken.None);

                var sourceLatestVersion = versions
                    .Where(r => r.IsPrerelease == false)
                    .MaxBy(v => v)?
                    .ToNormalizedString();
                if (!string.IsNullOrEmpty(sourceLatestVersion))
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
                        var latestVersion = await GetLatestNuGetVersion(packageName);

                        nugetPackages.Add(new NuGetPackage
                        {
                            Name = packageName,
                            CurrentVersion = packageVersion,
                            LatestVersion = latestVersion
                        });
                    }
                }
            }

            return nugetPackages;
        }
        public async Task<List<NuGetPackage>> ReadPackagesFromProjectFile(string projectPath)
        {
            var packages = new List<NuGetPackage>();
            var projectFile = XDocument.Load(projectPath);

            var packageReferences = projectFile.Descendants("PackageReference");
            foreach (var packageReference in packageReferences)
            {
                var packageName = packageReference.Attribute("Include")?.Value;
                var packageVersion = packageReference.Attribute("Version")?.Value;
                var latestVersion = await GetLatestNuGetVersion(packageName);

                packages.Add(new NuGetPackage
                {
                    Name = packageName,
                    CurrentVersion = packageVersion,
                    LatestVersion = latestVersion
                });
            }

            return packages;
        }
    }
}
