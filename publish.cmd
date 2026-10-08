@echo off
setlocal
cd /d "%~dp0"
set "OUT=%~dp0publish\FastDbExplorer"

if not exist "src\FastDbExplorer.Wpf\Assets\Map\fonts\NotoSans\0-255.pbf" (
  echo Map engine files are missing - downloading them first...
  call setup-map-assets.cmd nopause
  if errorlevel 1 goto :fail
)

echo [1/3] Running tests...
dotnet test tests\FastDbExplorer.Tests\FastDbExplorer.Tests.csproj -c Release --nologo
if errorlevel 1 goto :fail

echo [2/3] Publishing (self-contained, win-x64)...
if exist "%OUT%" rmdir /s /q "%OUT%"
dotnet publish src\FastDbExplorer.Wpf\FastDbExplorer.Wpf.csproj -c Release -r win-x64 --self-contained true -o "%OUT%" --nologo
if errorlevel 1 goto :fail

echo [3/3] Creating zip...
powershell -NoProfile -Command "Compress-Archive -Path '%OUT%\*' -DestinationPath '%~dp0publish\FastDbExplorer-win-x64.zip' -Force"
if errorlevel 1 goto :fail

echo.
echo Done. Portable folder: %OUT%
echo Run: %OUT%\FastDbExplorer.exe
pause
exit /b 0

:fail
echo.
echo FAILED. See the messages above.
pause
exit /b 1
