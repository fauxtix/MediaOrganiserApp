namespace MediaOrganizerApp.Application.DTOs.Reporting
{
    public class CategoryReportModel
    {
        public int Id { get; set; }
        public string Descricao { get; set; }
        public int TipoCategoria { get; set; }
        public string DescricaoTipoCategoria { get; set; }
        public string TituloListagem { get; set; }
        public string Header1 { get; set; }
        public string Header2 { get; set; }
    }
}
