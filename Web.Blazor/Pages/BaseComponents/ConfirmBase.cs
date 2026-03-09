using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Web.Blazor.Pages.BaseComponents
{
    public class ConfirmBase : ComponentBase
    {
        protected bool ShowConfirmation { get; set; }

        [Parameter]
        public string ConfirmationTitle { get; set; } = "Confirme operação, p.f.";

        [Parameter]
        public string ConfirmationMessage { get; set; } = "Quer mesmo apagar registo?";

        public void Show()
        {
            ShowConfirmation = true;
            StateHasChanged();
        }

        [Parameter]
        public EventCallback<bool> ConfirmationChanged { get; set; }

        protected async Task OnConfirmationChange(bool value)
        {
            ShowConfirmation = false;
            await ConfirmationChanged.InvokeAsync(value);
        }
    }
}