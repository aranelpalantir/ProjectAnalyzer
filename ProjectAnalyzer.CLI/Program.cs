using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ProjectAnalyzer.Core;
using ProjectAnalyzer.Core.Managers;

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

                var nugetManager = new NugetManager(appConfig.NuGetSources);
                var analyzer = new NugetPackagesAnalyzer(appConfig.SvnUsername, appConfig.SvnPassword, nugetManager);

                foreach (var svnRepo in appConfig.SVNRepositories)
                {
                    Console.WriteLine($"Svn Repo: {svnRepo}");

                    var projectInfos = await analyzer.AnalyzeProjects(svnRepo);

                    foreach (var projectInfo in projectInfos)
                    {
                        Console.WriteLine($"Project: {projectInfo.ProjectPath}");
                        Console.WriteLine($"DotNet Version: {projectInfo.DotNetVersion}");
                        Console.WriteLine($"Nuget Packages:");

                        foreach (var package in projectInfo.Packages)
                        {
                            Console.ForegroundColor = package.IsUpdateAvailable ? ConsoleColor.Red : ConsoleColor.Green;
                            Console.WriteLine($"Package: {package.Name}, Current Version: {package.CurrentVersion}, Latest Version: {package.LatestVersion}");
                            Console.ResetColor();
                        }

                        Console.WriteLine("------------------------------");
                    }
                }
            }
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
                    services.AddTransient<NugetPackagesAnalyzer>();
                });
    }
}
