$ErrorActionPreference = "Stop"

# Build VS2013 solution
Write-Output "Building VS2013 Solution..."
$project = Get-Project
$build = $project.DTE.Solution.SolutionBuild
$oldConfiguration = $build.ActiveConfiguration
$build.SolutionConfigurations.Item("Release").Activate()
$build.Build($true)
$oldConfiguration.Activate()
Write-Output "... done building VS2013 Solution."

# $p.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value == "bin\Debug\"
# $p.Properties.Item("FullPath").Value == "D:\Personal\AsyncEx-git\Source\Nito.AsyncEx.Dataflow (NET45, Win8, WP8, WPA81)\"

# Build VS2012 solution
Write-Output "Building VS2012 Solution..."
$buildVS2012 = "`"" + $env:VS110COMNTOOLS + "VsDevCmd.bat" + "`" && devenv VS2012\VS2012.sln /rebuild Release"
cmd /c $buildVS2012
Write-Output "... done building VS2012 Solution."

nuget pack -Symbols "Nito.AsyncEx.DataflowProxy.nuspec"
nuget pack -Symbols "Nito.AsyncEx.Dataflow.nuspec"
nuget pack -Symbols "Nito.AsyncEx.nuspec"
