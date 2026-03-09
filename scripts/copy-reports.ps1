$source = Join-Path -Path $PSScriptRoot -ChildPath "..\MediaOrganizerApp.WebApi\Reports_RDLC"
$target = Join-Path -Path $PSScriptRoot -ChildPath "..\ReportsRenderer\Reports_RDLC"

if (-Not (Test-Path $target)) { New-Item -ItemType Directory -Path $target -Force }

Get-ChildItem -Path $source -Filter *.rdlc -File | ForEach-Object {
    Copy-Item -Path $_.FullName -Destination $target -Force
    Write-Host "Copied: $($_.Name)"
}
Write-Host "Done copying RDLC files."