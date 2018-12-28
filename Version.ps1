param([String]$oldVersion="", [String]$newVersion="")
iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/StephenCleary/BuildTools/2e751627d6c09f2e3aa323cea8dd39bd880009fd/Version.ps1'))
