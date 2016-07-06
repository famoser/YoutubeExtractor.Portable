using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Famoser.YoutubeExtractor.Portable.Downloaders;
using Famoser.YoutubeExtractor.Portable.Helpers;
using Famoser.YoutubeExtractor.Portable.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Famoser.YoutubeExtractor.Portable.Tests
{
    [TestClass]
    public class VideoDownloaderTest
    {
        [TestMethod]
        public async Task DownloadVideo()
        {
            IEnumerable<VideoInfo> videoInfos = await DownloadUrlResolver.GetDownloadUrlsAsync("https://www.youtube.com/watch?v=vxMxYgkUcdU");

            var downloader = new VideoDownloader();
            var stream = await downloader.Execute(videoInfos.FirstOrDefault());

            Assert.IsTrue(stream.Length > 0);

            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                File.WriteAllBytes("video.flv", ms.ToArray());
            }
        }

        [TestMethod]
        public async Task ChooseBestVideo()
        {
            IEnumerable<VideoInfo> videoInfos = await DownloadUrlResolver.GetDownloadUrlsAsync("https://www.youtube.com/watch?v=vxMxYgkUcdU");

            var downloader = new VideoDownloader();
            Assert.IsNotNull(downloader.ChooseBest(videoInfos));
        }

        [TestMethod]
        public async Task TestVideos()
        {
            var list = new List<string>()
            {
                "https://www.youtube.com/watch?v=vxMxYgkUcdU",
                "https://www.youtube.com/watch?v=Pyly3JtXoy4"
            };

            foreach (var item in list)
            {
                IEnumerable<VideoInfo> videoInfos = await DownloadUrlResolver.GetDownloadUrlsAsync(item);
                Assert.IsNotNull(videoInfos);
                Assert.IsTrue(videoInfos.Any());
            }
        }
    }
}
