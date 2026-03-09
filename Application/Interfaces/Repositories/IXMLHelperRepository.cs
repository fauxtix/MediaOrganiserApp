using System.Xml;

namespace MediaOrganizerApp.Application.Interfaces.Repositories
{
    public interface IXMLHelperRepository
    {
        string GetValue(string itemName);
        XmlDocument GetXMLDoc();
        XmlDocument GetXMLDoc(int opcao);
        void Save();
        void SetValue(string itemName, string value);
    }
}