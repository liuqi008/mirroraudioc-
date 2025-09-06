
#include <windows.h>
#include <shellapi.h>
int wmain(int argc, wchar_t** argv);
int APIENTRY wWinMain(HINSTANCE, HINSTANCE, LPWSTR, int) {
    int argc = 0;
    LPWSTR* wargv = CommandLineToArgvW(GetCommandLineW(), &argc);
    int ret = wmain(argc, wargv);
    if (wargv) LocalFree(wargv);
    return ret;
}
