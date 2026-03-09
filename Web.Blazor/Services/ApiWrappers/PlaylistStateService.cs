using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Domain.Enums;
using Web.Blazor.Services.ApiWrappers;

public class PlaylistStateService
{
    private readonly PlaylistsClientService _client;

    public PlaylistDetailsDto? CurrentPlaylist { get; private set; }
    public int CurrentIndex { get; private set; } = -1;

    public PlaylistItemDto? CurrentItem =>
        CurrentPlaylist != null &&
        CurrentIndex >= 0 &&
        CurrentIndex < CurrentPlaylist.Items.Count
            ? CurrentPlaylist.Items[CurrentIndex]
            : null;

    public event Action? OnChange;

    public PlaylistStateService(PlaylistsClientService client)
    {
        _client = client;
    }

    public async Task LoadPlaylistAsync(int playlistId)
    {
        CurrentPlaylist = await _client.GetByIdAsync(playlistId);
        CurrentIndex = 0;
        Notify();
    }

    public async Task AddItemAsync(int playlistId, MediaType type, string url, string? title = null)
    {
        var dto = new PlaylistItemCreateDto
        {
            PlaylistId = playlistId,
            MediaType = type,
            ResourcePathOrUrl = url,
            Title = title
        };

        await _client.AddItemAsync(playlistId, dto);

        // recarrega para garantir estado atualizado
        await LoadPlaylistAsync(playlistId);
    }

    public void Next()
    {
        if (CurrentPlaylist == null || !CurrentPlaylist.Items.Any()) return;

        CurrentIndex = (CurrentIndex + 1) % CurrentPlaylist.Items.Count;
        Notify();
    }

    public void Previous()
    {
        if (CurrentPlaylist == null || !CurrentPlaylist.Items.Any()) return;

        CurrentIndex = (CurrentIndex - 1 + CurrentPlaylist.Items.Count) % CurrentPlaylist.Items.Count;
        Notify();
    }

    private void Notify() => OnChange?.Invoke();
}