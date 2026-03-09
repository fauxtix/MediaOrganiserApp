using MediaOrganizerApp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace MediaOrganizerApp.Application.DTOs
{
    public class UserVM
    {
        public int Id { get; set; }
        public string User_Name { get; set; }
        public string Pwd { get; set; }
        public int RoleId { get; set; }
        public string Descricao { get; set; }
        public DateTime Last_Login_Date { get; set; }
        public DateTime Password_Change_Date { get; set; }
        public string First_Name { get; set; }
        public string EMail { get; set; }
        public string Mobile { get; set; }
        public int IsActive { get; set; }
    }
}
