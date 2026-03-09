using BoldReports.RDL.DOM;
using Ganss.Xss;
using MediaOrganizerApp.Application.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Syncfusion.Blazor.Spinner;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Web.Blazor.Services.Common;
using TagLib;

namespace Web.Blazor.Pages.BaseComponents
{
    public class MediaPlayer : ComponentBase, IDisposable
    {
        [Inject] IJSRuntime js { get; set; }
        [Inject] IWebHostEnvironment hostingEnvironment { get; set; }
        [Inject] ILogger<App> _logger { get; set; }

        [Parameter] public MediaVM SelectedRecord { get; set; }
        [Parameter] public string mediaFileName { get; set; }
        [Parameter] public string mediaExtension { get; set; }
        [Parameter] public string command { get; set; }

        protected string mediaFile { get; set; }
        protected string videoFile { get; set; }

        protected bool Mp3HasImage { get; set; }
        protected System.Drawing.Image Mp3AlbumCover;
        protected string coverPictureData = "";
        protected string mp3Path = "";
        protected string mp3CoverFileSaved = "";
        protected string mp3CoverServerAddress = "";
        protected string mp3CoverServerFolder = "";

        protected string mp3Title = "";
        protected string mp3Album = "";

        protected string[] mp3Genres;
        protected int mp3EditionYear = 0;
        protected string mp3Pusblisher = "";
        protected string mp3FirstArtist = "";
        protected string[] mp3Composers;
        protected string mp3Lenght = "";

        protected string mp3FullPath;

        protected Mp3Handler jsMp3Handler;

        protected bool hidden = true;

        protected bool PassedHere = false;

        //Dictionary<VideoEvents, VideoStateOptions> options = new Dictionary<VideoEvents, VideoStateOptions>();
        //VideoState videoState;
        //Dictionary<VideoEvents, int> eventCounter = new Dictionary<VideoEvents, int>();
        protected List<string> sbWikiResponses = new();

        protected SfSpinner SpinnerObj;

        protected string Mp4_FileToPlay = "";



        protected override void OnInitialized()
        {
            jsMp3Handler = new Mp3Handler(js);

            //foreach (var item in (VideoEvents[])(Enum.GetValues(typeof(VideoEvents))))
            //{
            //    if (item != VideoEvents.NotSet)
            //    {
            //        eventCounter[item] = 0;
            //    }
            //}
            //var option = new VideoStateOptions() { All = true };
            //options[VideoEvents.Abort] = option;
            //options[VideoEvents.CanPlay] = option;
            //options[VideoEvents.CanPlayThrough] = option;
            //options[VideoEvents.DurationChange] = option;
            //options[VideoEvents.Emptied] = option;
            //options[VideoEvents.Ended] = option;
            //options[VideoEvents.Error] = option;
            //options[VideoEvents.LoadedData] = option;
            //options[VideoEvents.LoadedMetadata] = option;
            //options[VideoEvents.LoadStart] = option;
            //options[VideoEvents.Pause] = option;
            //options[VideoEvents.Play] = option;
            //options[VideoEvents.Playing] = option;
            //options[VideoEvents.Progress] = option;
            //options[VideoEvents.RateChange] = option;
            //options[VideoEvents.Seeked] = option;
            //options[VideoEvents.Seeking] = option;
            //options[VideoEvents.Stalled] = option;
            //options[VideoEvents.Suspend] = option;
            //options[VideoEvents.TimeUpdate] = option;
            //options[VideoEvents.VolumeChange] = option;
            //options[VideoEvents.Waiting] = option;
        }



