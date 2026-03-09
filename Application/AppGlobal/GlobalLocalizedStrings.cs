using MediaOrganizerApp.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace MediaOrganizerApp.Application.AppGlobal
{
    public class GlobalLocalizedStrings : IGlobalLocalizedStrings
    {
        public string RegistoNaoAnulado { get; private set; }
        public string RegistoNaoAlterado { get; private set; }
        public string RegistoNaoCriado { get; private set; }
        public string OperacaoCancelada { get; private set; }
        public string ProblemaGravacaoGrelha { get; private set; }
        public string PreenchimentoIncompleto { get; private set; }
        public string FicheirosDialog { get; private set; }
        public string SemRegistos { get; private set; }
        public string Pronto { get; private set; }
        public string EspacoOcupado { get; private set; }
        public string ID_NaoNumerico { get; private set; }
        public string PesquisaID { get; private set; }
        public string SaidaComDadosAlterados { get; private set; }
        public string FicheiroNaoExisteLocal { get; private set; }
        public string EdicaoUmRegisto { get; private set; }
        public string OperacaoSucesso { get; private set; }
        public string RegistoSeraApagadoDisco { get; private set; }
        public string PastaUsada { get; private set; }
        public string NaoCompativel { get; private set; }
        public string ExtrairDetalhesFicheiro { get; private set; }
        public string PreencherTitulo { get; private set; }
        public string Validacao { get; private set; }
        public string CampoRequerido { get; private set; }
        public string EscolhaCategoria { get; private set; }
        public string Registo { get; private set; }
        public string JaExisteBaseDados { get; private set; }
        public string RegistoDuplicado { get; private set; }
        public string NaoGravado { get; private set; }
        public string FicheiroNaoEscolhido { get; private set; }

        public string ExtensaoInvalida { get; private set; }
        public string OperacaoTerminada { get; private set; }
        public string RegistosEncontrados { get; private set; }
        public string FicheiroNaoExisteDisco { get; private set; }
        public string RegistoCriadoSucesso { get; private set; }
        public string RegistoGravadoSucesso { get; private set; }
        public string RegistoAnuladoSucesso { get; private set; }
        public string RegistoNaoGravado { get; private set; }
        public string GravarRegisto { get; private set; }
        public string CriarRegisto { get; private set; }
        public string AnularRegisto { get; private set; }

        public void ReadStrings(IConfiguration config)
        {
            string sLinguagem = config["Language"]; // _configuration Configurations.ApplicationConfiguration.Language;
            CultureInfo ci = new CultureInfo(sLinguagem);

            Assembly assembly = Assembly.Load("PubliSoft");
            ResourceManager rm = new ResourceManager("Publications.Resources.langres", assembly);
            RegistoNaoAnulado = rm.GetString("RegistoNaoAnulado", ci);
            RegistoNaoAlterado = rm.GetString("RegistoNaoAlterado", ci);
            RegistoNaoCriado = rm.GetString("RegistoNaoCriado", ci);
            OperacaoCancelada = rm.GetString("OperacaoCancelada", ci);
            ProblemaGravacaoGrelha = rm.GetString("ProblemaGravacaoGrelha", ci);
            PreenchimentoIncompleto = rm.GetString("PreenchimentoIncompleto", ci);
            FicheirosDialog = rm.GetString("RegistoNaoCriado", ci);
            SemRegistos = rm.GetString("SemRegistos", ci);
            Pronto = rm.GetString("Pronto", ci);
            EspacoOcupado = rm.GetString("RegistoNaoCriado", ci);
            ID_NaoNumerico = rm.GetString("ID_NaoNumerico", ci);
            PesquisaID = rm.GetString("PesquisaID", ci);
            SaidaComDadosAlterados = rm.GetString("SaidaComDadosAlterados", ci);
            FicheiroNaoExisteLocal = rm.GetString("FicheiroNaoExisteLocal", ci);
            EdicaoUmRegisto = rm.GetString("EdicaoUmRegisto", ci);
            OperacaoSucesso = rm.GetString("OperacaoSucesso", ci);
            RegistoSeraApagadoDisco = rm.GetString("RegistoSeraApagadoDisco", ci);
            PastaUsada = rm.GetString("PastaUsada", ci);
            NaoCompativel = rm.GetString("NaoCompativel", ci);
            ExtrairDetalhesFicheiro = rm.GetString("ExtrairDetalhesFicheiro", ci);
            PreencherTitulo = rm.GetString("PreencherTitulo", ci);
            Validacao = rm.GetString("Validacao", ci);
            CampoRequerido = rm.GetString("CampoRequerido", ci);
            EscolhaCategoria = rm.GetString("EscolhaCategoria", ci);
            Registo = rm.GetString("Registo", ci);
            JaExisteBaseDados = rm.GetString("JaExisteBaseDados", ci);
            RegistoDuplicado = rm.GetString("RegistoDuplicado", ci);
            NaoGravado = rm.GetString("NaoGravado", ci);
            FicheiroNaoEscolhido = rm.GetString("FicheiroNaoEscolhido", ci);

            ExtensaoInvalida = rm.GetString("ExtensaoInvalida", ci);
            OperacaoTerminada = rm.GetString("OperacaoTerminada", ci);
            RegistosEncontrados = rm.GetString("RegistosEncontrados", ci);
            FicheiroNaoExisteDisco = rm.GetString("FicheiroNaoExisteDisco", ci);
            RegistoCriadoSucesso = rm.GetString("RegistoCriadoSucesso", ci);
            RegistoGravadoSucesso = rm.GetString("RegistoGravadoSucesso", ci);
            RegistoAnuladoSucesso = rm.GetString("RegistoAnuladoSucesso", ci);
            RegistoNaoGravado = rm.GetString("RegistoNaoGravado", ci);
            GravarRegisto = rm.GetString("btnSalvar", ci);
            CriarRegisto = rm.GetString("btnNovo", ci);
            AnularRegisto = rm.GetString("btnExcluir", ci);
        }
    }
}
