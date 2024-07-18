namespace ProjectAnalyzer.Core.Models;

public class ProjectInfo
{
    public string ProjectPath { get; set; }
    public string DotNetVersion { get; set; }
    public List<NuGetPackage> Packages { get; set; }
}