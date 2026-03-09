namespace MediaOrganizerApp.Domain.Entities
{
    public class ImageCategoryLocations
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public int IdCategoria { get; set; }
        public string Categoria { get; set; }
        public string AnoEdicao { get; set; }
        public string ImagemCapa { get; set; }
    }
}
