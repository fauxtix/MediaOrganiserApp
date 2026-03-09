using System.Net.Http;
using System.Threading.Tasks;

namespace Web.Blazor.Services.ApiRelated
{
    public interface IPdfService
    {
        Task<HttpResponseMessage> DisplayPdfInBrowser(string physicalPath);
    }
}