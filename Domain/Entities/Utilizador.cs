using System;
using System.Text.Json.Serialization;

namespace MediaOrganizerApp.Domain.Entities
{
    public class Utilizador
    {
        public int User_Id { get; set; }
        public int RoleId { get; set; }
        public string User_Name { get; set; }
        public string Pwd { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
        public string EMail { get; set; }
        public string Mobile { get; set; }
        public DateTime? Last_Login_Date { get; set; }
        public DateTime? Password_Change_Date { get; set; }
        public int? IsActive { get; set; }

    }

    public class User_Info
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public string User_Name { get; set; }
        public string Pwd { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
        public string EMail { get; set; }
        public string Mobile { get; set; }
        public DateTime? Last_Login_Date { get; set; }
        public DateTime? Password_Change_Date { get; set; }
        public int IsActive { get; set; }
    }
}
