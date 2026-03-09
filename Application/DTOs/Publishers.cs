using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.DTOs
{
    public class Publishers
    {
        public int Id { get; set; }
        public string Descricao { get; set; }
        public string Morada { get; set; }
        public string Telefone { get; set; }
        public string EMail { get; set; }
        public string Site { get; set; }
    }
}
