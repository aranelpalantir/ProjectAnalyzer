using System.Xml.Linq;

namespace ProjectAnalyzer.Core.Helpers
{
    public static class ProjectHelpers
    {
        public static string GetDirectoryUrl(string fileUrl)
        {
            var uri = new Uri(fileUrl);
            return uri.GetLeftPart(UriPartial.Authority) + uri.AbsolutePath.Substring(0, uri.AbsolutePath.LastIndexOf('/'));
        }

        public static string CreateTempDirectory()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }

        public static string? GetDotNetVersion(string projectPath)
        {
            var projectFile = XDocument.Load(projectPath);

            XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";
            var targetFrameworkVersion = projectFile.Descendants(ns + "TargetFrameworkVersion").FirstOrDefault()?.Value;
            var targetFramework = projectFile.Descendants("TargetFramework").FirstOrDefault()?.Value;
            return targetFrameworkVersion ?? targetFramework;
        }
    }
}