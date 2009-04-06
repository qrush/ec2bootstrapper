
On Error Resume Next

Set objFSO = CreateObject("Scripting.FileSystemObject")
if objFSO is nothing then
   'wscript.Echo "Cannot create Scripting.FileSystemObject"
   Wscript.Quit   
end if

if NOT objFSO.FileExists("C:\Scripts\MakeSslBinding.cmd") then
    Wscript.Quit
end if

const Address = "http://169.254.169.254/2008-09-01/meta-data/public-hostname"

set xmlhttp = CreateObject("Microsoft.XMLHTTP")
if xmlhttp is nothing then
   'Wscript.Echo "cannot create Microsoft.XMLHTTP."
   Wscript.Quit
end if

xmlhttp.open "GET", Address, false

xmlhttp.send

dim objshell
set objshell = CreateObject("WScript.shell")

if objShell is nothing then
   'wscript.Echo "Cannot create WScript.shell"
   Wscript.Quit   
end if

objshell.run("MakeSslBinding.cmd " & xmlhttp.responseText)


