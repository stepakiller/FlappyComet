@echo off
rem Сборка всех платформ из командной строки (без открытия редактора).
rem Перед запуском закрой Unity с этим проектом.
chcp 65001 >nul
set UNITY="C:\Program Files\Unity\Hub\Editor\6000.3.8f1\Editor\Unity.exe"
set PROJECT=%~dp0

%UNITY% -quit -batchmode -nographics -projectPath "%PROJECT%" -executeMethod FlappyComet.EditorTools.BuildScript.BuildAll -logFile "%PROJECT%Logs\build_all.log"

if %ERRORLEVEL% neq 0 (
    echo Сборка завершилась с ошибкой. Смотри Logs\build_all.log
) else (
    echo Готово! Билды в папке Builds\
)
pause
