
#pragma once
#include <windows.h>
#include <string>
inline std::wstring werr(DWORD code=GetLastError()){
    wchar_t* buf=nullptr;
    FormatMessageW(FORMAT_MESSAGE_ALLOCATE_BUFFER|FORMAT_MESSAGE_FROM_SYSTEM|FORMAT_MESSAGE_IGNORE_INSERTS,
        nullptr, code, MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), (LPWSTR)&buf, 0, nullptr);
    std::wstring s = buf?buf:L"unknown";
    if(buf) LocalFree(buf);
    return s;
}
