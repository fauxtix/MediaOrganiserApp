using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Enums;
using MediaOrganizerApp.Domain.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Notifications;
using Syncfusion.Blazor.Popups;
using Syncfusion.Blazor.Spinner;
using Web.Blazor.Services.ApiWrappers;

namespace Web.Blazor.Pages.BaseComponents
{
    public class PlaylistsBase : ComponentBase
    {
        [Inject] public PlaylistsClientService PlaylistsService { get; set; }
        [Inject] public IMediaServiceWrapper MediaService { get; set; }
        [Inject] public IStringLocalizer<App> localizerApp { get; set; }
        [Inject] public NavigationManager Navigation { get; set; } = default!;


        protected SfGrid<PlaylistDto> gridObj;
        protected SfToast ToastObj;
        protected SfSpinner SpinnerObj;

        public DialogEffect Effects = DialogEffect.Zoom;

        protected string ToastTitle = "";
        protected string ToastContent = "";
        protected string ToastCssClass = "";
        public int ToastTimeOut { get; set; } = 1500;

        protected List<PlaylistDto> playLists = new();
        protected List<PlaylistItemDto> playListItems = new();

        protected PlaylistDto SelectedPlaylist = new();
        protected PlaylistItemDto SelectedPlaylistItem = new();

        protected List<PlaylistItemDto> DialogItems { get; set; } = new();

        protected PlaylistDetailsDto? CurrentDetails;

        protected bool EditDialogVisibility { get; set; } = false;
        protected bool DetailsDialogVisibility { get; set; } = false;
        protected bool DeleteRecordConfirmVisibility { get; set; } = false;
        protected bool DeletePlaylistItemConfirmVisibility { get; set; } = false;
        protected bool AddMediaDialogVisibility { get; set; } = false;
        protected bool AddItemsDialogVisibility { get; set; } = false;

        protected string ErrorTitle { get; set; } = string.Empty;
        protected List<string> ValidationMessages { get; set; } = new();
        protected bool ErrorVisibility { get; set; } = false;

        protected bool IsEditMode { get; set; } = false;
        protected int EditingPlaylistId { get; set; }
        protected PlaylistUpdateDto EditModel { get; set; } = new();

        protected List<MediaVM> mediaFiles = new();

        protected bool isLoading = true;

        protected Dictionary<int, List<PlaylistItemDto>> PlaylistItemsCache = [];

        protected HashSet<int> LoadingPlaylists = [];

        protected string message = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await LoadPlaylistsAsync();
            await OpenAddMediaDialog();
        }

        protected async Task LoadPlaylistsAsync()
        {
            isLoading = true;
            try
            {
                var result = await PlaylistsService.GetAllAsync();
                playLists = (result ?? Enumerable.Empty<PlaylistDto>())
                    .OrderByDescending(p => p.UpdatedAt)
                    .ToList();
            }
            catch (Exception ex)
            {
                ShowError("Erro ao carregar playlists.", ex.Message);
            }
            finally
            {
                isLoading = false;
            }
        }

        public async Task OnToolbarClick(Syncfusion.Blazor.Navigations.ClickEventArgs args)
        {
            if (args.Item.Id == "Clear")
            {
                await gridObj.SearchAsync("");
            }
        }

        public void RowDragStartingHandler(RowDragStartingEventArgs<PlaylistItemDto> Args)
        {
            message = "RowDragStart event triggered";
        }
        public void RowDroppingHandler(RowDroppingEventArgs<PlaylistItemDto> Args)
        {
            message = "RowDropping event triggered";
        }
        public async Task RowDroppedHandler(RowDroppedEventArgs<PlaylistItemDto> args)
        {
            var playlistId = args?.Data?.FirstOrDefault()?.PlaylistId ?? 0;
            if (playlistId <= 0)
                return;

            // Prefer using the cached datasource (it represents what the grid is bound to).
            // Fallback to args.Data if the cache isn't available.
            if (!PlaylistItemsCache.TryGetValue(playlistId, out var items) || items is null || items.Count == 0)
                items = args.Data?.ToList() ?? new List<PlaylistItemDto>();

            if (items.Count == 0)
                return;

            // IMPORTANT: Persist the order by writing SortOrder according to current list order
            for (var i = 0; i < items.Count; i++)
                items[i].SortOrder = i + 1;

            var ok = await PlaylistsService.UpdateSortOrderAsync(items);
            if (!ok)
            {
                ShowError("Erro", "API returned non-success while updating SortOrder.");
                return;
            }

            // Keep cache consistent (and force UI refresh if needed)
            PlaylistItemsCache[playlistId] =
                (items ?? Enumerable.Empty<PlaylistItemDto>())
                    .OrderBy(i => i.SortOrder)
                    .ToList();
            await InvokeAsync(StateHasChanged);
        }

