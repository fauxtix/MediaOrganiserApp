using System;
using System.ComponentModel.DataAnnotations;

namespace MediaOrganizerApp.Domain.Entities
{
    public abstract class BaseEntity : IEntity
    {
        [Required]
        public string CreatedBy { get; set; }
        [Required]
        public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
        public string LastModifiedBy { get; set; } = string.Empty;
        public DateTime? LastModifiedOnUtc { get; set; } = DateTime.UtcNow;
    }
}