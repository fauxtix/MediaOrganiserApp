namespace MediaOrganizerApp.Application.DTOs
{
    public class GoogleBookInfo
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? PublishedDate { get; set; }
        public string? Thumbnail { get; set; }
        // Confidence score when provided by AI fallback (0.0 - 1.0)
        public double? Confidence { get; set; }
    }
}