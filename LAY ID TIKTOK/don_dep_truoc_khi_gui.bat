@echo off
chcp 65001 >nul
cd /d "%~dp0"
echo Dang xoa file tham...
if exist __pycache__ rmdir /s /q __pycache__
if exist tiktok_live.db del /q tiktok_live.db
echo Xong! Thu muc da sach, san sang de gui cho ban.
pause
