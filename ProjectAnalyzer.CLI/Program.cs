using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ProjectAnalyzer.Core;
using ProjectAnalyzer.Core.Managers;
using ProjectAnalyzer.Core.Models;

namespace ProjectAnalyzer.CLI
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var serviceScope = host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                var appConfig = services.GetRequiredService<IOptions<AppConfig>>().Value;
                var analyzer = services.GetRequiredService<NugetPackagesAnalyzer>();

                await AnalyzeRepositories(appConfig, analyzer);
            }
        }
        static async Task AnalyzeRepositories(AppConfig appConfig, NugetPackagesAnalyzer analyzer)
        {
            foreach (var svnRepo in appConfig.SVNRepositories)
            {
                Console.WriteLine($"Svn Repo: {svnRepo}");

                var projectInfos = await analyzer.AnalyzeProjects(svnRepo);

                foreach (var projectInfo in projectInfos)
                {
                    DisplayProjectInfo(projectInfo);
                }
            }
        }
        static void DisplayProjectInfo(ProjectInfo projectInfo)
        {
            Console.WriteLine($"Project Path: {projectInfo.ProjectPath}");
            Console.WriteLine($"Target .NET Version: {projectInfo.DotNetVersion}");
            Console.WriteLine("NuGet Packages:");

            DisplayPackages("Packages with Updates Available:", projectInfo.Packages.Where(p => p.IsUpdateAvailable), ConsoleColor.Red);
            DisplayPackages("Packages Up-to-date:", projectInfo.Packages.Where(p => !p.IsUpdateAvailable), ConsoleColor.Green);

            Console.WriteLine("------------------------------");
        }
        static void DisplayPackages(string header, IEnumerable<NuGetPackage> packages, ConsoleColor color)
        {
            if (!packages.Any()) return;

            Console.WriteLine(header);
            foreach (var package in packages)
            {
                Console.ForegroundColor = color;
                Console.WriteLine($"Package: {package.Name}, Current Version: {package.CurrentVersion}, Latest Version: {package.LatestVersion}");
                Console.ResetColor();

                if (package.Vulnerabilities.Any())
                {
                    DisplayVulnerabilities(package.Vulnerabilities);
                }
            }
        }
        static void DisplayVulnerabilities(IEnumerable<Vulnerability> vulnerabilities)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(" Vulnerabilities:");
            foreach (var vulnerability in vulnerabilities)
            {
                Console.WriteLine($" - {vulnerability.Severity}: {vulnerability.Summary}");
            }
            Console.ResetColor();
        }
        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false);
                    config.AddUserSecrets<Program>();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<AppConfig>(hostContext.Configuration);
                    services.AddSingleton<NugetManager>(sp =>
                    {
                        var appConfig = sp.GetRequiredService<IOptions<AppConfig>>().Value;
                        return new NugetManager(appConfig.NuGetSources, appConfig.GitHubToken);
                    });
                    services.AddSingleton<NugetPackagesAnalyzer>(sp =>
                    {
                        var appConfig = sp.GetRequiredService<IOptions<AppConfig>>().Value;
                        var nugetManager = sp.GetRequiredService<NugetManager>();
                        return new NugetPackagesAnalyzer(appConfig.SvnUsername, appConfig.SvnPassword, nugetManager);
                    });
                });
    }
}
