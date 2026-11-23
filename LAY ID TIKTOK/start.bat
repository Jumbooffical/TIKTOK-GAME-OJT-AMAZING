@echo off
chcp 65001 >nul
title TikTok Live Dashboard
cd /d "%~dp0"

echo.
echo  ============================================
echo    TIKTOK LIVE DASHBOARD
echo  ============================================
echo.

:: Kiem tra Python co san khong
py --version >nul 2>&1
if errorlevel 1 (
    echo  [LOI] Khong tim thay Python!
    echo.
    echo  Vui long cai Python truoc khi chay:
    echo  https://www.python.org/downloads/
    echo.
    echo  Luu y: Tich vao "Add Python to PATH" khi cai!
    echo.
    pause
    start https://www.python.org/downloads/
    exit /b 1
)

echo  [1/2] Dang kiem tra va cai thu vien...
py -m pip install -r requirements.txt --quiet --disable-pip-version-check
if errorlevel 1 (
    echo  [LOI] Khong the cai thu vien. Kiem tra ket noi Internet.
    pause
    exit /b 1
)
echo  [1/2] Thu vien san sang!

echo  [2/2] Dang khoi dong server...
echo.
echo  ============================================
echo   Dashboard dang chay tai: http://127.0.0.1:8000
echo   Nhan Ctrl+C de tat chuong trinh
echo  ============================================
echo.

:: Mo trinh duyet
timeout /t 2 /nobreak >nul
start "" http://127.0.0.1:8000

:: Chay server
py -m uvicorn main:app --host 127.0.0.1 --port 8000

echo.
echo  Server da tat.
pause
