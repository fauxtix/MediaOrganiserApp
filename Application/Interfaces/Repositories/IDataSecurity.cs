namespace MediaOrganizerApp.Application.Interfaces.Repositories
{
    public interface IDataSecurity
    {
        string Decrypt(string cryptedString);
        string Decrypt(string cryptedString, string sCampo, int id);
        string Encrypt(string originalString);
    }
}