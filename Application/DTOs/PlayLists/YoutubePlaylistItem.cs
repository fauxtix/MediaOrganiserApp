namespace MediaOrganizerApp.Application.DTOs.PlayLists
{
    public class YoutubePlaylistItem
    {
        public string Id { get; set; }
        public string Title { get; set; }

        public override string ToString()
        {
            return Title; // o que aparece na ComboBox
        }
    }
}
