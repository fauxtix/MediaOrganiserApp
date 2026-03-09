using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Web.Blazor.Services.Common
{
    public class Mp3Handler
    {
        private readonly IJSRuntime _js;

        public Mp3Handler(IJSRuntime js)
        {
            _js = js;
        }

        public async Task SelectedMp3Changed(string mp3Filename)
        {
            await _js.InvokeVoidAsync("PlayAudioFile", mp3Filename);
        }

        public async ValueTask PauseAudio()
        {
            await _js.InvokeVoidAsync("PauseAudioFile");
        }
        public async ValueTask StopAudio()
        {
            await _js.InvokeVoidAsync("StopPlayAudioFile");
        }
        public async ValueTask DisposeAudio()
        {
            await _js.InvokeVoidAsync("DisposeAudioFile");
        }

        public void Dispose()
        {
        }
    }
}
