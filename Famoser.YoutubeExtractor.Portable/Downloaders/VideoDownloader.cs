using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Famoser.YoutubeExtractor.Portable.Helpers;
using Famoser.YoutubeExtractor.Portable.Models;

namespace Famoser.YoutubeExtractor.Portable.Downloaders
{
    /// <summary>
    /// Provides a method to download a video from YouTube.
    /// </summary>
    public class VideoDownloader : IDownloader
    {
        /// <summary>
        /// Occurs when the downlaod progress of the video file has changed.
        /// </summary>
        public event ProgressChangedEventHandler VideoDownloadProgressChanged;

        /// <summary>
        /// Occurs when the download is starts.
        /// </summary>
        public event EventHandler VideoDownloadStarted;

        /// <summary>
        /// Occurs when the download finished.
        /// </summary>
        public event EventHandler VideoDownloadFinished;

        /// <summary>
        /// Starts the video download.
        /// </summary>
        public virtual async Task<Stream> Execute(VideoInfo video)
        {
            VideoDownloadStarted?.Invoke(this, EventArgs.Empty);

            var res =  await HttpHelper.DownloadFileAsync(video.DownloadUrl, VideoDownloadProgressChanged, this, CancellationToken.None);
            res.Position = 0;
            
            VideoDownloadFinished?.Invoke(this, EventArgs.Empty);
            return res;
        }

        public virtual VideoInfo ChooseBest(IEnumerable<VideoInfo> video)
        {
            return video.OrderByDescending(v => v.Resolution).FirstOrDefault();
        }
    }
}