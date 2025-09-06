
cmake_minimum_required(VERSION 3.20)
project(MirrorAudioCore LANGUAGES CXX)
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

add_executable(mirroraudio_core WIN32
  src/main.cpp
  src/entry_win.cpp
  src/AudioEngine.cpp
  src/AudioEngine.h
  src/IPC.cpp
  src/IPC.h
  src/WinUtils.h
)

if (WIN32)
  target_compile_definitions(mirroraudio_core PRIVATE UNICODE _UNICODE WIN32_LEAN_AND_MEAN NOMINMAX _WIN32_WINNT=0x0A00 NTDDI_VERSION=0x0A000000)
  target_link_libraries(mirroraudio_core PRIVATE ole32 avrt Mmdevapi uuid)
endif()
