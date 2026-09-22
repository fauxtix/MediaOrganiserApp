namespace MediaOrganizerApp.Application.DTOs
{
    /// <summary>
    /// Data-quality / health counters for the dashboard (GET /api/dashboard/health).
    /// </summary>
    public class DashboardHealthDto
    {
        public int PubsSemCapa { get; set; }
        public int PubsSemPaginas { get; set; }
        public int AutoresSemPublicacoes { get; set; }
        public int EditorasSemPublicacoes { get; set; }
        public int CategoriasSemPublicacoes { get; set; }
        public int FormatosSemPublicacoes { get; set; }
        public int LocaisArquivoSemPublicacoes { get; set; }
        public int TitulosDuplicados { get; set; }
    }
}