
#include "IPC.h"
#include "WinUtils.h"
#include <vector>

NamedPipeServer::NamedPipeServer(const std::wstring& pipe_name, Handler handler)
: name_(pipe_name), handler_(std::move(handler)){}

void NamedPipeServer::run(std::atomic<bool>& running){
    while(running){
        HANDLE hPipe = CreateNamedPipeW(name_.c_str(), PIPE_ACCESS_DUPLEX,
            PIPE_TYPE_MESSAGE|PIPE_READMODE_MESSAGE|PIPE_WAIT,
            1, 64*1024, 64*1024, 0, nullptr);
        if (hPipe==INVALID_HANDLE_VALUE){ ok_=false; break; }
        BOOL connected = ConnectNamedPipe(hPipe, nullptr) ? TRUE : (GetLastError()==ERROR_PIPE_CONNECTED);
        if (!connected){ CloseHandle(hPipe); continue; }

        std::wstring req;
        wchar_t buf[1024];
        DWORD read=0;
        while (ReadFile(hPipe, buf, sizeof(buf), &read, nullptr)){
            if (read==0) break;
            req.append(buf, buf + read/sizeof(wchar_t));
            if (read < sizeof(buf)) break;
        }

        std::wstring resp;
        try { resp = handler_? handler_(req) : L"{"ok":true}"; }
        catch(...) { resp = L"{"ok":false}"; }

        DWORD written=0;
        WriteFile(hPipe, resp.c_str(), (DWORD)((resp.size()+1)*sizeof(wchar_t)), &written, nullptr);
        FlushFileBuffers(hPipe);
        DisconnectNamedPipe(hPipe);
        CloseHandle(hPipe);
    }
}
