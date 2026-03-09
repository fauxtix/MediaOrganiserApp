using System;

namespace MediaOrganizerApp.Application.DTOs
{
    public class PubsWithDescriptions
    {
        public int Id { get; set; }
        public DateTime DataMovimento { get; set; }
        public string Titulo { get; set; }
        public string TituloOriginal { get; set; }
        public string Resumo { get; set; }
        public int IdAutor { get; set; }
        public int IdCategoria { get; set; }
        public int IdEditora { get; set; }
        public int IdFormato { get; set; }
        public int IdLocalArquivo { get; set; }
        public decimal Preco { get; set; }
        public int Paginas { get; set; }
        public string AnoEdicao { get; set; }
        public string ImagemCapa { get; set; }
        public string DigitalPath { get; set; }
        public double Rating { get; set; }
        public bool Lido { get; set; }
        public int UserID { get; set; }
        public string CreatedBy { get; set; }
        public string LastModifiedBy { get; set; }
        public string isbn { get; set; }
        public string Autor { get; set; }
        public string Categoria { get; set; }
        public string Editora { get; set; }
        public string Formato { get; set; }
        public string Local { get; set; }


    }
}
