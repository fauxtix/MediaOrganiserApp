using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Domain.Entities.Author_Api_Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Links
    {
        public string self { get; set; }
        public string author { get; set; }
        public string next { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public Type type { get; set; }
    }

    //public class Type_Work
    //{
        
    //    public string key { get; set; }
    //}

    public class Author2
    {
        public string key { get; set; }
    }

    public class Author
    {
        public Type type { get; set; }
        public Author author { get; set; }
    }

    //public class Created_Work
    //{
    //    public string type { get; set; }
    //    public DateTime value { get; set; }
    //}

    //public class LastModified_Work
    //{
    //    public string type { get; set; }
    //    public DateTime value { get; set; }
    //}

    public class Excerpt
    {
        public Author author { get; set; }
        public string comment { get; set; }
        public string excerpt { get; set; }
    }

    public class FirstSentence
    {
        public string type { get; set; }
        public string value { get; set; }
    }

    public class Entry
    {
        public string title { get; set; }
        public List<int> covers { get; set; }
        public string key { get; set; }
        public List<Author> authors { get; set; }
        public Type type { get; set; }
        public int latest_revision { get; set; }
        public int revision { get; set; }
        public Created created { get; set; }
        public LastModified last_modified { get; set; }
        public object description { get; set; }
        public List<Link> links { get; set; }
        public List<string> subject_places { get; set; }
        public List<string> subjects { get; set; }
        public List<string> subject_people { get; set; }
        public List<Excerpt> excerpts { get; set; }
        public List<string> subject_times { get; set; }
        public FirstSentence first_sentence { get; set; }
        public string first_publish_date { get; set; }
        public string location { get; set; }
    }

    public class AuthorWorksModel
    {
        public Links links { get; set; }
        public int size { get; set; }
        public List<Entry> entries { get; set; }
    }


}
