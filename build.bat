@echo off
echo Building Recoiless...
"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /target:winexe /win32manifest:Recoiless.manifest /out:Recoiless.exe Recoiless.cs
if %errorlevel% neq 0 (
    echo Build Failed!
    exit /b %errorlevel%
)
echo Build Succeeded! Recoiless.exe generated.
