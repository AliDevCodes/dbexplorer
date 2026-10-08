@echo off
title DBExplorer - Push To GitHub

echo =====================================
echo   DBExplorer GitHub Push Script
echo =====================================
echo.

REM Go to script directory (project root)
cd /d "%~dp0"

echo Current folder:
echo %CD%
echo.

REM Check Git
git --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Git is not installed.
    pause
    exit /b 1
)

REM Initialize repository if needed
if not exist ".git" (
    echo Initializing git repository...
    git init
)

echo.

REM Configure remote
echo Setting remote origin...

git remote remove origin >nul 2>&1

git remote add origin https://github.com/AliDevCodes/dbexplorer.git

echo.

REM Switch branch
echo Switching to feature/nav-shell branch...

git checkout -B feature/nav-shell

echo.

REM Add files
echo Adding files...

git add .

echo.

REM Commit
echo Creating commit...

git commit -m "Update DBExplorer nav-shell feature"

echo.

REM Push
echo Pushing to GitHub...

git push -u origin feature/nav-shell --force

echo.

if errorlevel 1 (
    echo =====================================
    echo PUSH FAILED
    echo Check authentication or repository access.
    echo =====================================
) else (
    echo =====================================
    echo PUSH COMPLETED SUCCESSFULLY
    echo =====================================
)

pause