        public async Task OnCommandClicked(CommandClickEventArgs<PlaylistDto> args)
        {
            var data = args.RowData;
            SelectedPlaylist = data;

            if (args.CommandColumn.Type == CommandButtonType.Edit)
            {
                await BeginEditPlaylistAsync(data);
            }

            if (args.CommandColumn.Type == CommandButtonType.Delete)
            {
                DeleteRecordConfirmVisibility = true;
                StateHasChanged();
            }

            if (args.CommandColumn.Type == CommandButtonType.None &&
                args.CommandColumn.ButtonOption?.IconCss?.Contains("fa fa-play") == true)
            {
                // PlaylistDto.MediaType: 2 = Audio, else = Video (per your UI template)
                if (data.MediaType != MediaType.Audio)
                {
                    await ShowToastAsync("This option is only available for Audio playlists.", "e-toast-info");
                    return;
                }

                Navigation.NavigateTo($"/media/playlists/{data.Id}/player");
                return;
            }

            if (args.CommandColumn.Type == CommandButtonType.None &&
                args.CommandColumn.ButtonOption?.IconCss?.Contains("oi-add") == true)
            {
                await OpenAddItemsDialog(data);
            }
        }

        protected async Task ShowDetailsAsync(int playlistId)
        {
            try
            {
                if (SpinnerObj is not null)
                    await SpinnerObj.ShowAsync();

                CurrentDetails = await PlaylistsService.GetByIdAsync(playlistId);
                DetailsDialogVisibility = true;
            }
            catch (Exception ex)
            {
                ShowError("Erro ao carregar detalhes da playlist.", ex.Message);
            }
            finally
            {
                if (SpinnerObj is not null)
                    await SpinnerObj.HideAsync();
            }
        }

        public void BeginCreatePlaylist()
        {
            IsEditMode = false;
            EditingPlaylistId = 0;
            EditModel = new PlaylistUpdateDto
            {
                Name = "",
                Description = "",
                MediaType = MediaType.Audio
            };
            EditDialogVisibility = true;
        }

        protected async Task BeginEditPlaylistAsync(PlaylistDto dto)
        {
            IsEditMode = true;
            EditingPlaylistId = dto.Id;
            EditModel = new PlaylistUpdateDto
            {
                Name = dto.Name,
                Description = dto.Description,
                MediaType = dto.MediaType
            };
            EditDialogVisibility = true;
            await Task.CompletedTask;
        }

