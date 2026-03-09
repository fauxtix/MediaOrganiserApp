using System;

namespace MediaOrganizerApp.Application.DTOs
{
    public class MediaVM
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public DateTime DataMov { get; set; }
        public string FilePath { get; set; }
        public string FileSource { get; set; }
        public string FileUrl { get; set; }
        public byte TipoMedia { get; set; }
        public bool Visualizado { get; set; }
        public double Rating { get; set; }
        public string Notas { get; set; }
        public int IdGenero { get; set; }
        public string Autor { get; set; }
        public string AnoEdicao { get; set; }
        public int GuardaEmdisco { get; set; }
        public int IdFormato_Media { get; set; }
        public string Tempo { get; set; }
        public string Tamanho { get; set; }
        public string Titulo { get; set; }
        public string NomeAutor { get; set; }
        public string FormatoMedia { get; set; }
        public string Genero { get; set; }
        public string CoverFile { get; set; }
        public string CreatedBy { get; set; }
    }
}
