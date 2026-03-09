namespace MediaOrganizerApp.Application.DTOs
{
    public class BooksViewModel
    {
        public int Id { get; set; }
        public string Autor { get; set; }
        public string Categoria { get; set; }
        public string Editora { get; set; }
        public string Formato { get; set; }
        public string Titulo { get; set; }
        public string Local { get; set; }
        public int AnoEdicao { get; set; }
        public int Paginas { get; set; }
        public bool Lido { get; set; }
        public int Contagem { get; set; }
    }
}
