# Start ReportsRenderer, then Web API, then Blazor app (PowerShell)
# Usage: run from repo root in PowerShell

param(
    [string]$RendererProject = "./ReportsRenderer/ReportsRenderer.csproj",
    [string]$ApiProject = "./MediaOrganizerApp.WebApi/MediaOrganizerApp.WebApi.csproj",
    [string]$BlazorProject = "./Web.Blazor/Web.Blazor.csproj",
    [int]$RendererPort = 5000
)

Write-Host "Starting ReportsRenderer..."
Start-Process -FilePath dotnet -ArgumentList "run --project $RendererProject" -NoNewWindow

# Wait for renderer port to be listening
Write-Host "Waiting for renderer to be ready on port $RendererPort..."
while (-not (Test-NetConnection -ComputerName '127.0.0.1' -Port $RendererPort -WarningAction SilentlyContinue).TcpTestSucceeded) {
    Start-Sleep -Seconds 1
}
Write-Host "Renderer is ready."

Write-Host "Starting Web API..."
Start-Process -FilePath dotnet -ArgumentList "run --project $ApiProject" -NoNewWindow

Write-Host "Starting Blazor app..."
Start-Process -FilePath dotnet -ArgumentList "run --project $BlazorProject" -NoNewWindow

Write-Host "All projects started (processes launched). Check individual console windows or logs for readiness."