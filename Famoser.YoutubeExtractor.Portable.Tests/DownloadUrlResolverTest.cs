using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Famoser.YoutubeExtractor.Portable.Helpers;
using Famoser.YoutubeExtractor.Portable.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Famoser.YoutubeExtractor.Portable.Tests
{
    /// <summary>
    /// Small series of unit tests for DownloadUrlResolver. Run these with NUnit.
    /// </summary>
    /// 
    [TestClass]
    public class DownloadUrlResolverTest
    {
        [TestMethod]
        public void UrlsAreNormalizedCorrectly()
        {
            var targetUrl = "http://youtube.com/watch?v=12345";
            string[] urls = {
                "http://youtube.com/watch?v=12345",
                "http://youtu.be/12345",
                "http://m.youtube.com/?v=12345",
                "http://m.youtube.com/?v=12345&hawd"
            };

            foreach (var url in urls)
            {
                string normalizedUrl;
                Assert.IsTrue(DownloadUrlResolver.TryNormalizeYoutubeUrl(url, out normalizedUrl));
                Assert.AreEqual(targetUrl, normalizedUrl);
            }
        }

        [TestMethod]
        public void UrlsAreIgnoredCorrectly()
        {
            string[] urls = {
               "http://notAYouTubeUrl.com"
            };

            foreach (var url in urls)
            {
                string normalizedUrl;
                Assert.IsFalse(DownloadUrlResolver.TryNormalizeYoutubeUrl(url, out normalizedUrl));
            }
        }

        [TestMethod]
        public async Task CanExtractLinks()
        {
            IEnumerable<VideoInfo> videoInfos = await DownloadUrlResolver.GetDownloadUrlsAsync("https://www.youtube.com/watch?v=rKTUAESacQM");
            Assert.IsTrue(videoInfos != null);
            Assert.IsTrue(videoInfos.Any());
        }
    }
}
