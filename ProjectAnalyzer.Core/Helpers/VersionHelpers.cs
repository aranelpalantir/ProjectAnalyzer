namespace ProjectAnalyzer.Core.Helpers
{
    public class VersionHelpers
    {
        private static string NormalizeVersion(string version)
        {
            var parts = version.Split('.');

            while (parts.Length < 3)
            {
                version += ".0";
                parts = version.Split('.');
            }

            return version;
        }
        public static bool IsVersionUpdateAvailable(string currentVersion, string latestVersion)
        {
            var normalizedCurrentVersion = NormalizeVersion(currentVersion);
            var normalizedLatestVersion = NormalizeVersion(latestVersion);

            if (Version.TryParse(normalizedCurrentVersion, out var current) &&
                Version.TryParse(normalizedLatestVersion, out var latest))
            {
                return latest > current;
            }

            return false;
        }
    }
}
