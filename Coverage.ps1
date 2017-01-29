$testProjectLocations = @('test/UnitTests', 'test/Linq.UnitTests', 'test/Ix.UnitTests', 'test/Rx.UnitTests')
$outputLocation = 'testResults'
iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/StephenCleary/BuildTools/599beba35b53f495d4df6e5c323573aa839137a3/Coverage.ps1'))
