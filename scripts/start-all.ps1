# Start Web API, then Blazor app (PowerShell)
# Usage: run from repo root in PowerShell

param(
    [string]$ApiProject = "./MediaOrganizerApp.WebApi/MediaOrganizerApp.WebApi.csproj",
    [string]$BlazorProject = "./Web.Blazor/Web.Blazor.csproj"
)

Write-Host "Starting Web API..."
Start-Process -FilePath dotnet -ArgumentList "run --project $ApiProject" -NoNewWindow

Write-Host "Starting Blazor app..."
Start-Process -FilePath dotnet -ArgumentList "run --project $BlazorProject" -NoNewWindow

Write-Host "All projects started (processes launched). Check individual console windows or logs for readiness."