        public async Task SavePlaylistAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EditModel.Name))
                {
                    ShowError("Validação", "Nome é obrigatório.");
                    return;
                }

                if (IsEditMode)
                {
                    var ok = await PlaylistsService.UpdateAsync(EditingPlaylistId, EditModel);
                    if (!ok)
                        throw new System.Exception("Erro ao atualizar playlist.");
                }
                else
                {
                    var createDto = new PlaylistCreateDto
                    {
                        Name = EditModel.Name,
                        Description = EditModel.Description,
                        MediaType = EditModel.MediaType,                        
                    };
                    int? createdId = await PlaylistsService.CreateAsync(createDto);
                    if (createdId <= 0)
                        throw new System.Exception("Erro ao criar playlist.");
                }

                EditDialogVisibility = false;
                await LoadPlaylistsAsync();
                await ShowToastAsync(localizerApp["btnSalvar"], "e-toas-success");
                StateHasChanged();
            }
            catch (System.Exception ex)
            {
                ShowError("Erro", ex.Message);
            }
        }

        public void CancelEditPlaylist()
        {
            EditDialogVisibility = false;
        }

        public async Task ConfirmDeleteYes()
        {
            DeleteRecordConfirmVisibility = false;
            try
            {
                await PlaylistsService.DeleteAsync(SelectedPlaylist.Id);
                await LoadPlaylistsAsync();
                await ShowToastAsync(localizerApp["RecordDeleted"], "e-toast-success");
            }
            catch (Exception ex)
            {
                ShowError("Erro ao apagar playlist.", ex.Message);
            }
        }

        public async Task ConfirmDeletePlaylistItemYes()
        {
            DeletePlaylistItemConfirmVisibility = false;
            try
            {
                await PlaylistsService.DeleteItemAsync(SelectedPlaylistItem.PlaylistId, SelectedPlaylistItem.Id);

                // Atualizar o cache da playlist
                var updatedItems = await PlaylistsService.GetItemsAsync(SelectedPlaylistItem.PlaylistId);
                PlaylistItemsCache[SelectedPlaylistItem.PlaylistId] = updatedItems?.ToList() ?? [];
                // Atualizar a interface
                StateHasChanged();

                await ShowToastAsync(localizerApp["RecordDeleted"], "e-toast-success");
            }
            catch (Exception ex)
            {
                ShowError("Erro ao apagar playlist.", ex.Message);
            }
        }


        public void CloseDialogDelete() => DeleteRecordConfirmVisibility = false;

        public void CloseDetails()
        {
            DetailsDialogVisibility = false;
            CurrentDetails = null;
        }

        protected List<PlaylistItemDto> GetPlayListItems_ById(int playListId)
        {
            playListItems = (PlaylistsService.GetItemsAsync(playListId).GetAwaiter().GetResult()).ToList();
            return playListItems;
        }
        public async Task OpenAddMediaDialog()
        {
            try
            {
                mediaFiles = (await MediaService
                    .GetFilesByMediaType(AppDefinitions.Query_TipoMedia.Audio))
                    .ToList();
                AddMediaDialogVisibility = true;
            }
            catch (Exception ex)
            {
                ShowError("Erro ao carregar arquivos de mídia.", ex.Message);
            }
        }

        public async Task OpenAddItemsDialog(PlaylistDto playlist)
        {
            SelectedPlaylist = playlist;
            mediaFiles = (await MediaService.
                GetFilesByMediaType(AppDefinitions.Query_TipoMedia.Audio))
                .ToList();
            AddItemsDialogVisibility = true;
        }

        protected PlaylistItemCreateDto NewItem { get; set; } = new();
        protected bool AddItemDialogVisibility { get; set; } = false;

        protected void OpenAddItemDialog(int playlistId)
        {
            SelectedPlaylist = playLists.FirstOrDefault(p => p.Id == playlistId)!; // garante que existe
            if (PlaylistItemsCache.TryGetValue(playlistId, out var items))
                DialogItems = items.ToList();
            else
                DialogItems = new List<PlaylistItemDto>();

            AddItemDialogVisibility = true;
        }

        protected async Task AddItemToPlaylist()
        {
            try
            {
                await PlaylistsService.AddItemAsync(SelectedPlaylist.Id, NewItem);
                AddItemDialogVisibility = false;
                //await ShowDetailsAsync(SelectedPlaylist.Id); // Atualiza o detalhe
            }
            catch (Exception ex)
            {
                ShowError("Erro ao adicionar item", ex.Message);
            }
        }

        public void CloseAddMediaDialog() => AddMediaDialogVisibility = false;
        public void CloseAddItemsDialog() => AddItemsDialogVisibility = false;

        protected async Task ShowToastAsync(string message, string cssClass)
        {
            ToastTitle = localizerApp["Info"];
            ToastContent = message;
            ToastCssClass = cssClass;
            StateHasChanged();
            await ToastObj.ShowAsync();
        }

        protected async Task HandleFilesSelected(List<PlaylistItemDto> selectedFiles)
        {
            try
            {
                if (selectedFiles != null && selectedFiles.Any())
                {
                    foreach (var selectedFile in selectedFiles)
                    {
                        var newItem = new PlaylistItemCreateDto
                        {
                            PlaylistId = SelectedPlaylist.Id,
                            MediaId = selectedFile.MediaId,
                            MediaType = selectedFile.MediaType,
                            ResourcePathOrUrl = selectedFile.ResourcePathOrUrl,
                            SortOrder = selectedFile.SortOrder == 0 ? 1 : selectedFile.SortOrder + 1,
                            Title = selectedFile.Title
                        };

                        // Adicionar o item à playlist
                        await PlaylistsService.AddItemAsync(SelectedPlaylist.Id, newItem);
                    }

                    // Atualizar o cache da playlist
                    var updatedItems = await PlaylistsService.GetItemsAsync(SelectedPlaylist.Id);
                    PlaylistItemsCache[SelectedPlaylist.Id] = updatedItems?.ToList() ?? new List<PlaylistItemDto>();

                    // Atualizar o detalhe da grid
                    StateHasChanged();

                    // Fechar o diálogo
                    AddItemDialogVisibility = false;
                }
            }
            catch (Exception ex)
            {
                ShowError("Erro ao adicionar arquivo", ex.Message);
            }
        }

        protected async Task OnDetailDataBound(DetailDataBoundEventArgs<PlaylistDto> args)
        {
            var playlistId = args.Data.Id;

            if (PlaylistItemsCache.ContainsKey(playlistId))
                return;

            try
            {
                LoadingPlaylists.Add(playlistId);
                StateHasChanged();

                var items = await PlaylistsService.GetItemsAsync(playlistId);

                PlaylistItemsCache[playlistId] =
                    items?.ToList() ?? new();
            }
            catch (Exception ex)
            {
                ShowError("Erro ao carregar itens", ex.Message);
                PlaylistItemsCache[playlistId] = new();
            }
            finally
            {
                LoadingPlaylists.Remove(playlistId);
                StateHasChanged();
            }
        }
        protected void CloseAddItemDialog()
        {
            AddItemDialogVisibility = false;
        }

        private void ShowError(string title, string message)
        {
            ErrorTitle = title;
            ValidationMessages = new() { message };
            ErrorVisibility = true;
        }


        protected void OnDetailDeleteCommandClicked(CommandClickEventArgs<PlaylistItemDto> args)
        {
            var data = args.RowData;

            if (args.CommandColumn.Type == CommandButtonType.Delete)
            {

                if (data == null) return;

                SelectedPlaylistItem = data;
                DeletePlaylistItemConfirmVisibility = true;
            }
        }
    }
}