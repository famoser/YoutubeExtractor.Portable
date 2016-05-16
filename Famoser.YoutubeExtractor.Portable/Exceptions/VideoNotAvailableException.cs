﻿using System;

namespace Famoser.YoutubeExtractor.Portable.Exceptions
{
    /// <summary>
    /// The exception that is thrown when the video is not available for viewing.
    /// This can happen when the uploader restricts the video to specific countries.
    /// </summary>
    public class VideoNotAvailableException : Exception
    {
        public VideoNotAvailableException(string message)
            : base(message)
        { }
    }
}