namespace Famoser.YoutubeExtractor.Portable.Enums
{
    public enum AudioType
    {
        Aac,
        Mp3,
        Vorbis,
        Oga,

        /// <summary>
        /// The audio type is unknown. This can occur if YoutubeExtractor is not up-to-date.
        /// </summary>
        Unknown
    }
}