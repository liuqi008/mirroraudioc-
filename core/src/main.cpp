
#include "AudioEngine.h"
#include "IPC.h"
#include "WinUtils.h"
#include <windows.h>
#include <objbase.h>
#include <string>
#include <thread>
#include <atomic>
#include <fstream>

static std::atomic<bool> log_enabled{false};
static std::wofstream log_file;
static void logw(const std::wstring& s){
    if (!log_enabled) return;
    if (!log_file.is_open()){
        wchar_t path[MAX_PATH]{};
        GetModuleFileNameW(nullptr, path, MAX_PATH);
        std::wstring p(path);
        auto pos = p.find_last_of(L"\/");
        if (pos != std::wstring::npos) p = p.substr(0,pos+1) + L"mirroraudio_core.log";
        else p = L"mirroraudio_core.log";
        log_file.open(p, std::ios::app);
    }
    log_file << s << std::endl;
}

int wmain(int argc, wchar_t** argv) {
    CoInitializeEx(nullptr, COINIT_MULTITHREADED);
    logw(L"[MirrorAudioCore] starting...");

    AudioEngine engine;
    if (!engine.init()) {
        logw(L"[MirrorAudioCore] init failed.");
        return 1;
    }

    static const wchar_t* kPipeName = L"\\.\pipe\MirrorAudioSettings";
    std::atomic<bool> running{ true };
    NamedPipeServer server(std::wstring(kPipeName), [&](const std::wstring& msg){
        // logging toggle
        if (msg.find(L"\"log\"") != std::wstring::npos || msg.find(L""log"") != std::wstring::npos){
            if (msg.find(L"true") != std::wstring::npos) { log_enabled = true; logw(L"[log] enabled"); }
            else if (msg.find(L"false") != std::wstring::npos) { logw(L"[log] disabled"); log_enabled = false; }
            std::wstring resp = L"{"ok":true,"log":"; resp += (log_enabled ? L"true" : L"false"); resp += L"}";
            return resp;
        }
        // heartbeat
        if (msg.find(L"PING") != std::wstring::npos || msg.find(L"ping") != std::wstring::npos){
            logw(L"[ping] pong");
            return std::wstring(L"{"ok":true,"pong":1}");
        }
        // other control
        engine.onControlMessage(msg);
        return std::wstring(L"{"ok":true}");
    });

    std::thread pipeThread([&]{ server.run(running); });

    for (int i=1;i<argc;i++) {
        if (std::wstring(argv[i])==L"--auto-start") { engine.start(); break; }
    }

    logw(L"[MirrorAudioCore] ready.");

    while (running) {
        std::this_thread::sleep_for(std::chrono::milliseconds(200));
        if (!server.is_ok()) break;
    }

    engine.stop();
    running=false;
    if (pipeThread.joinable()) pipeThread.join();
    CoUninitialize();
    return 0;
}
