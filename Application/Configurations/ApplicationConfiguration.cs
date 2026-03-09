using MediaOrganizerApp.Application.Formatting;
using System;
using System.Configuration;

namespace MediaOrganizerApp.Application.Configurations
{
    public static class ApplicationConfiguration
    {
        const string DEFAULT_CONNECTION_KEY = "defaultConnection";
        const string LOGGING_ENABLED_KEY = "logging";
        const string TRACING_ENABLED_KEY = "tracing";
        const string DATE_WISE_LOG_TRACE_KEY = "dateWiseLogTrace";
        const string EXPENSE_CCY_KEY = "expenseCCY";
        const string SUPPORT_MAIL_KEY = "supportMail";
        const string SUPPORT_URL_KEY = "supportURL";
        const string PASTA_BD_SQLITE = "PastaBDSqLite";
        const string PASTA_BD_ACCESS = "PastaBDAccess";
        const string PASTA_BACKUP_SQLSERVER = "PastaBackupSqlServer";
        const string PASTA_BACKUP_SQLITE = "SqLiteBackupFolder";
        const string PASTA_BACKUP_ACCESS = "PastaBackupAccess";
        const string PASTA_IMAGENS_PUBS = "PastaImagensPubs";
        const string PASTA_IMAGEM_FUNDO = "PastaImagemFundo";
        const string PASTA_IMAGENS_AUTORES = "PastaImagensAutores";
        const string PASTA_LISTAGENS = "PastaListagens";
        const string PASTA_PUBLICACOES = "PastaPublicacoes";
        const string PASTA_DOWNLOADS = "PastaDownloads";
        const string FICHEIRO_BACKUP = "FicheiroBackup";
        const string PASTA_DOCUMENTOS_CAIXA = "PastaPublicacoesCaixa";
        const string PASTA_DOCUMENTOS_EXPORTACAO = "PastaPublicacoesExportacao";
        const string PASTA_DOCUMENTOS_SCRIPTS = "PastaScripts";
        const string FIRST_TIME = "first_time";
        const string LANGUAGE = "language";
        const string NOTIFIER = "UsaNotifier";
        const string TEMPO_NOTIFIER = "TempoNotifier";
        const string ISCLIENT = "Is_Client";
        const string LOGO_IMAGE = "LogoImage";

        const string FICHEIROS_ORGANIZADOS = "FicheirosOrganizados";
        const string TIPO_ORGANIZACAO_FICHEIROS = "TipoOrganizacaoFicheiros";


        /// <summary>
        /// Gets default connection being used.
        /// </summary>
        public static string DefaultConnection
        {
            get
            {
                return ConfigurationManager.AppSettings[DEFAULT_CONNECTION_KEY];
            }
        }

        /// <summary>
        /// Gets DB provider from app.config file
        /// </summary>
        public static string DBProvider
        {
            get
            {
                return ConfigurationManager.ConnectionStrings[DefaultConnection].ProviderName;
            }
        }

