using Microsoft.Extensions.Localization;
namespace Web.Blazor.Components.Layout.SideBar;

public class SidebarData
{
    private readonly IStringLocalizer<App> _localizer;

    public SidebarData(IStringLocalizer<App> localizer)
    {
        _localizer = localizer;
    }
    public List<MenuItem> GetStandardMenuItems()
    {
        var menu = new List<MenuItem>
        {

            new(title: "Backup", icon: "fa fa-database", childMenuItems: new List<MenuItem>
            {
                new(href: "/backupdatabase", title: _localizer["btnManutBD"], icon: "fa fa-database"),
                new(href: "/backupuploads/1", title: _localizer["TituloUploadsPublicacoes"], icon:"fa fa-file-pdf-o"),
                new(href: "/backupuploads/2", title: _localizer["TituloUploadsMedia"], icon:"fa fa-file-movie-o"),
                new(href: "/backupuploads/3", title: _localizer["TituloUploadsImagens"], icon:"fa fa-image"),
            }),

            new(title: _localizer["MsgManutencao"], icon: "fa fa-bars", childMenuItems: new List<MenuItem>
            {
                new(href: "/admin/manageusers", title: _localizer["frmUsers"], icon: "oi oi-people"),
                new(href: "/admin/managelookuptables", title: _localizer["TituloTabelasAuxiliares"], icon: "fa fa-table"),
             }),

            new(href: "/admin/appsettings", title: _localizer["TituloDefinicoes"], icon: "fa fa-cog"),
            new(href: "/admin/filemanager", title: _localizer["FileManager"], icon: "fa fa-folder-open"),
            new(href: "/admin/viewapplogs", title: _localizer["Logs"], icon: "fa fa-table"),

            new(title: "AI", icon: "fa fa-comments", childMenuItems: new List<MenuItem>
            {
                new(href: "/edit-prompts", title: _localizer["TituloEditorPrompts"], icon: "fa fa-file-word-o"),
                new(href: "/ai/chat", title: "Chat", icon: "fa fa-comment"),
            }),
        };
        return menu;
    }

    public List<MenuItem> GetGeneralMenuItems()
    {
        return new List<MenuItem>
        {
            new(href: "/", title: "Home", icon: "ri-home-3-fill"),

            new(title: _localizer["frmLivros"] , icon: "fa fa-book", childMenuItems: new List<MenuItem>
            {
                new(href: "/publicacoes", title: _localizer["lblGestao"], icon:"fa fa-bars"),
                new(href: "/imagegallery", title: _localizer["lblGaleria"], icon:"fa fa-list"),
                new(href: "/autores", title: _localizer["mnuAutores"], icon: "fa fa-users"),

            }),

            new(title: "Media", icon: "fa fa-film", childMenuItems: new List<MenuItem>
            {
                new(href: "/media", title: _localizer["lblGestao"], icon:"fa fa-bars"),
                new(href: "/videogallery", title: _localizer["lblGaleria"], icon: "fa fa-image"),
                new(href: "/media/playlists", title: _localizer["Playlists"], icon: "fa fa-play"),
            }),
        };
    }
}
