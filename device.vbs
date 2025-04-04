Set dToggleShell = CreateObject ("Wscript.Shell")
Dim strArgs
strArgs = "cmd /c device.bat"
dToggleShell.Run strArgs, 0, false