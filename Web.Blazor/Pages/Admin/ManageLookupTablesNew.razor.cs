using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Web.Blazor.Pages.Admin
{
    public partial class ManageLookupTablesNew
    {
        [Inject] public IStringLocalizer<App>? L { get; set; }

        private List<SettingsGroup> SettingsGroups = new();
        private string SelectedNode = "categorias-publicacoes";
        private readonly HashSet<string> ExpandedGroups = new() { "publicacoes" };

        protected override void OnInitialized()
        {
            SettingsGroups = new List<SettingsGroup>
            {
                new("publicacoes", "TituloGrupoPublicacoes", "fa-book", new List<SettingsLeaf>
                {
                    new("categorias-publicacoes", "TituloCategoriasPublicacoes", "fa-book"),
                    new("editoras", "frmEditoras", "fa-building-o"),
                    new("formatos", "frmFormatos", "fa-th-large"),
                    new("locais-arquivo", "frmLocaisArquivo", "fa-folder-open-o")
                }),
                new("media", "TituloGrupoMedia", "fa-camera", new List<SettingsLeaf>
                {
                    new("categorias-media", "TituloCategoriasMedia", "fa-camera"),
                    new("formatos-media", "frmFormatosMedia", "fa-film")
                }),
                new("ia", "TituloGrupoIA", "fa-cogs", new List<SettingsLeaf>
                {
                    new("ai-models", "AiModelsTitle", "fa-cogs")
                })
            };
        }

        private void ToggleGroup(string id)
        {
            if (!ExpandedGroups.Remove(id))
            {
                ExpandedGroups.Add(id);
            }
        }

        private void SelectNode(string id)
        {
            SelectedNode = id;
        }

        private sealed class SettingsGroup
        {
            public SettingsGroup(string id, string resxKey, string icon, List<SettingsLeaf> leaves)
            {
                Id = id;
                ResxKey = resxKey;
                Icon = icon;
                Leaves = leaves;
            }

            public string Id { get; }
            public string ResxKey { get; }
            public string Icon { get; }
            public List<SettingsLeaf> Leaves { get; }
        }

        private sealed class SettingsLeaf
        {
            public SettingsLeaf(string id, string resxKey, string icon)
            {
                Id = id;
                ResxKey = resxKey;
                Icon = icon;
            }

            public string Id { get; }
            public string ResxKey { get; }
            public string Icon { get; }
        }
    }
}