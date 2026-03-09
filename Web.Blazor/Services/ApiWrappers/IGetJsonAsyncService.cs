using System.Threading.Tasks;

namespace Web.Blazor.Services.ApiWrappers
{
    public interface IGetJsonAsyncService<T>
    {
        Task<P> SendGetRequestAsync<P>(string uri) where P : class;
    }
}