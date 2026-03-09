# .NET 10.0 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that a .NET 10.0 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 10.0 upgrade.
3. Upgrade Domain\MediaOrganizerApp.Domain.csproj
4. Upgrade Application\MediaOrganizerApp.Application.csproj
5. Upgrade Web.Blazor.ReportObjects\Web.Blazor.Reporting.csproj
6. Upgrade MediaOrganizerApp.Persistence\MediaOrganizerApp.Persistence.csproj
7. Upgrade MediaOrganizerApp.Tests\MediaOrganizerApp.Tests.csproj
8. Upgrade Web.Blazor\Web.Blazor.csproj
9. Upgrade MediaOrganizerApp.Identity\MediaOrganizerApp.Identity.csproj
10. Upgrade MediaOrganizerApp.WebApi\MediaOrganizerApp.WebApi.csproj

## Settings

This section contains settings and data used by execution steps.

### Aggregate NuGet packages modifications across all projects

NuGet packages used across all selected projects or their dependencies that need version update in projects that reference them.

| Package Name                                            | Current Version | New Version | Description                                                    |
|:--------------------------------------------------------|:---------------:|:-----------:|:---------------------------------------------------------------|
| AutoMapper.Extensions.Microsoft.DependencyInjection     | 12.0.1          | 12.0.1      | Deprecated - functionality included directly in AutoMapper     |
| Blazored.Menu                                           | 2.2.0           | 2.2.0       | Deprecated - should be replaced                                |
| FluentValidation.AspNetCore                             | 11.3.0          | 11.3.1      | Deprecated - should be replaced with newer version             |
| Microsoft.AspNet.Identity.EntityFramework               | 2.2.4           |             | Incompatible - no supported version found                      |
| Microsoft.AspNetCore.Authentication.Google              | 7.0.0           | 10.0.1      | Recommended for .NET 10.0                                      |
| Microsoft.AspNetCore.Authentication.OpenIdConnect       | 7.0.0           | 10.0.1      | Recommended for .NET 10.0                                      |
| Microsoft.AspNetCore.Components                         | 7.0.20          | 10.0.1      | Recommended for .NET 10.0                                      |
| Microsoft.AspNetCore.Components.Forms                   | 7.0.0           | 10.0.1      | Recommended for .NET 10.0                                      |
| Microsoft.AspNetCore.Components.Web                     | 7.0.0           | 10.0.1      | Recommended for .NET 10.0                                      |
| Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore    | 7.0.0           | 10.0.1      | Recommended for .NET 10.0                                      |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore       | 7.0.0           | 10.0.1      | Recommended for .NET 10.0                                      |
| Microsoft.AspNetCore.Identity.UI                        | 7.0.20          | 10.0.1      | Recommended for .NET 10.0                                      |
| Microsoft.AspNetCore.Mvc.NewtonsoftJson                 | 6.0.7           | 10.0.1      | Recommended for .NET 10.0                                      |
| Microsoft.AspNetCore.OpenApi                            | 7.0.0           | 10.0.1      | Recommended for .NET 10.0                                      |
| Microsoft.AspNetCore.SignalR                            | 1.1.0           |             | Replace with Microsoft.AspNetCore.SignalR.Client 10.0.1        |
| Microsoft.AspNetCore.SignalR.Client                     |                 | 10.0.1      | Replacement for Microsoft.AspNetCore.SignalR                   |
| Microsoft.EntityFrameworkCore                           | 7.0.0           | 10.0.1      | Recommended for .NET 10.0                                      |
| Microsoft.EntityFrameworkCore.Design                    | 7.0.0           | 10.0.1      | Recommended for .NET 10.0                                      |
| Microsoft.EntityFrameworkCore.SqlServer                 | 7.0.0           | 10.0.1      | Recommended for .NET 10.0                                      |
| Microsoft.EntityFrameworkCore.Tools                     | 7.0.0           | 10.0.1      | Recommended for .NET 10.0                                      |
| Microsoft.Extensions.Configuration                      | 7.0.0           | 10.0.1      | Recommended for .NET 10.0                                      |
| Microsoft.Extensions.Configuration.Abstractions         | 7.0.0           | 10.0.1      | Recommended for .NET 10.0                                      |
| Microsoft.Extensions.Configuration.FileExtensions       | 7.0.0           | 10.0.1      | Recommended for .NET 10.0                                      |
| Microsoft.Extensions.Configuration.Json                 | 7.0.0           | 10.0.1      | Recommended for .NET 10.0                                      |
| Microsoft.Extensions.DependencyInjection                | 7.0.0           | 10.0.1      | Recommended for .NET 10.0                                      |
| Microsoft.Extensions.Http                               | 7.0.0           | 10.0.1      | Recommended for .NET 10.0                                      |
| Microsoft.Win32.Registry                                | 5.0.0           |             | Functionality included with framework reference                |
| Microsoft.Windows.Compatibility                         | 6.0.6           | 10.0.1      | Recommended for .NET 10.0                                      |
| Newtonsoft.Json                                         | 13.0.3          | 13.0.4      | Recommended for .NET 10.0                                      |
| System.Configuration.ConfigurationManager               | 7.0.0           | 10.0.1      | Recommended for .NET 10.0                                      |
| System.Drawing.Common                                   | 7.0.0           | 10.0.1      | Recommended for .NET 10.0                                      |
| System.IdentityModel.Tokens.Jwt                         | 6.34.0          | 8.15.0      | Deprecated - move to latest version                            |
| System.Linq.Dynamic.Core                                | 1.3.0           | 1.7.1       | **Security vulnerability**                                     |
| System.Net.Http                                         | 4.3.4           |             | Functionality included with framework reference                |
| System.Text.Json                                        | 8.0.5           | 10.0.1      | Recommended for .NET 10.0                                      |
| System.Text.RegularExpressions                          | 4.3.1           |             | Functionality included with framework reference                |

