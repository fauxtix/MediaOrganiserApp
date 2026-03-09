using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace MediaOrganizerApp.Application.DTOs
{
    public  class LogCRUDModel
    {
        public int Id { get; set; }
        public string TableName { get; set; }
        public string UserId { get; set; }
        public OpcaoCRUD Action { get; set; }

    }
}
