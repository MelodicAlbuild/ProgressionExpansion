REM echo "Starting"
REM ping -n 2 localhost

REM TODO: If proper Unity version is installed, launch it directly?

REM Read ProjectVersion.txt and create unityhub link in format: unityhub://2021.1.16f1/5fa502fca597
set VERSION_FILE="%~dp0\..\ProjectSettings\ProjectVersion.txt"
for /F "tokens=2,3 delims=() " %%A in ('findstr m_EditorVersionWithRevision %VERSION_FILE%') do (
	set UNITY_VERSION=%%A
    set HUB_LINK=unityhub://%%A/%%B
)

REM Gracefull close UnityHub main window (Hub will keep running in tray), project list will be refreshed when window is reopened
taskkill /IM "Unity Hub.exe" /FI "WINDOWTITLE ne N/A"

REM Change to parent dir and add project into UnityHub through registry
pushd %~dp0\..
reg add "HKCU\SOFTWARE\Unity Technologies\Unity Editor 5.x" /f /v RecentlyUsedProjectPaths-10_h1446599995 /t REG_SZ /d "%CD%"
popd

REM Start UnityHub
REM - when correct version is installed use exe to start it up
REM - otherwise use hub link (hub link always opens 'Installs' page)
reg query "HKCU\Software\Unity Technologies\Installer\Unity %UNITY_VERSION%" >nul 2>nul
if %ERRORLEVEL% EQU 0 (
    REM Required unity version is installed, run hub normally
    start "" "C:\Program Files\Unity Hub\Unity Hub.exe"
) else (
    REM Required version not installed, run hub link to open installation page in the hub
    start "" "%HUB_LINK%"
)

REM There should be some command after 'start' to prevent window being hooked to the started process
EXIT