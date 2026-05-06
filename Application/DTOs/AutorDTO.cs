using System;
using System.Collections.Generic;

namespace MediaOrganizerApp.Application.DTOs
{
    public class AutorDTO
    {
        public int Id { get; set; }
        public string Descricao { get; set; }
        public string Sobre { get; set; }
        public DateTime? DataNascimento { get; set; }
        public string ImagemAutor { get; set; }
        public string Titulo { get; set; }
        public string NomeEditora { get; set; }
        public string DigitalPath { get; set; }

    }

    public class AutorVM
    {
        public string Descricao { get; set; }
        public string NomeAutor { get; set; }
        public string Titulo { get; set; }
        public string NomeEditora { get; set; }
        public string DigitalPath { get; set; }
    }
}

public class AuthorSummaryData
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<string> Occupations { get; set; } = new();
}