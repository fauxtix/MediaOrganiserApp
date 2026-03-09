namespace MediaOrganizerApp.Application.DTOs
{
    public class FileCheckResult
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string DigitalPath { get; set; }
        public string FullPath { get; set; }
        public int Edition { get; set; }
        public int Pages { get; set; }
        public string ImagemCapa { get; set; } = string.Empty;
    }
}