        public static bool Is_Client
        {
            get
            {
                return DataFormat.GetBoolean(ConfigurationManager.AppSettings[ISCLIENT]);
            }
        }
        /// <summary>
        /// Gets connection string from app.config file
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["sqlServerCon"].ConnectionString;
            }
        }


        public static string LogoImage
        {
            get
            {
                return DataFormat.GetString(ConfigurationManager.AppSettings[LOGO_IMAGE]);
            }
        }
        /// <summary>
        /// Gets whether logging is enabled or not
        /// </summary>
        public static bool LoggingEnabled
        {
            get
            {
                return DataFormat.GetBoolean(ConfigurationManager.AppSettings[LOGGING_ENABLED_KEY]);
            }
        }

        public static bool FicheirosOrganizados
        {
            get
            {
                return DataFormat.GetBoolean(ConfigurationManager.AppSettings[FICHEIROS_ORGANIZADOS]);
            }
        }

        public static string TipoOrganizacaoFicheiros
        {
            get
            {
                return DataFormat.GetString(ConfigurationManager.AppSettings[TIPO_ORGANIZACAO_FICHEIROS]);
            }
        }

        public static string PastaBackupSqLite
        {
            
            get
            {
                return DataFormat.GetString(ConfigurationManager.AppSettings[PASTA_BACKUP_SQLITE]);
            }
        }

        public static string PastaBackupAccess
        {
            get
            {
                return DataFormat.GetString(ConfigurationManager.AppSettings[PASTA_BD_ACCESS]);
            }
        }

        public static string PastaBDSqLite
        {
            get
            {
                return DataFormat.GetString(ConfigurationManager.AppSettings[PASTA_BD_SQLITE]);
            }
        }

        public static string PastaBDccess
        {
            get
            {
                return DataFormat.GetString(ConfigurationManager.AppSettings[PASTA_BACKUP_ACCESS]);
            }
        }

        public static string PastaSqlServer
        {
            get
            {
                return DataFormat.GetString(ConfigurationManager.AppSettings[PASTA_BACKUP_SQLSERVER]);
            }
        }

        public static string PastaImagensPubs
        {
            get
            {
                return DataFormat.GetString(ConfigurationManager.AppSettings[PASTA_IMAGENS_PUBS]);
            }
        }

        public static string PastaImagensAutores
        {
            get
            {
                return DataFormat.GetString(ConfigurationManager.AppSettings[PASTA_IMAGENS_AUTORES]);
            }
        }

        public static string PastaImagemFundo
        {
            get
            {
                return DataFormat.GetString(ConfigurationManager.AppSettings[PASTA_IMAGEM_FUNDO]);
            }
        }

        public static string PastaListagens
        {
            get
            {
                return DataFormat.GetString(ConfigurationManager.AppSettings[PASTA_LISTAGENS]);
            }
        }

        public static string Language
        {
            get
            {
                return DataFormat.GetString(ConfigurationManager.AppSettings[LANGUAGE]);
            }
        }

        public static string UsaNotifier
        {
            get
            {
                return DataFormat.GetString(ConfigurationManager.AppSettings[NOTIFIER]);
            }
        }

        public static int Tempo_Notifier
        {
            get
            {
                return Convert.ToInt32(DataFormat.GetString(ConfigurationManager.AppSettings[TEMPO_NOTIFIER]));
            }
        }

        public static string FicheiroBackup
        {
            get
            {
                return DataFormat.GetString(ConfigurationManager.AppSettings[FICHEIRO_BACKUP]);
            }
        }

        public static string PastaPublicacoes
        {
            get
            {
                return DataFormat.GetString(ConfigurationManager.AppSettings[PASTA_PUBLICACOES]);
            }
        }

        public static string PastaDownloads
        {
            get
            {
                return DataFormat.GetString(ConfigurationManager.AppSettings[PASTA_DOWNLOADS]);
            }
        }

        public static string PastaScripts
        {
            get
            {
                return DataFormat.GetString(ConfigurationManager.AppSettings[PASTA_DOCUMENTOS_SCRIPTS]);
            }
        }

        /// <summary>
        /// Gets whether logging is enabled or not
        /// </summary>
        public static string PastaPublicacoesCaixa
        {
            get
            {
                return DataFormat.GetString(ConfigurationManager.AppSettings[PASTA_DOCUMENTOS_CAIXA]);
            }
        }

        /// <summary>
        /// Gets whether logging is enabled or not
        /// </summary>
        public static string PastaPublicacoesExportacao
        {
            get
            {
                return DataFormat.GetString(ConfigurationManager.AppSettings[PASTA_DOCUMENTOS_EXPORTACAO]);
            }
        }


        /// <summary>
        /// Gets whether tracing is enabled or not
        /// </summary>
        public static bool TracingEnabled
        {
            get
            {
                return DataFormat.GetBoolean(ConfigurationManager.AppSettings[TRACING_ENABLED_KEY]);
            }
        }

        /// <summary>
        /// Type of Logging/ Tracing
        /// </summary>
        public enum LogTraceType
        {
            SingleFile,
            DateWise
        }

        /// <summary>
        /// Gets Type of Log/ Trace i.e. Separate file of one file for each date
        /// </summary>
        public static LogTraceType LogTraceSetting
        {
            get
            {
                return DataFormat.GetBoolean(ConfigurationManager.AppSettings[DATE_WISE_LOG_TRACE_KEY]) ? LogTraceType.DateWise : LogTraceType.SingleFile;
            }
        }

        /// <summary>
        /// Gets Expense Currency from app.config
        /// </summary>
        public static string ExpenseCCY
        {
            get
            {
                return DataFormat.GetString(ConfigurationManager.AppSettings[EXPENSE_CCY_KEY]);
            }
        }

        /// <summary>
        /// Gets support mail from app.config
        /// </summary>
        public static string SupportMail
        {
            get
            {
                return DataFormat.GetString(ConfigurationManager.AppSettings[SUPPORT_MAIL_KEY]);
            }
        }


        /// <summary>
        /// Gets support URL
        /// </summary>
        public static string SupportURL
        {
            get
            {
                return DataFormat.GetString(ConfigurationManager.AppSettings[SUPPORT_URL_KEY]);
            }
        }

        /// <summary>
        /// 1ª utilização do programa
        /// </summary>
        public static string Primeira_Vez
        {
            get
            {
                return DataFormat.GetString(ConfigurationManager.AppSettings[FIRST_TIME]);
            }
        }

    }
}