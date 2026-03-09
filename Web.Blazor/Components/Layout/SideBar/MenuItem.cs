using System.Collections.Generic;

namespace Web.Blazor.Components.Layout.SideBar;

public class Suffix
{
    public string Title { get; set; }
    public string Class { get; set; }

    public Suffix(string title, string _class)
    {
        Title = title;
        Class = _class;
    }
}

public class MenuItem
{
    private const int _itemHeight = 50;

    public string? Href { get; set; }
    public string? Title { get; set; }
    public string? Icon { get; set; }
    public Suffix? Suffix { get; set; }
    public List<MenuItem>? ChildMenuItems { get; set; }

    private bool _isOpened = false;

    public bool IsOpened
    {
        get => _isOpened;
        set
        {
            if (HasChildMenuItems)
            {
                foreach (var child in ChildMenuItems!)
                {
                    if (!value)
                    {
                        child.IsOpened = false;
                    }
                }
            }

            _isOpened = value;
        }
    }

    public bool IsActive { get; set; } = false;

    public bool HasChildMenuItems => ChildMenuItems != null && ChildMenuItems.Count > 0;

    public MenuItem(
        string? href = null,
        string? title = null,
        string? icon = null,
        Suffix? suffix = null,
        List<MenuItem>? childMenuItems = null)
    {
        Href = href;
        Title = title;
        Icon = icon;
        Suffix = suffix;
        ChildMenuItems = childMenuItems;
    }

    public int CalcSubMenuHeight()
    {
        if (ChildMenuItems == null || !IsOpened)
            return 0;

        int sumHeight = 0;
        foreach (var childMenuItem in ChildMenuItems)
        {
            sumHeight += childMenuItem.CalcSubMenuHeight();
        }

        return sumHeight + ChildMenuItems.Count * _itemHeight;
    }
}
