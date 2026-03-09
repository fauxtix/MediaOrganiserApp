using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Web.Blazor.Services.Common
{
    public class Mp4Handler
    {
        private readonly IJSRuntime _js;

        public Mp4Handler(IJSRuntime js)
        {
            _js = js;
        }

        public async Task SelectedMp4Changed(string mediaFilename)
        {
            await _js.InvokeVoidAsync("PlayVideo", $"/videos/{mediaFilename}");
        }

        public async Task PauseVideo(string mediaFilename)
        {
            await _js.InvokeVoidAsync("PauseVideo", $"/videos/{mediaFilename}");
        }
        public async Task StopVideo(string mediaFilename)
        {
            await _js.InvokeVoidAsync("StopVideo", $"/videos/{mediaFilename}");
        }

        public void Dispose()
        {
        }
    }
}
