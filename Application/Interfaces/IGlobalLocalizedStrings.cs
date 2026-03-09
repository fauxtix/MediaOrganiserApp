using Microsoft.Extensions.Configuration;

namespace MediaOrganizerApp.Application.Interfaces
{
    public interface IGlobalLocalizedStrings
    {
        string AnularRegisto { get; }
        string CampoRequerido { get; }
        string CriarRegisto { get; }
        string EdicaoUmRegisto { get; }
        string EscolhaCategoria { get; }
        string EspacoOcupado { get; }
        string ExtensaoInvalida { get; }
        string ExtrairDetalhesFicheiro { get; }
        string FicheiroNaoEscolhido { get; }
        string FicheiroNaoExisteDisco { get; }
        string FicheiroNaoExisteLocal { get; }
        string FicheirosDialog { get; }
        string GravarRegisto { get; }
        string ID_NaoNumerico { get; }
        string JaExisteBaseDados { get; }
        string NaoCompativel { get; }
        string NaoGravado { get; }
        string OperacaoCancelada { get; }
        string OperacaoSucesso { get; }
        string OperacaoTerminada { get; }
        string PastaUsada { get; }
        string PesquisaID { get; }
        string PreencherTitulo { get; }
        string PreenchimentoIncompleto { get; }
        string ProblemaGravacaoGrelha { get; }
        string Pronto { get; }
        string Registo { get; }
        string RegistoAnuladoSucesso { get; }
        string RegistoCriadoSucesso { get; }
        string RegistoDuplicado { get; }
        string RegistoGravadoSucesso { get; }
        string RegistoNaoAlterado { get; }
        string RegistoNaoAnulado { get; }
        string RegistoNaoCriado { get; }
        string RegistoNaoGravado { get; }
        string RegistosEncontrados { get; }
        string RegistoSeraApagadoDisco { get; }
        string SaidaComDadosAlterados { get; }
        string SemRegistos { get; }
        string Validacao { get; }

        void ReadStrings(IConfiguration config);
    }
}