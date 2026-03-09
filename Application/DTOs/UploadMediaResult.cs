using Microsoft.AspNetCore.Components;

namespace MediaOrganizerApp.Application.DTOs
{
    public class UploadMediaResult
    {
        public bool Flag { get; set; }
        public string Message { get; set; } 
        public string ClassName { get; set; }
    }
}
