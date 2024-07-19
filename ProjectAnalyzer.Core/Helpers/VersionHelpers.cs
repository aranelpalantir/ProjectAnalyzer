using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace ProjectAnalyzer.Core.Helpers
{
    public static class VersionHelpers
    {
        private static string NormalizeVersion(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
                return "0.0.0";

            var parts = version.Split('.');

            while (parts.Length < 3)
            {
                version += ".0";
                parts = version.Split('.');
            }

            return version;
        }
        private static async Task<string> ResolveLatestVersionAsync(string packageId, string version, List<string> sources)
        {
            NuGetVersion latestVersion = null;

            foreach (var source in sources)
            {
                var repository = Repository.Factory.GetCoreV3(source);
                var metadataResource = await repository.GetResourceAsync<MetadataResource>();

                var versions = await metadataResource.GetVersions(packageId, new SourceCacheContext(), NullLogger.Instance, CancellationToken.None);

                var versionRange = VersionRange.Parse(version.Replace("*", "0"));

                var resolvedVersion = versions.Where(v => versionRange.Satisfies(v)).OrderByDescending(v => v).FirstOrDefault();

                if (resolvedVersion != null)
                {
                    latestVersion = resolvedVersion;
                    break;
                }
            }

            return latestVersion?.ToNormalizedString() ?? "0.0.0";
        }
        public static NuGetVersion? TryParseNugetVersion(string version, string packageName, List<string> sources)
        {
            if (version.Contains("*"))
            {
                version = ResolveLatestVersionAsync(packageName, version, sources).Result;
            }
            return NuGetVersion.TryParse(NormalizeVersion(version), out var normalizedCurrentVersion) ? normalizedCurrentVersion : null;
        }
        public static string ConvertToNuGetVersionRange(string versionRangeString)
        {
            if (string.IsNullOrWhiteSpace(versionRangeString))
                return "";

            var parts = versionRangeString.Split(new[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 4)
            {
                var minVersion = parts[1];
                var maxVersion = parts[3];
                var includeMinVersion = false;
                var includeMaxVersion = false;

                if (parts[0] == ">=")
                    includeMinVersion = true;
                if (parts[2] == "<=")
                    includeMaxVersion = true;

                return $"{(includeMinVersion ? "[" : "(")}{minVersion}, {maxVersion}{(includeMaxVersion ? "]" : ")")}";
            }
            else if (parts.Length == 2)
            {
                var maxVersion = parts[1];
                var includeMaxVersion = parts[0] == "<=";

                return $"[0.0.0, {maxVersion}{(includeMaxVersion ? "]" : ")")}";
            }

            return versionRangeString;
        }
    }
}