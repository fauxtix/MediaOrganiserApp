using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MediaOrganizerApp.Application.DTOs.PlayLists
{
    public class YouTubeSearchResponse
    {
        [JsonPropertyName("items")]
        public List<Item> Items { get; set; }

        public class Item
        {
            [JsonPropertyName("id")]
            public Id Id { get; set; }

            [JsonPropertyName("snippet")]
            public Snippet Snippet { get; set; }
        }

        public class Id
        {
            [JsonPropertyName("playlistId")]
            public string PlaylistId { get; set; }
        }

        public class Snippet
        {
            [JsonPropertyName("title")]
            public string Title { get; set; }
        }
    }
}
