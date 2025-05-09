using System;
using System.IO;
using System.Linq;
using System.Net;
using Octokit;

namespace Little_Hafiz
{
    public class GetAppUpdate
    {
        private string localVersion, onlineVersion;
        private Release release;

        public bool? CheckForUpdates()
        {
            try
            {
                localVersion = "v" + System.Windows.Forms.Application.ProductVersion;

                GitHubClient client = new GitHubClient(new ProductHeaderValue("little-hafiz"));
                release = client.Repository.Release.GetLatest("MohamedAshref371", "little-hafiz").Result;
                onlineVersion = release.TagName;

                return RemoveTrailingZerosFromVersion(localVersion) != RemoveTrailingZerosFromVersion(onlineVersion);
            }
            catch { return null; }
        }

        public bool GetTheUpdate()
        {
            if (onlineVersion is null) return false;
            try
            {
                if (release.Assets.Count == 0) return false;

                var asset = release.Assets.FirstOrDefault(ast => ast.Name.IndexOf("update", StringComparison.OrdinalIgnoreCase) >= 0);
                if (asset is null) return false;

                using (var webClient = new WebClient())
                    webClient.DownloadFile(asset.BrowserDownloadUrl, release.TagName + " " + asset.Name);

                return true;
            }
            catch { }
            return false;
        }

        private string RemoveTrailingZerosFromVersion(string version)
        {
            string[] parts = version.Split('.');

            while (parts.Length > 1 && parts[parts.Length - 1] == "0")
                Array.Resize(ref parts, parts.Length - 1);

            return string.Join(".", parts);
        }
    }
}
