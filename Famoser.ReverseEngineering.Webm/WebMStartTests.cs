using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Famoser.ReverseEngineering.Webm
{
    [TestClass]
    public class WebMStartTests
    {
        private const int ContentStart = 45;
        private const int ContentEnd = 0;
        private const int AacSmallStart = 4;
        private const int AacBigStart = 1024;
        private const int AacEnd = 0;
        [TestMethod]
        public void TestStartOfContainer()
        {
            var bytes1 = File.ReadAllBytes("testfiles/249.flv");
            var bytes2 = File.ReadAllBytes("testfiles/250.flv");
            var bytes3 = File.ReadAllBytes("testfiles/251.flv");
            var bytes4 = File.ReadAllBytes("testfiles/2_249.flv");
            var bytes5 = File.ReadAllBytes("testfiles/3_251.flv");

            //test start
            for (int i = 0; i < bytes1.Length && i < ContentStart; i++)
            {
                Assert.IsTrue(bytes1[i] == bytes2[i]);
                Assert.IsTrue(bytes1[i] == bytes3[i]);
                Assert.IsTrue(bytes1[i] == bytes4[i]);
                Assert.IsTrue(bytes1[i] == bytes5[i]);
            }

            Assert.IsFalse(bytes1[ContentStart] == bytes2[ContentStart]);
            
            //test end
            for (int i = 0; i < bytes1.Length && i < ContentEnd; i++)
            {
                Assert.IsTrue(bytes1[bytes1.Length - i] == bytes2[bytes2.Length - i]);
                Assert.IsTrue(bytes1[bytes1.Length - i] == bytes3[bytes3.Length - i]);
                Assert.IsTrue(bytes1[bytes1.Length - i] == bytes4[bytes4.Length - i]);
                Assert.IsTrue(bytes1[bytes1.Length - i] == bytes5[bytes5.Length - i]);
            }
        }

        [TestMethod]
        public void TestAacFiles()
        {
            var low1 = File.ReadAllBytes("testaacs/low.aac");
            var low2 = File.ReadAllBytes("testaacs/low_1.aac");
            var low3 = File.ReadAllBytes("testaacs/low_2.aac");
            var low4 = File.ReadAllBytes("testaacs/low_3.aac");
            var high1 = File.ReadAllBytes("testaacs/high.aac");
            var high2 = File.ReadAllBytes("testaacs/high_1.aac");

            //test start
            for (int i = 0; i < low1.Length && i < AacSmallStart; i++)
            {
                Assert.IsTrue(low1[i] == low3[i]);
                Assert.IsTrue(low1[i] == low2[i]);
                Assert.IsTrue(low1[i] == low4[i]);
            }

            //test start
            for (int i = 0; i < high1.Length && i < AacBigStart; i++)
            {
                Assert.IsTrue(high1[i] == high2[i]);
            }

            //test end
            for (int i = 1 ; i < low1.Length && i < AacEnd; i++)
            {
                Assert.IsTrue(low1[low1.Length - i] == low2[low2.Length - i]);
                Assert.IsTrue(low1[low1.Length - i] == low2[low2.Length - i]);
            }
        }

        [TestMethod]
        public void TestAudioExtraction()
        {
            var bytes1 = File.ReadAllBytes("testfiles/249.flv");
            var bytes2 = File.ReadAllBytes("testfiles/250.flv");
            var bytes3 = File.ReadAllBytes("testfiles/251.flv");
            var bytes4 = File.ReadAllBytes("testfiles/2_249.flv");
            var bytes5 = File.ReadAllBytes("testfiles/3_251.flv");

            if (!Directory.Exists("testfiles/extracted"))
                Directory.CreateDirectory("testfiles/extracted");

            File.WriteAllBytes("testfiles/extracted/249.aac", bytes1.Skip(ContentStart - AacSmallStart).ToArray());
            File.WriteAllBytes("testfiles/extracted/250.aac", bytes2.Skip(ContentStart - AacSmallStart).ToArray());
            File.WriteAllBytes("testfiles/extracted/251.aac", bytes3.Skip(ContentStart - AacSmallStart).ToArray());
            File.WriteAllBytes("testfiles/extracted/2_249.aac", bytes4.Skip(ContentStart - AacSmallStart).ToArray());
            File.WriteAllBytes("testfiles/extracted/3_251.aac", bytes5.Skip(ContentStart - AacSmallStart).ToArray());
        }

        [TestMethod]
        public void CompareTwoFiles()
        {
            var webmBytes = File.ReadAllBytes("testfiles/sample.webm");
            var aacBytes = File.ReadAllBytes("testfiles/sample.aac");

            var l =webmBytes.Length;
        }
    }
}
