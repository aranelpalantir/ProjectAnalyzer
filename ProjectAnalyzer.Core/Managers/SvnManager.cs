using SharpSvn;

namespace ProjectAnalyzer.Core.Managers
{
    public class SvnManager
    {
        private readonly string _svnUser;
        private readonly string _svnPassword;

        public SvnManager(
            string svnUser,
            string svnPassword)
        {
            _svnUser = svnUser;
            _svnPassword = svnPassword;
        }
        public void CheckoutFile(string projectUrl, string localPath)
        {
            using (var client = new SvnClient())
            {
                client.Authentication.DefaultCredentials = new System.Net.NetworkCredential(_svnUser, _svnPassword);
                var targetUri = new Uri(projectUrl);
                var args = new SvnExportArgs { Depth = SvnDepth.Empty, ThrowOnError = false };
                client.Export(targetUri, localPath, args);
            }
        }
        public IEnumerable<string> GetProjectPaths(string svnUrl)
        {
            var paths = new List<string>();
            using (var client = new SvnClient())
            {
                var listArgs = new SvnListArgs { Depth = SvnDepth.Infinity };
                client.GetList(new Uri(svnUrl), listArgs, out var list);

                foreach (var item in list)
                {
                    if (item.Path.EndsWith(".csproj"))
                    {
                        paths.Add(item.Uri.ToString());
                    }
                }
            }
            return paths;
        }
    }
}
