using ProjectAnalyzer.Core.Helpers;
using ProjectAnalyzer.Core.Managers;
using ProjectAnalyzer.Core.Models;

namespace ProjectAnalyzer.Core
{
    public class NugetPackagesAnalyzer
    {
        private readonly NugetManager _nugetManager;
        private readonly SvnManager _svnManager;

        public NugetPackagesAnalyzer(
            string svnUser,
            string svnPassword,
            NugetManager nugetManager)
        {
            _nugetManager = nugetManager;
            _svnManager = new SvnManager(svnUser, svnPassword);
        }

        public async Task<List<ProjectInfo>> AnalyzeProjects(string svnUrl)
        {
            var projectInfos = new List<ProjectInfo>();
            var projectPaths = _svnManager.GetProjectPaths(svnUrl);

            foreach (var projectPath in projectPaths)
            {
                var tempDirectory = ProjectHelpers.CreateTempDirectory();
                var localProjectPath = Path.Combine(tempDirectory, Path.GetFileName(projectPath));
                var packagesConfigFileName = "packages.config";
                var localPackagesConfigPath = Path.Combine(tempDirectory, packagesConfigFileName);

                _svnManager.CheckoutFile(projectPath, localProjectPath);
                var packagesConfigPath = ProjectHelpers.GetDirectoryUrl(projectPath) + $"/{packagesConfigFileName}";
                _svnManager.CheckoutFile(packagesConfigPath, localPackagesConfigPath);

                var projectInfo = new ProjectInfo
                {
                    ProjectPath = projectPath,
                    DotNetVersion = ProjectHelpers.GetDotNetVersion(localProjectPath) ?? ""
                };

                projectInfo.Packages = await GetNuGetPackages(localPackagesConfigPath, localProjectPath);
                projectInfos.Add(projectInfo);

                Directory.Delete(tempDirectory, true);
            }

            return projectInfos;
        }

        private async Task<List<NuGetPackage>> GetNuGetPackages(string localPackagesConfigPath, string localProjectPath)
        {
            if (File.Exists(localPackagesConfigPath))
                return await _nugetManager.ReadPackagesFromConfigFile(localPackagesConfigPath);
            else
                return await _nugetManager.ReadPackagesFromProjectFile(localProjectPath);
        }
    }
}
