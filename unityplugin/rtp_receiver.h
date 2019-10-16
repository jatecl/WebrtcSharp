#pragma once
#include "framework.h"


class RtpReceiverObserverProxy : public webrtc::RtpReceiverObserverInterface
{
public:
	RtpReceiverObserverProxy() {}
	~RtpReceiverObserverProxy();
	WebrtcUnityStateCallback FirstPacketReceived = nullptr;
	void OnFirstPacketReceived(cricket::MediaType media_type) override;
};


extern "C" {
	__declspec(dllexport) void* RtpReceiver_GetMediaStreamTrack(void* ptr);
	__declspec(dllexport) void RtpReceiver_SetFirstPacketReceivedObserve(void* ptr, WebrtcUnityStateCallback FirstPacketReceived);
}