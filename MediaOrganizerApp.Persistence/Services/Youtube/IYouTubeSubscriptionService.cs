using Google.Apis.YouTube.v3.Data;
using MediaOrganizerApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Persistence.Services.Youtube
{
    public interface IYouTubeSubscriptionService
    {
        Task<IList<YouTubeSubscription>> GetAndSaveSubscriptionsAsync();
        Task<IList<Subscription>> GetSubscriptionsAsync();
        Task<IList<YouTubeVideo>> GetVideosByChannelIdAsync(string channelId);
        Task SaveSubscriptionsAsync();
    }
}