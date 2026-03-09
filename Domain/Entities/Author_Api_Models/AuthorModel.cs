using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Domain.Entities.Author_Api_Models
{
    // Author general model (to get the author's key to get personal detail - bellow)
    public class Doc
    {
        public string key { get; set; }
        public List<string> text { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public List<string> alternate_names { get; set; }
        public string birth_date { get; set; }
        public string top_work { get; set; }
        public int work_count { get; set; }
        public List<string> top_subjects { get; set; }
        public long _version_ { get; set; }
    }

    public class AuthorModel
    {
        public int numFound { get; set; }
        public int start { get; set; }
        public bool numFoundExact { get; set; }
        public List<Doc> docs { get; set; }
    }

}
