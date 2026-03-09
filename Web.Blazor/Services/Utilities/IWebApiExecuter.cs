using System.Threading.Tasks;

namespace Web.Blazor.Services.Utilities
{
    public interface IWebApiExecuter
    {
        Task InvokeDelete(string uri);
        Task<T> InvokeGet<T>(string uri);
        Task<T> InvokePost<T>(string uri, T obj);
        Task<string> InvokePostReturnString<T>(string uri, T obj);
        Task InvokePut<T>(string uri, T obj);
    }
}