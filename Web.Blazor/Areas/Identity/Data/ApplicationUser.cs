using Microsoft.AspNetCore.Identity;
using System;

namespace Web.Blazor.Areas.Identity.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string First_Name { get; set; }
        public string Last_Name{ get; set; }
        public DateTime Last_Login_Date { get; set; }
        public DateTime Password_Change_Date { get; set; }
        public bool IsActive { get; set; }
        public bool IsDoctor { get; set; }
        public int CodMedico { get; set; }
        public bool PodeAlterarFicha { get; set; }
    }
}
