namespace MediaOrganizerApp.Domain.Entities
{
    public class QueryMostUsed
    {
        public string Campo { get; set; }
        public string Tabela { get; set; }
        public string Chave { get; set; }
        public string Descricao { get; set; }
        public string TabelaAusar { get; set; } = "publicacao";
    }
}
