@echo off
setlocal
cd /d "%~dp0"

echo Building Recoiless...

set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"

if not exist "%CSC%" (
    echo Build Failed! Could not find .NET Framework csc.exe.
    exit /b 1
)

"%CSC%" /nologo /target:winexe /win32manifest:Recoiless.manifest /out:Recoiless.exe Recoiless.cs
if %errorlevel% neq 0 (
    echo Build Failed!
    exit /b %errorlevel%
)
echo Build Succeeded! Recoiless.exe generated.
