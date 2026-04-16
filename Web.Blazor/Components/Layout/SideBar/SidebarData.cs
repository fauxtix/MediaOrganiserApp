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

            new MenuItem(title: "Backup", icon: "fa fa-database", childMenuItems: new List<MenuItem>
            {
                new MenuItem(href: "/backupdatabase", title: _localizer["btnManutBD"], icon: "fa fa-database"),
                new MenuItem(href: "/backupuploads/1", title: _localizer["TituloUploadsPublicacoes"], icon:"fa fa-file-pdf-o"),
                new MenuItem(href: "/backupuploads/2", title: _localizer["TituloUploadsMedia"], icon:"fa fa-file-movie-o"),
                new MenuItem(href: "/backupuploads/3", title: _localizer["TituloUploadsImagens"], icon:"fa fa-image"),
            }),

            new MenuItem(title: _localizer["MsgManutencao"], icon: "fa fa-bars", childMenuItems: new List<MenuItem>
            {
                new MenuItem(href: "/admin/manageusers", title: _localizer["frmUsers"], icon: "oi oi-people"),
                new MenuItem(href: "/admin/managelookuptables", title: _localizer["TituloTabelasAuxiliares"], icon: "fa fa-table"),
             }),

            new MenuItem(href: "/admin/appsettings", title: _localizer["TituloDefinicoes"], icon: "fa fa-cog"),
            new MenuItem(href: "/admin/filemanager", title: _localizer["FileManager"], icon: "fa fa-folder-open"),
            new MenuItem(href: "/admin/viewapplogs", title: _localizer["Logs"], icon: "fa fa-table"),

            // AI submenu: Edit Prompts and Chat grouped under a single parent
            new MenuItem(title: "AI", icon: "fa fa-comments", childMenuItems: new List<MenuItem>
            {
                new MenuItem(href: "/edit-prompts", title: _localizer["TituloEditorPrompts"], icon: "fa fa-file-word-o"),
                new MenuItem(href: "/ai/chat", title: "Chat", icon: "fa fa-comment"),
            }),
        };
        return menu;
    }

    public List<MenuItem> GetGeneralMenuItems()
    {
        return new List<MenuItem>
        {
            new MenuItem(href: "/", title: "Home", icon: "ri-home-3-fill"),

            new MenuItem(title: _localizer["frmLivros"] , icon: "fa fa-book", childMenuItems: new List<MenuItem>
            {
                new MenuItem(href: "/publicacoes", title: _localizer["lblGestao"], icon:"fa fa-bars"),
                new MenuItem(href: "/imagegallery", title: _localizer["lblGaleria"], icon:"fa fa-list"),
            }),

            new MenuItem(title: "Media", icon: "fa fa-film", childMenuItems: new List<MenuItem>
            {
                new MenuItem(href: "/media", title: _localizer["lblGestao"], icon:"fa fa-bars"),
                new MenuItem(href: "/videogallery", title: _localizer["lblGaleria"], icon: "fa fa-image"),
                new MenuItem(href: "/media/playlists", title: _localizer["Playlists"], icon: "fa fa-play"),
            }),
            new MenuItem(href: "/autores", title: _localizer["mnuAutores"], icon: "fa fa-users"),
        };
    }
}