### Project upgrade details

This section contains details about each project upgrade and modifications that need to be done in the project.

#### Domain\MediaOrganizerApp.Domain.csproj modifications

Project properties changes:
- Target framework should be changed from `net7.0` to `net10.0`

NuGet packages changes:
- Microsoft.AspNet.Identity.EntityFramework 2.2.4 is incompatible and has no supported version

#### Application\MediaOrganizerApp.Application.csproj modifications

Project properties changes:
- Target framework should be changed from `net7.0` to `net10.0`

NuGet packages changes:
- Microsoft.AspNetCore.Components should be updated from `7.0.20` to `10.0.1` (*recommended for .NET 10.0*)
- Microsoft.AspNetCore.Components.Forms should be updated from `7.0.0` to `10.0.1` (*recommended for .NET 10.0*)
- Microsoft.AspNetCore.Components.Web should be updated from `7.0.0` to `10.0.1` (*recommended for .NET 10.0*)
- Microsoft.Extensions.Configuration should be updated from `7.0.0` to `10.0.1` (*recommended for .NET 10.0*)
- Microsoft.Extensions.Configuration.Abstractions should be updated from `7.0.0` to `10.0.1` (*recommended for .NET 10.0*)
- Microsoft.Extensions.Configuration.FileExtensions should be updated from `7.0.0` to `10.0.1` (*recommended for .NET 10.0*)
- Microsoft.Extensions.Configuration.Json should be updated from `7.0.0` to `10.0.1` (*recommended for .NET 10.0*)
- System.Configuration.ConfigurationManager should be updated from `7.0.0` to `10.0.1` (*recommended for .NET 10.0*)
- System.Drawing.Common should be updated from `7.0.0` to `10.0.1` (*recommended for .NET 10.0*)
- Microsoft.Win32.Registry 5.0.0 functionality is included with framework reference (*can be removed*)

#### Web.Blazor.ReportObjects\Web.Blazor.Reporting.csproj modifications

Project properties changes:
- Target framework should be changed from `net7.0` to `net10.0`

#### MediaOrganizerApp.Persistence\MediaOrganizerApp.Persistence.csproj modifications

Project properties changes:
- Target framework should be changed from `net7.0` to `net10.0`

NuGet packages changes:
- Microsoft.EntityFrameworkCore should be updated from `7.0.0` to `10.0.1` (*recommended for .NET 10.0*)

#### MediaOrganizerApp.Tests\MediaOrganizerApp.Tests.csproj modifications

Project properties changes:
- Target framework should be changed from `net7.0` to `net10.0`

#### Web.Blazor\Web.Blazor.csproj modifications

Project properties changes:
- Target framework should be changed from `net7.0` to `net10.0`

