
#define _WIN32_DCOM
#include "AudioEngine.h"
#include "WinUtils.h"
#include <mmdeviceapi.h>
#include <audioclient.h>
#include <functiondiscoverykeys_devpkey.h>
#include <avrt.h>
#include <objbase.h>
#include <sdkddkver.h>
#ifndef AUDCLNT_STREAMFLAGS_RAW
#define AUDCLNT_STREAMFLAGS_RAW 0x00000020
#endif
#include <thread>
#include <vector>

bool AudioEngine::init(){
    HRESULT hr = CoCreateInstance(__uuidof(MMDeviceEnumerator), nullptr, CLSCTX_ALL, IID_PPV_ARGS(&enum_));
    if (FAILED(hr)){ return false; }
    return true;
}

bool AudioEngine::createDevices(){
    HRESULT hr;
    if (cfg_.render_device_id.empty()){
        hr = enum_->GetDefaultAudioEndpoint(eRender, eConsole, &devRender_);
    } else {
        hr = enum_->GetDevice(cfg_.render_device_id.c_str(), &devRender_);
    }
    if (FAILED(hr)){ return false; }

    if (cfg_.capture_device_id.empty()){
        hr = enum_->GetDefaultAudioEndpoint(eRender, eConsole, &devCapture_);
    } else {
        hr = enum_->GetDevice(cfg_.capture_device_id.c_str(), &devCapture_);
    }
    if (FAILED(hr)){ return false; }
    return true;
}

bool AudioEngine::createClients(){
    HRESULT hr;
    hr = devRender_->Activate(__uuidof(IAudioClient3), CLSCTX_ALL, nullptr, (void**)&acRender_);
    if (FAILED(hr)){ return false; }
    hr = devCapture_->Activate(__uuidof(IAudioClient3), CLSCTX_ALL, nullptr, (void**)&acCapture_);
    if (FAILED(hr)){ return false; }

    WAVEFORMATEX* mixRender=nullptr; WAVEFORMATEX* mixCapture=nullptr;
    acRender_->GetMixFormat(&mixRender);
    acCapture_->GetMixFormat(&mixCapture);

    WAVEFORMATEXTENSIBLE wfx = {};
    wfx.Format.wFormatTag = WAVE_FORMAT_EXTENSIBLE;
    wfx.Format.nChannels = (WORD)cfg_.channels;
    wfx.Format.nSamplesPerSec = cfg_.sample_rate;
    wfx.Format.wBitsPerSample = (WORD)cfg_.bits;
    wfx.Format.nBlockAlign = (wfx.Format.wBitsPerSample/8) * wfx.Format.nChannels;
    wfx.Format.nAvgBytesPerSec = wfx.Format.nBlockAlign * wfx.Format.nSamplesPerSec;
    wfx.Format.cbSize = 22;
    wfx.Samples.wValidBitsPerSample = wfx.Format.wBitsPerSample;
    wfx.dwChannelMask = SPEAKER_FRONT_LEFT | SPEAKER_FRONT_RIGHT;
    wfx.SubFormat = KSDATAFORMAT_SUBTYPE_PCM;
    WAVEFORMATEX* fmt = (WAVEFORMATEX*)&wfx;

    DWORD streamFlags = AUDCLNT_STREAMFLAGS_NOPERSIST;
    if (cfg_.raw) streamFlags |= AUDCLNT_STREAMFLAGS_RAW;

    DWORD capFlags = AUDCLNT_STREAMFLAGS_LOOPBACK | AUDCLNT_STREAMFLAGS_NOPERSIST;
    AUDCLNT_SHAREMODE renShare = cfg_.exclusive ? AUDCLNT_SHAREMODE_EXCLUSIVE : AUDCLNT_SHAREMODE_SHARED;

    UINT32 defaultPeriod=0, fundamental=0, minPeriod=0, maxPeriod=0;
    acRender_->GetSharedModeEnginePeriod(fmt, &defaultPeriod, &fundamental, &minPeriod, &maxPeriod);
    UINT32 targetPeriod = (UINT32)std::max( (int)minPeriod, cfg_.period_us );

    HRESULT hrR = acRender_->Initialize(renShare, streamFlags, 0, targetPeriod, fmt, nullptr);
    if (FAILED(hrR)){
        if (acRender_) { acRender_->Release(); acRender_=nullptr; }
        renShare = AUDCLNT_SHAREMODE_SHARED;
        streamFlags = AUDCLNT_STREAMFLAGS_NOPERSIST;
        hr = devRender_->Activate(__uuidof(IAudioClient3), CLSCTX_ALL, nullptr, (void**)&acRender_);
        if (FAILED(hr)) return false;
        WAVEFORMATEX* fmtR=nullptr; acRender_->GetMixFormat(&fmtR);
        acRender_->Initialize(renShare, streamFlags, 0, 0, fmtR, nullptr);
        CoTaskMemFree(fmtR);
    }

    HRESULT hrC = acCapture_->Initialize(AUDCLNT_SHAREMODE_SHARED, capFlags, 0, 0, mixCapture, nullptr);
    if (FAILED(hrC)){ return false; }

    hr = acRender_->GetService(__uuidof(IAudioRenderClient), (void**)&rc_); if (FAILED(hr)) return false;
    hr = acCapture_->GetService(__uuidof(IAudioCaptureClient), (void**)&cc_); if (FAILED(hr)) return false;

    CoTaskMemFree(mixRender); CoTaskMemFree(mixCapture);
    return true;
}

