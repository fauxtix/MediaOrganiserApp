using System;
using System.Collections.Generic;

namespace MediaOrganizerApp.Domain.Entities
{
    public class Autor
    {
        public int Id { get; set; }
        public string Descricao { get; set; }
        public string Sobre { get; set; }
        public DateTime? DataNascimento { get; set; }
        public string ImagemAutor { get; set; }
    }
}
