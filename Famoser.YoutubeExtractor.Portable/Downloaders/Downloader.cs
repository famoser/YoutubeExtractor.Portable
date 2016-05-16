using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Famoser.YoutubeExtractor.Portable.Models;

namespace Famoser.YoutubeExtractor.Portable.Downloaders
{
    /// <summary>
    /// Provides the base class for the <see cref="AudioDownloader"/> and <see cref="VideoDownloader"/> class.
    /// </summary>
    public interface IDownloader
    {
        /// <summary>
        /// Starts the work of the <see cref="IDownloader"/>.
        /// </summary>
        Task<Stream> Execute(VideoInfo video);

        /// <summary>
        /// Starts the work of the <see cref="IDownloader"/> for the best VideoInfo it can find.
        /// </summary>
        VideoInfo ChooseBest(IEnumerable<VideoInfo> video);
    }
}