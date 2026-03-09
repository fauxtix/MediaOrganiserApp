# MediaOrganizerApp

Have you ever felt frustrated with managing your media like hundreds of media scattered across different storage mediums/ hard disks? Wouldn't it be great to have it organised, allowing for easy management?

Media  Organiser App is an application to organise your media (Videos and mp3 files ) and Documents (Pdf's and Word documents). 
It also allows you to organise collections of Books, LPs, CDs, DVDs and documents written on paper (filing cabinets, for example).

# Demo

https://github.com/fauxtix/MediaOrganiserApp.Backend/assets/49880538/7eb85ef6-0a38-4d9e-af2f-1c374fde4801

## Running the Renderer and Web API (development)

This project includes two services used for RDLC report rendering:
- `ReportsRenderer` — standalone renderer service (can render RDLC to PDF)
- `MediaOrganizerApp.WebApi` — main API which can forward rendering to the renderer service

Below are exact commands and troubleshooting tips for local development.

Prerequisites
- .NET SDK installed (projects target .NET 7 and .NET 10). Verify with:
  - `dotnet --info`

Start the renderer service
(open a terminal, keep it running):

PowerShell
```
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project ./ReportsRenderer/ReportsRenderer.csproj
```

Bash (Linux / macOS / WSL)
```
export ASPNETCORE_ENVIRONMENT=Development
dotnet run --project ./ReportsRenderer/ReportsRenderer.csproj
```

Start the Web API
(open another terminal):

PowerShell
```
$env:ASPNETCORE_ENVIRONMENT = 'Development'
# optional: explicit renderer URL if not using launchSettings/appsettings
$env:RDLC_RENDERER_URL = 'http://localhost:5000/'
dotnet run --project ./MediaOrganizerApp.WebApi/MediaOrganizerApp.WebApi.csproj
```

Bash
```
export ASPNETCORE_ENVIRONMENT=Development
export RDLC_RENDERER_URL='http://localhost:5000/'
dotnet run --project ./MediaOrganizerApp.WebApi/MediaOrganizerApp.WebApi.csproj
```

Using Visual Studio / launch profiles
- The Web API project contains `Properties/launchSettings.json` with `ASPNETCORE_ENVIRONMENT=Development` and `RDLC_RENDERER_URL` configured for the development profile. Run using that profile from the IDE to get the same settings.

Verify rendering flow
- The renderer exposes `POST /api/render` — the Web API forwards to this endpoint when `RDLC_RENDERER_URL` is set. Example: start both services and call an endpoint that triggers a report (e.g. `GET /api/report/publications`).

Troubleshooting
- NU1101: Unable to find `Microsoft.Reporting.NETCore`
  - Ensure `nuget.org` is reachable. Run:
    - `dotnet nuget locals all --clear`
    - `dotnet restore ./ReportsRenderer/ReportsRenderer.csproj -v diag`
  - If behind a corporate feed, add your feed to `NuGet.Config` or add credentials.
  - As a temporary workaround the repo contains `ReportsRenderer/Lib/ReportingStubs.cs` (compile-time stubs). Remove this file after you successfully restore the official package.

- Startup errors: RDLC not found
  - Ensure the RDLC files are in `ReportsRenderer/Reports_RDLC` or set `REPORTS_PATH` environment variable to point to the folder containing `.rdlc` files.
  - Example: `export REPORTS_PATH=/path/to/Reports_RDLC` (bash) or ` $env:REPORTS_PATH='C:\path\to\Reports_RDLC'` (PowerShell).

- Renderer unreachable / API forwarding fails
  - Confirm renderer is running and reachable at the base address configured in `RDLC_RENDERER_URL` (default `http://localhost:5000/`).
  - Use `curl -v http://localhost:5000/api/render` (POST with JSON payload) to test connectivity.

- Port conflicts
  - Change `ASPNETCORE_URLS` or update `launchSettings.json` to use different ports. Check conflicts with `netstat` / `ss`.

- Runtime exceptions
  - Start the Web API with `ASPNETCORE_ENVIRONMENT=Development` so the Developer Exception Page is shown and logs contain full stack traces.
  - Check application logs (Serilog) printed to console for upstream errors when forwarding to renderer.

If you get an error you cannot resolve, paste the full console output or the failing `dotnet restore -v diag` / runtime stack trace and I will analyze it.



