
# MirrorAudio Suite (C# UI + C++ Core)

- **C++ Core**: WASAPI 内核，Windows 子系统（无控制台），命名管道 `\\.\pipe\MirrorAudioSettings`，支持 `PING` 心跳与 `{"log":true|false}` 日志开关。
- **C# UI**: WinForms（.NET 8），CoreHost 托管内核（隐藏窗口启动、JobObject 随托盘退出、自恢复、日志切换）。

## 本地构建
```powershell
# C++ core
cmake -S core -B core/build -G "Visual Studio 17 2022" -A x64
cmake --build core/build --config Release --parallel

# C# UI (需 .NET 8 SDK)
copy core\build\Release\mirroraudio_core.exe ui\
dotnet publish ui\MirrorAudio.UI.csproj -c Release -o out
```

## GitHub Actions
推送后自动：先构建 Core，再发布 UI，并将 `mirroraudio_core.exe` 一并打入最终产物。产物见 workflow artifacts。

## 运行
发布目录中，直接运行 `MirrorAudio.UI.exe`。UI 按钮：**启动/停止/应用**，并可勾选 **独占/RAW/强制直通/启用日志**。