NuGet packages changes:
- Microsoft.AspNetCore.Authentication.Google should be updated from `7.0.0` to `10.0.1` (*recommended for .NET 10.0*)
- Microsoft.AspNetCore.Components should be updated from `7.0.20` to `10.0.1` (*recommended for .NET 10.0*)
- Microsoft.AspNetCore.Identity.UI should be updated from `7.0.20` to `10.0.1` (*recommended for .NET 10.0*)
- Microsoft.AspNetCore.Mvc.NewtonsoftJson should be updated from `6.0.7` to `10.0.1` (*recommended for .NET 10.0*)
- Microsoft.AspNetCore.SignalR.Client should be updated from `7.0.0` to `10.0.1` (*recommended for .NET 10.0*)
- Microsoft.Extensions.DependencyInjection should be updated from `7.0.0` to `10.0.1` (*recommended for .NET 10.0*)
- Microsoft.Extensions.Http should be updated from `7.0.0` to `10.0.1` (*recommended for .NET 10.0*)
- Microsoft.Windows.Compatibility should be updated from `6.0.6` to `10.0.1` (*recommended for .NET 10.0*)
- Newtonsoft.Json should be updated from `13.0.3` to `13.0.4` (*recommended for .NET 10.0*)
- System.Text.Json should be updated from `8.0.5` to `10.0.1` (*recommended for .NET 10.0*)
- Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore should be updated from `7.0.0` to `10.0.1` (*recommended for .NET 10.0*)
- Microsoft.AspNetCore.Identity.EntityFrameworkCore should be updated from `7.0.0` to `10.0.1` (*recommended for .NET 10.0*)
- Microsoft.EntityFrameworkCore.SqlServer should be updated from `7.0.0` to `10.0.1` (*recommended for .NET 10.0*)
- Microsoft.EntityFrameworkCore.Tools should be updated from `7.0.0` to `10.0.1` (*recommended for .NET 10.0*)
- System.Net.Http 4.3.4 functionality is included with framework reference (*can be removed*)
- System.Text.RegularExpressions 4.3.1 functionality is included with framework reference (*can be removed*)
- **System.Linq.Dynamic.Core should be updated from `1.3.0` to `1.7.1` (security vulnerability)**
- AutoMapper.Extensions.Microsoft.DependencyInjection 12.0.1 is deprecated (*functionality included in AutoMapper*)
- Blazored.Menu 2.2.0 is deprecated (*should be replaced*)
- FluentValidation.AspNetCore should be updated from `11.3.0` to `11.3.1` (*deprecated*)
- System.IdentityModel.Tokens.Jwt should be updated from `6.34.0` to `8.15.0` (*deprecated*)

#### MediaOrganizerApp.Identity\MediaOrganizerApp.Identity.csproj modifications

Project properties changes:
- Target framework should be changed from `net7.0` to `net10.0`

#### MediaOrganizerApp.WebApi\MediaOrganizerApp.WebApi.csproj modifications

Project properties changes:
- Target framework should be changed from `net7.0` to `net10.0`

NuGet packages changes:
- Microsoft.AspNetCore.Authentication.Google should be updated from `7.0.0` to `10.0.1` (*recommended for .NET 10.0*)
- Microsoft.AspNetCore.Identity.UI should be updated from `7.0.20` to `10.0.1` (*recommended for .NET 10.0*)
- Microsoft.AspNetCore.OpenApi should be updated from `7.0.0` to `10.0.1` (*recommended for .NET 10.0*)
- Microsoft.AspNetCore.SignalR 1.1.0 should be replaced with Microsoft.AspNetCore.SignalR.Client 10.0.1 (*deprecated*)
- Microsoft.EntityFrameworkCore should be updated from `7.0.0` to `10.0.1` (*recommended for .NET 10.0*)
- Microsoft.EntityFrameworkCore.Design should be updated from `7.0.0` to `10.0.1` (*recommended for .NET 10.0*)
- Microsoft.EntityFrameworkCore.SqlServer should be updated from `7.0.0` to `10.0.1` (*recommended for .NET 10.0*)
- Microsoft.EntityFrameworkCore.Tools should be updated from `7.0.0` to `10.0.1` (*recommended for .NET 10.0*)
- Microsoft.AspNetCore.Authentication.OpenIdConnect should be updated from `7.0.0` to `10.0.1` (*recommended for .NET 10.0*)
- AutoMapper.Extensions.Microsoft.DependencyInjection 12.0.1 is deprecated (*functionality included in AutoMapper*)
