using NuGet.Versioning;

namespace ProjectAnalyzer.Core.Models;

public class NuGetPackage
{
    public string Name { get; set; }
    public NuGetVersion? CurrentVersion { get; set; }
    public NuGetVersion? LatestVersion { get; set; }
    public bool IsUpdateAvailable => LatestVersion > CurrentVersion;
    public List<Vulnerability> Vulnerabilities { get; set; } = new();
}