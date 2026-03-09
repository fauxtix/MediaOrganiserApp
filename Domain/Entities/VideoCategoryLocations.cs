namespace MediaOrganizerApp.Domain.Entities
{
    public class VideoCategoryLocations
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public int IdCategoria { get; set; }
        public string Categoria { get; set; }
        public string Tempo { get; set; }
        public string AnoEdicao { get; set; }
        public string FileUrl { get; set; }
        public string Formato { get; set; }
        public string Autor { get; set; }
        public string CoverFile { get; set; }
        public string VideoId { get; set; }

    }
}
