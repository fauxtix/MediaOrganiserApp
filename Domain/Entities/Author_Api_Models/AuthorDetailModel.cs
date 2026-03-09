using System;
using System.Collections.Generic;

namespace MediaOrganizerApp.Domain.Entities.Author_Api_Models
{
    public class Type
    {
        public string key { get; set; }
    }

    public class Link
    {
        public string url { get; set; }
        public Type type { get; set; }
        public string title { get; set; }
    }

    public class Bio
    {
        public string type { get; set; }
        public string value { get; set; }
    }

    public class RemoteIds
    {
        public string viaf { get; set; }
        public string wikidata { get; set; }
        public string isni { get; set; }
    }

    public class Created
    {
        public string type { get; set; }
        public DateTime value { get; set; }
    }

    public class LastModified
    {
        public string type { get; set; }
        public DateTime value { get; set; }
    }

    public class AuthorInfoDetail
    {
        public string birth_date { get; set; }
        public List<int> photos { get; set; }
        public string title { get; set; }
        public List<Link> links { get; set; }
        public Bio bio { get; set; }
        public List<string> source_records { get; set; }
        public List<string> alternate_names { get; set; }
        public RemoteIds remote_ids { get; set; }
        public string entity_type { get; set; }
        public string wikipedia { get; set; }
        public string name { get; set; }
        public string personal_name { get; set; }
        public string key { get; set; }
        public Type type { get; set; }
        public int latest_revision { get; set; }
        public int revision { get; set; }
        public Created created { get; set; }
        public LastModified last_modified { get; set; }
    }
}
