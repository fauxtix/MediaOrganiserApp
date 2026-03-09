namespace MediaOrganizerApp.Application.DTOs
{
    public class ApplicationSettingsDto
    {
        public int Id { get; set; }
        public string ApiKey { get; set; }
        public string DefaultLanguage { get; set; }
        public string Language { get; set; }
        public string PastaDownloadsPdf { get; set; }
        public string PastaDownloadsMedia { get; set; }
        public string PastaImagens { get; set; }
        public string PastaBackupUploads { get; set; }
        public string PastaBackupBD { get; set; }
        public string PastaPublicacoes { get; set; }
        public string PastaListagens { get; set; }
        public string PastaImagensAutores { get; set; }
        public string PastaImagensCapasLivros { get; set; }
        public string PastaImagemFundo { get; set; }
        public bool FicheirosOrganizados { get; set; }
        public bool RemoveImagemAposAlteracao { get; set; }
        public int TempoNotifier { get; set; }

        // Hotmail settings

        public string Hostname { get; set; }
        public string Port { get; set; }
        public bool UseSsl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        // landing page info
        public string AuthorMobile { get; set; }
        public string SupportMail { get; set; }



    }
}
