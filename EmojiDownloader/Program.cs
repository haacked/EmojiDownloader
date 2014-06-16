using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Octokit;

namespace EmojiDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            string outputDirectory = args.Any()
                ? String.Join("", args)
                : Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);

            Debug.Assert(outputDirectory != null, "The output directory should not be null.`");

            Task.Run(async () =>
            {
                var githubClient = new GitHubClient(new ProductHeaderValue("Haack-Emoji-Downloader"));
                var emojis = await githubClient.Miscellaneous.GetEmojis();
                foreach (var emoji in emojis)
                {
                    string emojiFileName = Path.Combine(outputDirectory, emoji.Name + ".png");
                    await DownloadImage(emoji.Url, emojiFileName);
                }

            }).Wait();
        }

        public static async Task DownloadImage(Uri url, string filePath)
        {
            Console.WriteLine("Downloading " + filePath);

            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    var response = await httpClient.SendAsync(request);

                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    using (var writeStream = new FileStream(filePath, System.IO.FileMode.Create))
                    {
                        await responseStream.CopyToAsync(writeStream);
                    }     
                }
            }
        }
    }
}