void AudioEngine::start(){
    if (running_) return;
    if (!devRender_ && !createDevices()) return;
    if (!acRender_ && !createClients()) return;
    acRender_->Start();
    acCapture_->Start();
    running_ = true;
    std::thread(&AudioEngine::renderLoop, this).detach();
}

void AudioEngine::stop(){
    running_ = false;
    if (acRender_) acRender_->Stop();
    if (acCapture_) acCapture_->Stop();
}

void AudioEngine::teardown(){
    if (rc_) { rc_->Release(); rc_=nullptr; }
    if (cc_) { cc_->Release(); cc_=nullptr; }
    if (acRender_) { acRender_->Release(); acRender_=nullptr; }
    if (acCapture_) { acCapture_->Release(); acCapture_=nullptr; }
    if (devRender_) { devRender_->Release(); devRender_=nullptr; }
    if (devCapture_) { devCapture_->Release(); devCapture_=nullptr; }
    if (enum_) { enum_->Release(); enum_=nullptr; }
}

void AudioEngine::renderLoop(){
    HANDLE hTask = AvSetMmThreadCharacteristicsW(L"Pro Audio", nullptr);
    UINT32 pack=0; acRender_->GetBufferSize(&pack);
    BYTE* pData=nullptr; DWORD flags=0; UINT32 frames=0;

    while (running_){
        HRESULT hr = cc_->GetBuffer(&pData, &frames, &flags, nullptr, nullptr);
        if (hr==AUDCLNT_S_BUFFER_EMPTY){ Sleep(0); continue; }
        if (FAILED(hr)){ break; }

        BYTE* pOut=nullptr;
        UINT32 q=0; acRender_->GetCurrentPadding(&q);
        UINT32 avail = pack - q;
        UINT32 toWrite = (frames < avail) ? frames : avail;
        if (toWrite>0){
            hr = rc_->GetBuffer(toWrite, &pOut);
            if (SUCCEEDED(hr)){
                memcpy(pOut, pData, toWrite * 4); // demo: assume 16-bit stereo
                rc_->ReleaseBuffer(toWrite, 0);
            }
        }
        cc_->ReleaseBuffer(frames);
        Sleep(0);
    }

    if (hTask) AvRevertMmThreadCharacteristics(hTask);
}

void AudioEngine::onControlMessage(const std::wstring& json){
    if (json.find(L"START")!=std::wstring::npos) start();
    else if (json.find(L"STOP")!=std::wstring::npos) stop();
    else if (json.find(L"exclusive")!=std::wstring::npos) cfg_.exclusive = (json.find(L"true")!=std::wstring::npos);
    else if (json.find(L"raw")!=std::wstring::npos) cfg_.raw = (json.find(L"true")!=std::wstring::npos);
    else if (json.find(L"period_us")!=std::wstring::npos){
        auto p = json.find(L"period_us");
        if (p!=std::wstring::npos){
            auto q = json.find_first_of(L"0123456789", p);
            if (q!=std::wstring::npos) cfg_.period_us = std::stoi(json.substr(q));
        }
    }
}
