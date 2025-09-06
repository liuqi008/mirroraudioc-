
#pragma once
#include <string>
#include <atomic>
struct IAudioClient3;
struct IAudioRenderClient;
struct IAudioCaptureClient;
struct IMMDevice;
struct IMMDeviceEnumerator;

struct EngineConfig {
    bool exclusive = true;
    bool raw = true;
    int  sample_rate = 48000;
    int  bits = 24;
    int  channels = 2;
    int  period_us = 3000;
    bool autostart = false;
    bool force_passthrough = false;
    std::wstring render_device_id;
    std::wstring capture_device_id;
};

class AudioEngine {
public:
    bool init();
    void start();
    void stop();
    void onControlMessage(const std::wstring& json_utf16);
private:
    bool createDevices();
    bool createClients();
    void renderLoop();
    void teardown();

    std::atomic<bool> running_{false};
    EngineConfig cfg_;

    IMMDeviceEnumerator* enum_ = nullptr;
    IMMDevice* devRender_ = nullptr;
    IMMDevice* devCapture_ = nullptr;
    IAudioClient3* acRender_ = nullptr;
    IAudioClient3* acCapture_ = nullptr;
    IAudioRenderClient* rc_ = nullptr;
    IAudioCaptureClient* cc_ = nullptr;
};
