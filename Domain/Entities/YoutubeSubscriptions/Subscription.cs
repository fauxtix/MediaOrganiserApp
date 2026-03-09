using System;

namespace MediaOrganizerApp.Domain.Entities.YoutubeSubscriptions
{
    public class Subscription
    {
        public string Id { get; set; } // Unique identifier for the subscription

        public string Title { get; set; } // Title of the subscription
        public string Description { get; set; } // Description of the subscription

        public DateTime PublishedAt { get; set; } // Date the subscription was published
        public string ChannelId { get; set; } // ID of the subscribed channel
        public string ChannelTitle { get; set; } // Title of the subscribed channel

        public string ThumbnailUrl { get; set; } // URL of the thumbnail image

        public string Kind { get; set; } // Type of resource
        public string ETag { get; set; } // ETag of the resource
    }
}
