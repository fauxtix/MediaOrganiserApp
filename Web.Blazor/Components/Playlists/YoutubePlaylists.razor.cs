using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.DTOs.PlayLists;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Inputs;
using Web.Blazor.Services.YouTube;

namespace Web.Blazor.Components.Playlists
{
    public partial class YoutubePlaylists
    {
        [Inject] public IStringLocalizer<App> localizerApp { get; set; }
        [Inject] IJSRuntime JS { get; set; }
        [Inject] public YoutubeService YoutubeService { get; set; } = default!;

        protected List<YoutubePlaylistItem> playlistList = new();

        private readonly Dictionary<string, List<YoutubePlaylistItem>> _playlistCache = [];

        protected string? selectedGenre;
        protected string? selectedPlaylistId;
        protected bool ShowTitleSearch { get; set; } = false;
        protected string? youtubeSearchError;
        protected string TitleSearch { get; set; } = string.Empty;

        protected List<YoutubeVideoDto>? youtubeVideos;

        protected bool SpinnerVisibleProperty { get; set; } = false;

        protected Dictionary<string, string> genreMap = new()
        {
            { "Pop", "pop music playlist" },
            { "Punk", "punk music playlist" },
            { "Rock", "rock classics playlist" },
            { "Progressive Rock", "progressive rock classics playlist" },
            { "Hard Rock", "hard rock classics playlist" },
            { "Metal Rock", "metal rock classics playlist" },
            { "Blues", "blues classics playlist" },
            { "Jazz", "jazz classics playlist" },
            { "Jazz Rock", "jazz rock classics playlist" },
            { "Soul", "soul music playlist" }
        };

        // ✅ REGRA DE NEGÓCIO PARA O BOTÃO
        protected bool CanSearch =>
            !string.IsNullOrWhiteSpace(selectedGenre) &&
            !string.IsNullOrWhiteSpace(selectedPlaylistId);

        // FILTRO DE VÍDEOS


        //protected IEnumerable<YoutubeVideoDto> FilteredYoutubeVideos =>
        //    string.IsNullOrWhiteSpace(TitleSearch)
        //        ? youtubeVideos ?? Enumerable.Empty<YoutubeVideoDto>()
        //        : youtubeVideos.Where(v =>
        //            v.Title.Contains(TitleSearch, StringComparison.OrdinalIgnoreCase));
        protected IEnumerable<YoutubeVideoDto> FilteredYoutubeVideos =>
            string.IsNullOrWhiteSpace(TitleSearch)
                ? youtubeVideos ?? Enumerable.Empty<YoutubeVideoDto>()
                : youtubeVideos?.Where(v =>
                    v.Title.Contains(TitleSearch, StringComparison.OrdinalIgnoreCase))
                  ?? Enumerable.Empty<YoutubeVideoDto>();

        // 🔎 GET VIDEOS
        protected async Task GetYoutubePlaylist()
        {
            youtubeSearchError = null;
            youtubeVideos = null;

            if (string.IsNullOrWhiteSpace(selectedPlaylistId))
            {
                youtubeSearchError = "Informe o nome ou URL da playlist.";
                return;
            }

            try
            {
                SpinnerVisibleProperty = true;

                ResetVideos();

                youtubeVideos =
                    await YoutubeService.GetVideosFromPlaylist(selectedPlaylistId);
                if (youtubeVideos is null)
                    youtubeSearchError = localizerApp["TituloNoDataToShow"];

                ShowTitleSearch = youtubeVideos != null && youtubeVideos.Any();
            }
            catch (Exception ex)
            {
                youtubeSearchError = $"Erro ao pesquisar playlist: {ex.Message}";
            }
            finally
            {
                SpinnerVisibleProperty = false;
            }
        }

        // 🎵 GENRE CHANGE
        protected async Task OnGenreChange(ChangeEventArgs<string, string> args)
        {
            try
            {
                SpinnerVisibleProperty = true;

                selectedGenre = args.Value;

                ResetVideos();

                selectedPlaylistId = null;

                if (string.IsNullOrWhiteSpace(selectedGenre))
                    return;

                // CACHE HIT
                if (_playlistCache.TryGetValue(selectedGenre, out var cachedPlaylists))
                {
                    playlistList = cachedPlaylists.ToList();
                    await InvokeAsync(StateHasChanged);
                    return;
                }

                // CACHE MISS
                var query = genreMap[selectedGenre];

                playlistList = await YoutubeService.GetPlaylistsAsync(query);

                _playlistCache[selectedGenre] = playlistList.ToList();

                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                youtubeSearchError = $"Erro ao pesquisar playlist: {ex.Message}";
            }
            finally
            {
                SpinnerVisibleProperty = false;
            }
        }        // 📀 PLAYLIST CHANGE
        protected void OnPlaylistChange(ChangeEventArgs<string, YoutubePlaylistItem> args)
        {
            try
            {
                selectedPlaylistId = args.Value;
                ResetVideos();
            }
            catch (Exception ex)
            {
                youtubeSearchError = $"Erro ao processar playlist: {ex.Message}";
            }
        }

        protected void OnTitleInput(InputEventArgs args)
        {
            TitleSearch = args.Value?.ToString() ?? string.Empty;
        }

        protected void ClearTitleSearch()
        {
            TitleSearch = string.Empty;
        }

        private void ResetVideos()
        {
            youtubeVideos = null;
            TitleSearch = string.Empty;
        }

        // ⬆ SCROLL TOP
        protected async Task ScrollToTop()
        {
            await JS.InvokeVoidAsync(
                "window.scrollTo",
                new { top = 0, behavior = "smooth" });
        }
    }
}