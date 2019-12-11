#iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/StephenCleary/BuildTools/6efa9f040c0622d60dc167947b7d35b55d071f25/Coverage.ps1'))

# Expected variables:
#   $testProjectLocations (optional) - an array of relative paths to projects that can run "dotnet test". Defaults to all subdirectories under 'test'.
#   $outputLocation (optional) - the relative path where test results should be stored. This path does not have to exist. Defaults to 'testResults'.
#   $dotnetTestArgs (optional) - additional arguments to pass to "dotnet test". Defaults to the empty string.
#   $toolsLocation (optional) - the relative path where tools are stored. This path does not have to exist. Defaults to 'tools'.

$ErrorActionPreference = "Stop"

Function DefaultValue($value, $default) {
	if ($null -eq $value) { $default } else { $value }
}

Function EndPathInSlash([string]$path) {
	if ($path[-1] -ne '/' -and $path[-1] -ne '\') { $path + '/' } else { $path }
}

$toolsLocation = DefaultValue $toolsLocation 'tools\'
$toolsLocation = EndPathInSlash $toolsLocation
$outputLocation = DefaultValue $outputLocation 'testResults\'
$outputLocation = EndPathInSlash $outputLocation
if ($null -eq $testProjectLocations) {
  $testProjectLocations = Get-ChildItem 'test' | ForEach-Object FullName
} else {
	for ($i = 0; $i -ne $testProjectLocations.length; ++$i) {
		$testProjectLocations[$i] = (Resolve-Path $testProjectLocations[$i]).Path
	}
}

Function ResolveAndForcePath([string]$relativePath) {
	New-Item -ItemType Directory -Force $relativePath | Out-Null
	return (Resolve-Path $relativePath).Path
}

Function WriteAndExecute([string]$command) {
	Write-Output $command
	Invoke-Expression $command
}

$toolsPath = ResolveAndForcePath $toolsLocation
$outputPath = ResolveAndForcePath $outputLocation
$mergeFile = Join-Path $outputPath -childpath 'coverage.json'
$uploadFile = Join-Path $outputPath -childpath 'coverage.opencover.xml'

Remove-Item ($outputPath + '*') -Force -Recurse

Write-Output $toolsPath
Write-Output $outputPath
Write-Output $mergeFile
Write-Output $uploadFile
Write-Output ($testProjectLocations -join ', ')

Push-Location
try {
	# Run the tests
	foreach ($testProjectLocation in $testProjectLocations) {
		Set-Location $testProjectLocation
		WriteAndExecute "dotnet test /p:CollectCoverage=true /p:Include=`"[Nito.*]*`" /p:ExcludeByAttribute=System.Diagnostics.DebuggerNonUserCodeAttribute /p:CoverletOutput=`"${outputPath}`" /p:MergeWith=`"${mergeFile}`" /p:CoverletOutputFormat=opencover%2Cjson ${dotnetTestArgs}"
	}

	# Publish the results
	if ($env:CI -eq 'True') {
		WriteAndExecute "dotnet tool install coveralls.net --tool-path `"${toolsPath}`""
		WriteAndExecute ". `"${toolsPath}csmacnz.Coveralls`" --opencover -i `"${uploadFile}`""
	} else {
		WriteAndExecute "dotnet tool install dotnet-reportgenerator-globaltool --tool-path `"${toolsPath}`""
		WriteAndExecute ". `"${toolsPath}reportgenerator`" -reports:`"${uploadFile}`" -targetdir:`"${outputPath}`""
		Set-Location $outputPath
		./index.htm
	}
} finally { Pop-Location }