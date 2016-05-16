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
    public class AudioDownloaderTest
    {
        [TestMethod]
        public async Task DownloadAudio()
        {
            IEnumerable<VideoInfo> videoInfos = await DownloadUrlResolver.GetDownloadUrlsAsync("https://www.youtube.com/watch?v=vxMxYgkUcdU");
            var videoInfo = videoInfos.FirstOrDefault(vi => vi.CanExtractAudio);

            var downloader = new AudioDownloader();
            var stream = await downloader.Execute(videoInfo);

            Assert.IsTrue(stream.Length > 0);

            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = await stream.ReadAsync(buffer,0,buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                File.WriteAllBytes("video.mp3", ms.ToArray());
            }
        }

        [TestMethod]
        public async Task ChooseBestVideo()
        {
            IEnumerable<VideoInfo> videoInfos = await DownloadUrlResolver.GetDownloadUrlsAsync("https://www.youtube.com/watch?v=vxMxYgkUcdU");

            var downloader = new AudioDownloader();
            Assert.IsNotNull(downloader.ChooseBest(videoInfos));
            Assert.IsTrue(downloader.ChooseBest(videoInfos).CanExtractAudio);
        }
    }
}
