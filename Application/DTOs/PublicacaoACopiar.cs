using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace MediaOrganizerApp.Application.DTOs
{
    public class PublicacaoACopiar
    {
        public int PubID { get; set; }
        public string Publicacao { get; set; }
        public int CodGenero { get; set; }
        public OpcoesRegisto opcao { get; set; } = OpcoesRegisto.Gravar;
    }
}
