namespace ProjectAnalyzer.CLI
{
    public class AppConfig
    {
        public List<string> NuGetSources { get; set; }
        public string SvnUsername { get; set; }
        public string SvnPassword { get; set; }
        public List<string> SVNRepositories { get; set; }
        public string GitHubToken { get; set; }
    }
}