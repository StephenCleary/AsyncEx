$ErrorActionPreference = "Stop"

# Build VS2013 solution
Write-Output "Building VS2013 Solution..."
$project = Get-Project
$build = $project.DTE.Solution.SolutionBuild
$oldConfiguration = $build.ActiveConfiguration
$build.SolutionConfigurations.Item("Release").Activate()
$build.Clean($true)
$build.Build($true)
$oldConfiguration.Activate()
Write-Output "... done building VS2013 Solution."

# Build VS2012 solution
Write-Output "Building VS2012 Solution..."
$cleanVS2012 = "`"" + $env:VS110COMNTOOLS + "VsDevCmd.bat" + "`" && devenv VS2012\VS2012.sln /Clean"
cmd /c $cleanVS2012
if ($LastExitCode -ne 0) { throw "Build error." }
$buildVS2012 = "`"" + $env:VS110COMNTOOLS + "VsDevCmd.bat" + "`" && devenv VS2012\VS2012.sln /rebuild Release"
cmd /c $buildVS2012
if ($LastExitCode -ne 0) { throw "Build error." }
Write-Output "... done building VS2012 Solution."

# Build NuGet packages
nuget pack -Symbols "Nito.AsyncEx.DataflowProxy.nuspec"
nuget pack -Symbols "Nito.AsyncEx.Dataflow.nuspec"
nuget pack -Symbols "Nito.AsyncEx.nuspec"

# Strong-name the dlls
$key = Import-StrongNameKeyPair Nito.AsyncEx.snk
Set-StrongName -KeyPair $key -Force -NoBackup "Nito.AsyncEx (NET45, Win8, WP8, WPA81)\bin\Release\Nito.AsyncEx.dll"
Set-StrongName -KeyPair $key -Force -NoBackup "VS2012\Nito.AsyncEx (NET4, Win8, SL4, WP71)\bin\Release\Nito.AsyncEx.dll"
Set-StrongName -KeyPair $key -Force -NoBackup "Nito.AsyncEx (NET4, Win8, SL5, WP8, WPA81)\bin\Release\Nito.AsyncEx.dll"
Set-StrongName -KeyPair $key -Force -NoBackup "VS2012\Nito.AsyncEx (NET4, Win8, SL4, WP71)\bin\Release\Nito.AsyncEx.dll"
Set-StrongName -KeyPair $key -Force -NoBackup "Nito.AsyncEx.Concurrent (NET45, Win8, WPA81)\bin\Release\Nito.AsyncEx.Concurrent.dll"
Set-StrongName -KeyPair $key -Force -NoBackup "Nito.AsyncEx.Concurrent (NET4, Win8)\bin\Release\Nito.AsyncEx.Concurrent.dll"
Set-StrongName -KeyPair $key -Force -NoBackup "Nito.AsyncEx.Dataflow (NET45, Win8, WP8, WPA81)\bin\Release\Nito.AsyncEx.Dataflow.dll"
Set-StrongName -KeyPair $key -Force -NoBackup "Nito.AsyncEx.Dataflow (NET40)\bin\Release\Nito.AsyncEx.Dataflow.dll"
Set-StrongName -KeyPair $key -Force -NoBackup "Enlightenment\Nito.AsyncEx.Enlightenment (NET45)\bin\Release\Nito.AsyncEx.Enlightenment.dll"
Set-StrongName -KeyPair $key -Force -NoBackup "Enlightenment\Nito.AsyncEx.Enlightenment (Win8)\bin\Release\Nito.AsyncEx.Enlightenment.dll"
Set-StrongName -KeyPair $key -Force -NoBackup "Enlightenment\Nito.AsyncEx.Enlightenment (WPA81)\bin\Release\Nito.AsyncEx.Enlightenment.dll"
Set-StrongName -KeyPair $key -Force -NoBackup "Enlightenment\Nito.AsyncEx.Enlightenment (NET40)\bin\Release\Nito.AsyncEx.Enlightenment.dll"
Set-StrongName -KeyPair $key -Force -NoBackup "Enlightenment\Nito.AsyncEx.Enlightenment (SL5)\bin\Release\Nito.AsyncEx.Enlightenment.dll"
Set-StrongName -KeyPair $key -Force -NoBackup "Enlightenment\Nito.AsyncEx.Enlightenment (WP8)\bin\Release\Nito.AsyncEx.Enlightenment.dll"
Set-StrongName -KeyPair $key -Force -NoBackup "VS2012\Enlightenment\Nito.AsyncEx.Enlightenment (SL4)\Bin\Release\Nito.AsyncEx.Enlightenment.dll"
Set-StrongName -KeyPair $key -Force -NoBackup "VS2012\Enlightenment\Nito.AsyncEx.Enlightenment (WP71)\Bin\Release\Nito.AsyncEx.Enlightenment.dll"

# Create StrongName NuSpecs
(Get-Content "Nito.AsyncEx.nuspec") | Foreach-Object { $_ -Replace "<id>Nito.AsyncEx</id>", "<id>Nito.AsyncEx.StrongNamed</id>" } | Set-Content "Nito.AsyncEx.StrongNamed.nuspec"
(Get-Content "Nito.AsyncEx.Dataflow.nuspec") | Foreach-Object { $_ -Replace "<id>Nito.AsyncEx.Dataflow</id>", "<id>Nito.AsyncEx.Dataflow.StrongNamed</id>" } | Set-Content "Nito.AsyncEx.Dataflow.StrongNamed.nuspec"

# Build strong-named packages
nuget pack -Symbols "Nito.AsyncEx.Dataflow.StrongNamed.nuspec"
nuget pack -Symbols "Nito.AsyncEx.StrongNamed.nuspec"
