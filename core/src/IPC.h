
#pragma once
#include <windows.h>
#include <string>
#include <functional>
#include <atomic>

class NamedPipeServer {
public:
    using Handler = std::function<std::wstring(const std::wstring&)>;
    NamedPipeServer(const std::wstring& pipe_name, Handler handler);
    void run(std::atomic<bool>& running);
    bool is_ok() const { return ok_; }
private:
    std::wstring name_;
    Handler handler_;
    std::atomic<bool> ok_{true};
};
