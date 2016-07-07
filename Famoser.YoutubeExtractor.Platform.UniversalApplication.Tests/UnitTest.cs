using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace Famoser.YoutubeExtractor.Platform.UniversalApplication.Tests
{
    [TestClass]
    public class TestAudioConversion
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            //IRandomAccessStream readStream = await file.OpenAsync(FileAccessMode.Read);
            //FFmpegMSS = FFmpegInteropMSS.CreateFFmpegInteropMSSFromStream(readStream, false, false);
            //// Pass MediaStreamSource to Media Element
            //mediaElement.SetMediaStreamSource(FFmpegMSS.GetMediaStreamSource());
        }
    }
}
