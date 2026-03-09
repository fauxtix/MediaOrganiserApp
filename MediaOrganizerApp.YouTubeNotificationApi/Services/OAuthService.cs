using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using MediaOrganizerApp.YouTubeNotificationApi.Settings;
using System.Threading;
using System.Threading.Tasks;

public class OAuthService
{
    private static readonly string[] Scopes = { Google.Apis.YouTube.v3.YouTubeService.Scope.YoutubeReadonly };
    private static readonly string ApplicationName = Keys.ApplicationName;

    public static async Task<UserCredential> GetUserCredentialAsync()
    {
        var clientSecrets = new ClientSecrets
        {
            ClientId = Keys.ClientId,
            ClientSecret = Keys.ClientSecret
        };

        // Path to store OAuth tokens
        string credPath = "token.json";

        try
        {
            // Initiate authorization request
            return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                clientSecrets,
                Scopes,
                "user", // This can be any unique identifier for the user
                CancellationToken.None,
                new FileDataStore(credPath, true) // Save and load tokens to/from file
            );
        }
        catch (Exception ex)
        {
            // Handle any exceptions and provide feedback
            Console.WriteLine($"An error occurred: {ex.Message}");
            throw;
        }
    }
}
