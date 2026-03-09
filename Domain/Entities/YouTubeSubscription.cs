using System.ComponentModel.DataAnnotations;

namespace MediaOrganizerApp.Domain.Entities
{
    public class YouTubeSubscription
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string ChannelId { get; set; }
    }
}
