using System;

namespace MediaOrganizerApp.Domain.Entities
{
    public interface IEntity
    {
        public string CreatedBy { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime? LastModifiedOnUtc { get; set; }
    }
}