using Newtonsoft.Json;

namespace ProjectAnalyzer.CLI
{
    public class AppConfig
    {
        public List<string> NuGetSources { get; set; }
        public string SvnUsername { get; set; }
        public string SvnPassword { get; set; }
        public List<string> SVNRepositories { get; set; }
      

        public static AppConfig Load(string configFile)
        {
            string json = File.ReadAllText(configFile);
            return JsonConvert.DeserializeObject<AppConfig>(json);
        }
    }
    
}
