
Push-Location $PSScriptRoot/../src/aspnetcore

Set-Location .\src\Servers\Kestrel\
&cmd /U /C "build.cmd -NoRestore -NoBuildNodeJS -NoBuildJava -c Release"

Pop-Location
