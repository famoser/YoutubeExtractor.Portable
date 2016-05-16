using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Famoser.YoutubeExtractor.Portable.Models;

namespace Famoser.YoutubeExtractor.Portable.Downloaders
{
    /// <summary>
    /// Provides a method to download a video and extract its audio track.
    /// </summary>
    public class AudioDownloader : VideoDownloader
    {
        /// <summary>
        /// Occurs when the progress of the audio extraction has changed.
        /// </summary>
        public event ProgressChangedEventHandler AudioExtractionProgressChanged;

        /// <summary>
        /// Occurs when the extraction is starts.
        /// </summary>
        public event EventHandler AudioExtractionStarted;

        /// <summary>
        /// Occurs when the extraction finished.
        /// </summary>
        public event EventHandler AudioExtractionFinished;

        /// <summary>
        /// Downloads the video from YouTube and then extracts the audio track out if it.
        /// </summary>
        public override async Task<Stream> Execute(VideoInfo videoInfo)
        {
            var stream = await base.Execute(videoInfo);

            AudioExtractionStarted?.Invoke(this, EventArgs.Empty);
            
            stream = await ExtractAudio(stream);
            stream.Position = 0;

            AudioExtractionFinished?.Invoke(this, EventArgs.Empty);
            return stream;
        }

        private Task<Stream> ExtractAudio(Stream stream)
        {
            var flvFile = new FlvFile(stream);
            {
                flvFile.ConversionProgressChanged += (sender, args) =>
                {
                    AudioExtractionProgressChanged?.Invoke(this, args);
                };
                
                return flvFile.ExtractStreams();
            }
        }

        public override VideoInfo ChooseBest(IEnumerable<VideoInfo> video)
        {
            return video.Where(v => v.CanExtractAudio).OrderByDescending(d => d.AudioBitrate).FirstOrDefault();
        }
    }
}
