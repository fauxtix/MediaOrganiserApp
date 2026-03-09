namespace MediaOrganizerApp.Application.DTOs.Reporting
{
    public class MediaReportModel
    {
        public int IdMedia { get; set; }
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public int TipoMedia { get; set; }
        public string Formato_Media { get; set; }
        public string Genero { get; set; }
    }
}
