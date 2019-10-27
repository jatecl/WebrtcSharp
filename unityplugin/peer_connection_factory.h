#pragma once

#include "framework.h"


extern "C" {
	__declspec(dllexport) void* PeerConnectionFactory_new();
	__declspec(dllexport) void* PeerConnectionFactory_CreatePeerConnection(void* ptr, void* config, void* observe);
	__declspec(dllexport) void* PeerConnectionFactory_CreateVideoTrack(void* ptr, const char* label, void* source);
	__declspec(dllexport) void* PeerConnectionFactory_CreateAudioTrack(void* ptr, const char* label, void* source);
	__declspec(dllexport) void* PeerConnectionFactory_CreateVideoSource(void* ptr, int index, int width, int height, int fps);
	__declspec(dllexport) void* PeerConnectionFactory_CreateAudioSource(void* ptr);
}

class PeerConnectionFactoryPointer : public rtc::RefCountedObject<rtc::RefCountInterface> {
public:
	PeerConnectionFactoryPointer() {}
	~PeerConnectionFactoryPointer();
	std::unique_ptr<rtc::Thread> network_thread;
	std::unique_ptr<rtc::Thread> worker_thread;
	std::unique_ptr<rtc::Thread> signaling_thread;
	rtc::scoped_refptr<webrtc::PeerConnectionFactoryInterface> factory;
};