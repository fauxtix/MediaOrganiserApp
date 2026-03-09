using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components;
using System.Linq;

public interface INavigationService
{
    string PreviousPage { get; }
    void UpdatePreviousPage(string currentPage);
}

public class NavigationService : INavigationService
{
    private string _previousPage;
    private readonly NavigationManager _navigationManager;

    public NavigationService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
        _navigationManager.LocationChanged += OnLocationChanged;
    }

    public string PreviousPage => _previousPage;

    public void UpdatePreviousPage(string currentPage)
    {
        // Update the previous page only if the current page is one of interest
        if (currentPage == "DashboardPage" || currentPage == "ManageLookupTables")
        {
            _previousPage = _navigationManager.Uri;
        }
    }

    private void OnLocationChanged(object sender, LocationChangedEventArgs e)
    {
        UpdatePreviousPage(GetCurrentPageName());
    }

    private string GetCurrentPageName()
    {
        var last = _navigationManager.Uri.Split('/').Last();
        return last;
    }
}