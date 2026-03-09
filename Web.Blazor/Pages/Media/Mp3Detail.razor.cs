using Ganss.Xss;
using MediaOrganizerApp.Application.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TagLib;

namespace Web.Blazor.Pages.Media
{
    public partial class Mp3Detail
    {
        [Parameter] public MediaVM SelectedRecord { get; set; }
        [Parameter] public string mediaFileName { get; set; }
        [Inject] ILogger<App> _logger { get; set; }

        private Dictionary<string, object> htmlAttribute { get; set; } = new Dictionary<string, object>()
    {
            {"rows", "12" }
    };

        protected string mp3Path = "";
        protected string mp3Title = "";
        protected string mp3Album = "";

        protected string[] mp3Genres;
        protected int mp3EditionYear = 0;
        protected string mp3Pusblisher = "";
        protected string mp3FirstArtist = "";
        protected string[] mp3Composers;
        protected string mp3Lenght = "";
        protected bool PassedHere = false;

        protected List<string> sbWikiResponses = new();


        protected override async Task OnParametersSetAsync()
        {
            mp3Title = "";
            mp3Album = "";

            mp3EditionYear = 0;
            mp3Pusblisher = "";
            mp3FirstArtist = "";
            mp3Lenght = "";

            if (!string.IsNullOrEmpty(mediaFileName))
            {
                mp3Path = $"{SelectedRecord.FilePath}";
            }

            GetMp3Metadata();
            string searchString = SelectedRecord.Autor;
            try
            {
                sbWikiResponses.Clear();
                PassedHere = false;
                HttpClient client = new();
                var streamTask = client.GetStreamAsync($"https://pt.wikipedia.org/w/api.php?action=query&gpslimit=50&list=search&srsearch={searchString}&format=json");
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

        private void GetMp3Metadata()
        {
            if (string.IsNullOrEmpty(mp3Path)) return;


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
