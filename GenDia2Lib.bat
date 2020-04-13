@echo off
title DIA Binding Generator

if not defined VSINSTALLDIR (
    echo This script must be called from a Visual Studio developer command prompt.
    echo Press any key to exit...
    pause >nul
    exit
)

if not exist "%VSINSTALLDIR%\DIA SDK" (
    echo The DIA SDK could not be found. Please install the C++ native tools for Visual Studio.
    echo Press any key to exit...
    pause >nul
    exit
)

echo Generating type library...
set DIA=%VSINSTALLDIR%\DIA SDK
midl /I "%DIA%\idl;%DIA%\include" dia2.idl /tlb "%TEMP%\dia2.tlb" /header nul /iid nul /proxy nul /dlldata nul >nul 2>&1

echo Generating binding assembly...
tlbimp "%TEMP%\dia2.tlb" /out:Assets\Plugins\Managed\Dia2Lib.dll >nul 2>&1

echo Copying unmanaged DIA libraries...
copy /y "%DIA%\bin\msdia*.dll" "Assets\Plugins\x86" >nul
copy /y "%DIA%\bin\amd64\msdia*.dll" "Assets\Plugins\x86_64" >nul
