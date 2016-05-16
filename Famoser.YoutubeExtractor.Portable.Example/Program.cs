using System;
using System.Collections.Generic;
using Famoser.YoutubeExtractor.Portable.Helpers;
using Famoser.YoutubeExtractor.Portable.Models;

namespace Famoser.YoutubeExtractor.Portable.Example
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Run();

            Console.ReadLine();
        }

        private static async void Run()
        {
            IEnumerable<VideoInfo> videoInfos = await DownloadUrlResolver.GetDownloadUrlsAsync("https://www.youtube.com/watch?v=rKTUAESacQM");

            foreach (VideoInfo videoInfo in videoInfos)
            {
                Console.WriteLine(videoInfo.DownloadUrl);
                Console.WriteLine();
            }
        }
    }
}