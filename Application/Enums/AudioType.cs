namespace MediaOrganizerApp.Application.Enums
{
    public enum AudioType
    {
        Aac,
        Mp3,
        Opus,
        Vorbis,

        /// <summary>
        /// The audio type is unknown. This can occur if YoutubeExtractor is not up-to-date.
        /// </summary>
        Unknown
    }
}