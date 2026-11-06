using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using System.Threading;
using System.Threading.Tasks;

namespace MediaOrganizerApp.YouTubeNotificationApi.Services
{
    public class OAuthService
    {
        private static readonly string[] Scopes = { YouTubeService.Scope.YoutubeReadonly };

        public static async Task<UserCredential> GetUserCredentialAsync(string clientId, string clientSecret)
        {
            var clientSecrets = new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
            };

            string credPath = "token.json"; // Path to store the token

            var output = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                clientSecrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true)
            );

            return output;
        }
    }
}
