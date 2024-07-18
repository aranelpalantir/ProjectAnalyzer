using ProjectAnalyzer.Core.Helpers;

namespace ProjectAnalyzer.Core.Models;

public class NuGetPackage
{
    public string Name { get; set; }
    public string CurrentVersion { get; set; }
    public string LatestVersion { get; set; }
    public bool IsUpdateAvailable => VersionHelpers.IsVersionUpdateAvailable(CurrentVersion, LatestVersion);

}