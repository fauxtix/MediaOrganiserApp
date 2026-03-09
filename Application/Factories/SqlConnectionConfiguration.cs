using MediaOrganizerApp.Application.Interfaces;

namespace MediaOrganizerApp.Application.Factories
{
    public class SqlConnectionConfiguration : ISqlConnectionConfiguration
    {
        public SqlConnectionConfiguration(string value) => Value = value;
        public string Value { get; }
    }
}
