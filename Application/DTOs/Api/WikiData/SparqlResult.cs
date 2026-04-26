using System.Collections.Generic;

namespace MediaOrganizerApp.Application.DTOs.Api.WikiData
{
    public class SparqlResult
    {
        public SparqlResults results { get; set; }
    }

    public class SparqlResults
    {
        public List<SparqlBinding> bindings { get; set; }
    }

    public class SparqlBinding
    {
        public SparqlValue book { get; set; }
        public SparqlValue bookLabel { get; set; }
        public SparqlValue image { get; set; }
        public SparqlValue date { get; set; }
    }

    public class SparqlValue
    {
        public string value { get; set; }
    }

    public class WikidataSearchResult
    {
        public List<WikidataSearchItem> search { get; set; }
    }
    public class WikidataSearchItem
    {
        public string id { get; set; }        // QID (ex: Q42)
        public string label { get; set; }     // nome do autor
        public string description { get; set; }
    }
}
