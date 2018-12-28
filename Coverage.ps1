$testProjectLocations = @('test/AsyncEx.Tasks.UnitTests', 'test/AsyncEx.Coordination.UnitTests', 'test/AsyncEx.Context.UnitTests', 'test/AsyncEx.Oop.UnitTests', 'test/AsyncEx.Interop.WaitHandles.UnitTests')
$outputLocation = 'testResults'
iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/StephenCleary/BuildTools/2e751627d6c09f2e3aa323cea8dd39bd880009fd/Coverage.ps1'))
