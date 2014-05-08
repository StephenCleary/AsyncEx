Param($version = "0.0.0", $extraVersion = "")
$ErrorActionPreference = "Stop"

if ($version -eq "0.0.0") {
  throw "Pass a version number to this script."
}

# Set environment variables for Visual Studio Command Prompt
if (Test-Path 'C:\Program Files\Microsoft Visual Studio 12.0\VC') {
  pushd 'C:\Program Files\Microsoft Visual Studio 12.0\VC'
}
else {
  pushd 'C:\Program Files (x86)\Microsoft Visual Studio 12.0\VC'
}

cmd /c "vcvarsall.bat&set" |
foreach {
  if ($_ -match "=") {
    $v = $_.split("="); set-item -force -path "ENV:\$($v[0])" -value "$($v[1])"
  }
}
popd

# Find NuGet.exe
if (Test-Path '.nuget\nuget.exe') {
  $nuget = Get-ChildItem '.nuget' -Filter 'nuget.exe'
}
else {
  $nuget = Get-ChildItem '..\Util' -Filter 'nuget.exe'
}

# Write out version info for dll
"using System.Reflection;`r`n[assembly: AssemblyVersion(`"$version`")]`r`n" > 'AssemblyVersion.cs'

# Build solution
$solution = Get-ChildItem '.' -Filter '*.sln'
devenv $solution /rebuild Release | Write-Output

# Create binaries directory if necessary
if (!(Test-Path '..\Binaries')) {
  New-Item '..\Binaries' -type directory | Out-Null
}

# Build NuGet package
if ($extraVersion -Eq "") {
  $fullVersion = $version
} else {
  $fullVersion = $version + "-" + $extraVersion
}

&$nuget.FullName pack -Symbols 'Nito.AsyncEx.DataflowProxy.nuspec' -OutputDirectory ..\Binaries
&$nuget.FullName pack -Symbols 'Nito.AsyncEx.Dataflow.nuspec' -Version $fullVersion -OutputDirectory ..\Binaries
&$nuget.FullName pack -Symbols 'Nito.AsyncEx.nuspec' -Version $fullVersion -OutputDirectory ..\Binaries

# Create a tag.
# hg tag $version

"Built " + $nuspec.BaseName + " version $fullVersion"