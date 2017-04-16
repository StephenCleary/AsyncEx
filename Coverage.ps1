$testProjectLocations = @('test/AsyncEx.Tasks.UnitTests', 'test/AsyncEx.Coordination.UnitTests', 'test/AsyncEx.Context.UnitTests', 'test/AsyncEx.Oop.UnitTests', 'test/AsyncEx.Interop.WaitHandles.UnitTests')
$outputLocation = 'testResults'
iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/StephenCleary/BuildTools/f27313f986ce2b26b091b87649771b0bcee0c30c/Coverage.ps1'))
