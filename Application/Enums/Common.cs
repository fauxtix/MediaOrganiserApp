namespace MediaOrganizerApp.Application.Enums
{
    public class AppDefinitions
    {
        public enum ReportType
        {
            Authors,
            Publications,
            Media,
            Categories,
            StoragePlaces,
            Formats,
            Publishers
        }

        public enum Modules
        {
            Inquilinos,
            Fiadores,
            Fracoes,
            Imoveis,
            Recebimentos,
            Pagamentos,
            Contactos,
            Arrendamentos,
            PdfViewer
        }

        public enum TipoBackup
        {
            SqLiteBackup,
            AccessBackup,
            SqlServerBackup
        }

        public enum Roles
        {
            Admin = 1,
            Utilizador = 2,
        }

        /// <summary>
        /// Tabelas a carregar nas comboboxes
        /// </summary>
        public enum ComboBoxItem
        {
            Role,
            CategoriasMedia,
            Formatos_Media,
            CategoriasPublicacoes,
            Autores,
            Editoras,
            Formatos,
            LocaisArquivo,
            Utilizadores
        }

        /// <summary>
        /// Operações a efetuar sobre os registos
        /// </summary>
        public enum OpcoesRegisto
        {
            Inserir,
            Gravar,
            Apagar,
            Duplicar,
            Backup,
            Zip,
            Warning,
            Info,
            Error
        }

        public enum PlayerState
        {
            Indefinido,
            Parado,
            Pausa,
            EmExecucao,
            ScanForward,
            ScanReverse,
            Buffering,
            Aguardando,
            Terminado,
            Ligando,
            Pronto,
            Reconectando,
            Ultimo
        }

        public enum Query_TipoMedia
        {
            Video = 1,
            Audio = 2,
            Others = 3,
            Todos = 99
        }

        public enum CategoryType
        {
            Publicacoes = 1,
            Media = 2
        }

        /// <summary>
        /// User roles supported by system
        /// </summary>
        public enum UserRole
        {
            Admin, GeneralUser
        }

        public enum TipoPesquisa
        {
            Todos,
            Autor,
            Editora,
            Genero,
            Formato,
            Arquivo,
            Lido,
            SemPaginas,
            AnoEdicao,
            SemCapa,
            Pago,
            Duplicados,
            Utilizador,
            Titulo
        }

        public enum OpcaoCRUD
        {
            Inserir,
            Atualizar,
            Anular,
            Ler
        }

        public enum TipoPasta
        {
            PastaDoc,
            PastaImgPubs,
            PastaImgAutores,
            PastaList,
            PastaBackupSqLitse,
            PastaBackupAccess,
            PastaScripts
        }

        public enum TipoBD
        {
            SqLite,
            Access,
            MySql,
            SqlServer
        }

        public enum Idioma
        {
            Portugues,
            Espanhol,
            Ingles,
            Frances
        }

        public enum Metadados
        {
            Autor,
            AnoEdicao,
            Titulo,
            Paginas,
            Assunto,
            Keywords
        }

        public enum OpcaoSeguranca
        {
            Backup,
            Restore
        }

        public enum Upload_Backup_Options
        {
            Publications,
            Media,
            Images
        }

        public enum TipoDocumento
        {
            Word,
            Pdf,
            Excel
        }

    }
}