        protected override async Task OnParametersSetAsync()
        {
            mp3CoverServerAddress = "/images/headphones.jpg";
            mp3Title = "";
            mp3Album = "";

            mp3EditionYear = 0;
            mp3Pusblisher = "";
            mp3FirstArtist = "";
            mp3Lenght = "";
            mp3FullPath = "";

            if (!string.IsNullOrEmpty(mediaFileName))
            {
                mp3Path = $"{SelectedRecord.FilePath}";
                mp3FullPath = Path.Combine("Files", "Uploads", "Media", mediaFileName); ;
            }
            else
            {
                Console.WriteLine("been here...");
            }

            if (!System.IO.File.Exists(SelectedRecord.FilePath))
            {
                mediaFileName = SelectedRecord.FileUrl;
            }

            if (!string.IsNullOrEmpty(mediaFileName) && mediaExtension.ToLower() == ".mp3")
            {

                GetMp3Metadata();

                string searchString = SelectedRecord.Autor;
                try
                {
                    sbWikiResponses.Clear();
                    PassedHere = false;
                    HttpClient client = new();
                    var streamTask = client.GetStreamAsync($"https://pt.wikipedia.org/w/api.php?action=query&list=search&srsearch={searchString}&format=json");
                    Root output = await JsonSerializer.DeserializeAsync<Root>(await streamTask);
                    int wikiEntries = output.query.search.Count;
                    if (output is not null && wikiEntries > 0)
                    {
                        if (!PassedHere)
                        {
                            var sanitizer = new HtmlSanitizer();
                            for (int i = 0; i < wikiEntries; i++)
                            {
                                if (i > 3)
                                    continue;

                                string actual = sanitizer.Sanitize(output.query.search[i].snippet);
                                actual = actual.Replace("<span>", "").Replace("</span>", "").Replace("&nbsp;", "").Replace("&amp;", "");
                                sbWikiResponses.Add(actual);
                            }
                        }

                        PassedHere = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }
            else if (mediaExtension.ToLower() == ".mp4")
            {
                Mp4_FileToPlay = "";

                if (!string.IsNullOrEmpty(SelectedRecord.FilePath) && System.IO.File.Exists(SelectedRecord.FilePath))
                {
                    Mp4_FileToPlay = $"files/uploads/media/{mediaFileName}";
                }
                else if (!string.IsNullOrEmpty(SelectedRecord.FilePath) && !System.IO.File.Exists(SelectedRecord.FilePath))
                {
                    if (!string.IsNullOrEmpty(SelectedRecord.FileUrl))
                    {
                        Mp4_FileToPlay = DecodeYoutubeUrl(SelectedRecord.FileUrl);
                    }
                }
                else if (!string.IsNullOrEmpty(SelectedRecord.FileUrl))
                {
                    Mp4_FileToPlay = DecodeYoutubeUrl(SelectedRecord.FileUrl);
                }
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                if (mediaExtension.ToLower() == ".mp4")
                {
                    // No need to check permissions for video playback
                    await js.InvokeVoidAsync("initializeVideoPlayer");
                }
            }
        }

        protected async Task PlayAudio()
        {
            string fName = Path.GetFileNameWithoutExtension(mp3Path);
            mp3CoverServerFolder = Path.Combine(hostingEnvironment.WebRootPath, "images", "Audiocovers");
            mp3CoverFileSaved = $@"{mp3CoverServerFolder}\{fName}.jpg";

            var mediaDirectory = Path.Combine(hostingEnvironment.WebRootPath, "files", "uploads", "Media");

            if (!Directory.Exists(mediaDirectory))
            {
                Directory.CreateDirectory(mediaDirectory);
            }
            var mediaFile = Path.Combine(mediaDirectory, mediaFileName);
            if (!System.IO.File.Exists(mediaFile) && !string.IsNullOrEmpty(mediaFileName))
            {
                System.IO.File.Copy(SelectedRecord.FilePath, mediaFile);
            }

            await Task.Delay(1000);
            await jsMp3Handler.SelectedMp3Changed(mp3FullPath);

        }

        private void GetMp3Metadata()
        {
            if (string.IsNullOrEmpty(mp3Path)) return;

            mp3CoverServerAddress = "/images/headphones.jpg";

            TagLib.File tagFile = TagLib.File.Create(mp3Path);

            if (!string.IsNullOrEmpty(tagFile.Tag.Title))
            {
                mp3Title = tagFile.Tag.Title.Replace("_", " ");
                mp3Album = tagFile.Tag.Album;
                mp3Genres = tagFile.Tag.Genres;

                mp3EditionYear = (int)tagFile.Tag.Year;
                mp3Pusblisher = tagFile.Tag.Publisher;
                mp3FirstArtist = tagFile.Tag.FirstPerformer;
                mp3Composers = tagFile.Tag.Composers;
            }
            else
            {
                mp3CoverServerAddress = "/images/Headphones.jpg";
            }
        }

        protected override void OnAfterRender(bool firstRender)
        {
            //if (firstRender)
            //{
            //    mediaFileName = "";
            //    mp3FullPath = "";
            //    StateHasChanged();
            //}
        }

        protected async Task StopAudio()
        {
            await jsMp3Handler.StopAudio();
        }

        protected async Task DisposeAudio()
        {
            await jsMp3Handler.DisposeAudio();
            //mediaFileName = "";
            //mp3FullPath = "";
            jsMp3Handler.Dispose();
            StateHasChanged();
        }


        protected async Task PauseAudio()
        {
            await jsMp3Handler.PauseAudio();
        }

        private static void OnPlaybackFinished(object sender, EventArgs e)
        {
            Console.WriteLine("Playback finished");
        }


        private string DecodeYoutubeUrl(string url)
        {
            string output = url.Replace("watch?v=", "embed/");
            return output;
        }

        public void Dispose()
        {
            jsMp3Handler?.Dispose();
        }

        public class Continue
        {
            public int sroffset { get; set; }
            public string @continue { get; set; }
        }

        public class Searchinfo
        {
            public int totalhits { get; set; }
        }

        public class Search
        {
            public int ns { get; set; }
            public string title { get; set; }
            public int pageid { get; set; }
            public int size { get; set; }
            public int wordcount { get; set; }
            public string snippet { get; set; }
            public DateTime timestamp { get; set; }
        }

        public class Query
        {
            public Searchinfo searchinfo { get; set; }
            public List<Search> search { get; set; }
        }

        public class Root
        {
            public string batchcomplete { get; set; }
            public Continue @continue { get; set; }
            public Query query { get; set; }
        }
    }
}
