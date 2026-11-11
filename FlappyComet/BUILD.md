# Сборка Flappy Comet

## Задание 1 — автоматическая сборка
Скрипт: `Assets/Editor/BuildScript.cs`

Меню **Build** в редакторе:
- `Build/Windows` → `Builds/Windows/FlappyComet.exe`
- `Build/Android` → `Builds/Android/FlappyComet.apk`
- `Build/WebGL` → `Builds/WebGL/`
- `Build/Build All` — все три платформы по очереди

Сцены берутся из Build Settings (только включённые). В Console пишутся логи с префиксом `[BuildScript]`.

CLI (или просто запусти `build_all.bat`):
```
"C:\Program Files\Unity\Hub\Editor\6000.3.8f1\Editor\Unity.exe" -quit -batchmode -nographics -projectPath "." -executeMethod FlappyComet.EditorTools.BuildScript.BuildAll -logFile Logs/build_all.log
```

## Задание 2 — кастомный WebGL Template
- Шаблон: [better-unity-webgl-template](https://github.com/greggman/better-unity-webgl-template), лежит в `Assets/WebGLTemplates/Better2020/`
- Выбран в **Project Settings → Player → Resolution and Presentation → WebGL Template → Better2020**
  (скрипт сборки дополнительно выставляет его перед WebGL-билдом)

Проверка: запустить локальный сервер в папке билда
```
npx http-server Builds/WebGL
```
и открыть http://localhost:8080 — игра на весь экран, без стандартной плашки Unity внизу, со спиннером при загрузке.

## Требования
Модули в Unity Hub: Windows Build Support, Android Build Support (+ OpenJDK, SDK & NDK), WebGL Build Support.
