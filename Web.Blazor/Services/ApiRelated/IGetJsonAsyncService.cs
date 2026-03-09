using System.Threading.Tasks;

namespace Web.Blazor.Services.ApiRelated
{
    public interface IGetJsonAsyncService<T>
    {
        Task<T> SendGetRequestAsync<T>(string uri) where T : class;
    }
}