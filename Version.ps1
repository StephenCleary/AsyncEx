param([String]$oldVersion="", [String]$newVersion="")
iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/StephenCleary/BuildTools/3823868f19eb286d490e3d7304bfd25cf317d015/Version.ps1'))
