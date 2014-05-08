$ErrorActionPreference = "Stop"

# Find NuGet.exe
if (Test-Path '.nuget\nuget.exe') {
  $nuget = Get-ChildItem '.nuget' -Filter 'nuget.exe'
}
else {
  $nuget = Get-ChildItem '..\Util' -Filter 'nuget.exe'
}

&$nuget.FullName update -Self
