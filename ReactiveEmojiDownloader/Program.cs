using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using Octokit.Reactive;

namespace ReactiveEmojiDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            string outputDirectory = args.Any()
                ? String.Join("", args)
                : Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);

            Debug.Assert(outputDirectory != null, "The output directory should not be null.`");

            var githubClient = new ObservableGitHubClient(new ProductHeaderValue("Haack-Reactive-Emoji-Downloader"));
            githubClient.Miscellaneous.GetEmojis()
                .Select(emoji => Observable.FromAsync(async () =>
                {
                    var path = Path.Combine(outputDirectory, emoji.Name + ".png");
                    await DownloadImage(emoji.Url, path);
                    return path;
                }))
                .Merge(4)
                .ToArray()
                .Wait();
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
