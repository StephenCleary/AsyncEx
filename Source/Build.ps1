$ErrorActionPreference = "Stop"

Write-Output "Solution MUST be built by hand in Release mode!"

# Build NuGet packages
nuget pack -Symbols "Nito.AsyncEx.DataflowProxy.nuspec"
nuget pack -Symbols "Nito.AsyncEx.Dataflow.nuspec"
nuget pack -Symbols "Nito.AsyncEx.nuspec"

# Strong-name the dlls
$key = Import-StrongNameKeyPair Nito.AsyncEx.snk
Set-StrongName -KeyPair $key -Force -NoBackup "Nito.AsyncEx (NET45, Win8, WP8, WPA81)\bin\Release\Nito.AsyncEx.dll"
Set-StrongName -KeyPair $key -Force -NoBackup "Nito.AsyncEx (NET4, Win8, SL5, WP8, WPA81)\bin\Release\Nito.AsyncEx.dll"
Set-StrongName -KeyPair $key -Force -NoBackup "Nito.AsyncEx.Concurrent (NET45, Win8, WPA81)\bin\Release\Nito.AsyncEx.Concurrent.dll"
Set-StrongName -KeyPair $key -Force -NoBackup "Nito.AsyncEx.Concurrent (NET4, Win8)\bin\Release\Nito.AsyncEx.Concurrent.dll"
Set-StrongName -KeyPair $key -Force -NoBackup "Nito.AsyncEx.Dataflow (NET45, Win8, WP8, WPA81)\bin\Release\Nito.AsyncEx.Dataflow.dll"
Set-StrongName -KeyPair $key -Force -NoBackup "Nito.AsyncEx.Dataflow (NET40)\bin\Release\Nito.AsyncEx.Dataflow.dll"
Set-StrongName -KeyPair $key -Force -NoBackup "Enlightenment\Nito.AsyncEx.Enlightenment (NET45)\bin\Release\Nito.AsyncEx.Enlightenment.dll"
Set-StrongName -KeyPair $key -Force -NoBackup "Enlightenment\Nito.AsyncEx.Enlightenment (Win81)\bin\Release\Nito.AsyncEx.Enlightenment.dll"
Set-StrongName -KeyPair $key -Force -NoBackup "Enlightenment\Nito.AsyncEx.Enlightenment (WPA81)\bin\Release\Nito.AsyncEx.Enlightenment.dll"
Set-StrongName -KeyPair $key -Force -NoBackup "Enlightenment\Nito.AsyncEx.Enlightenment (NET40)\bin\Release\Nito.AsyncEx.Enlightenment.dll"
Set-StrongName -KeyPair $key -Force -NoBackup "Enlightenment\Nito.AsyncEx.Enlightenment (SL5)\bin\Release\Nito.AsyncEx.Enlightenment.dll"
Set-StrongName -KeyPair $key -Force -NoBackup "Enlightenment\Nito.AsyncEx.Enlightenment (WP8)\bin\Release\Nito.AsyncEx.Enlightenment.dll"

# Create StrongName NuSpecs
(Get-Content "Nito.AsyncEx.nuspec") | Foreach-Object { $_ -Replace "<id>Nito.AsyncEx</id>", "<id>Nito.AsyncEx.StrongNamed</id>" } | Set-Content "Nito.AsyncEx.StrongNamed.nuspec"
(Get-Content "Nito.AsyncEx.Dataflow.nuspec") | Foreach-Object { $_ -Replace "<id>Nito.AsyncEx.Dataflow</id>", "<id>Nito.AsyncEx.Dataflow.StrongNamed</id>" } | Set-Content "Nito.AsyncEx.Dataflow.StrongNamed.nuspec"

# Build strong-named packages
nuget pack -Symbols "Nito.AsyncEx.Dataflow.StrongNamed.nuspec"
nuget pack -Symbols "Nito.AsyncEx.StrongNamed.nuspec"
