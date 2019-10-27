#pragma once
#include "framework.h"


class RTCDataChannelObserver : public rtc::RefCountedObject<webrtc::DataChannelObserver> {
public:
	RTCDataChannelObserver() {}
	// The data channel state have changed.
	virtual void OnStateChange() override;
	WebrtcUnityCallback StateChange;
	//  A data buffer was successfully received.
	virtual void OnMessage(const webrtc::DataBuffer& buffer) override;
	WebrtcUnityResultCallback Message;
	// The data channel's buffered_amount has changed.
	virtual void OnBufferedAmountChange(uint64_t sent_data_size) override;
	WebrtcUnityResultCallback BufferedAmountChange;
	virtual ~RTCDataChannelObserver();
};

extern "C" {
	__declspec(dllexport) void RTCDataChannel_Close(void* ptr);
	__declspec(dllexport) void RTCDataChannel_Send(void* ptr, bool binnary, void* buffer, int length);
	__declspec(dllexport) void* RTCDataChannel_Label(void* ptr);
	__declspec(dllexport) int RTCDataChannel_State(void* ptr);
	__declspec(dllexport) void* RTCDataChannel_RegisterObserver(void* ptr, WebrtcUnityCallback stateChange, WebrtcUnityResultCallback message, WebrtcUnityResultCallback bufferedAmountChange);
	__declspec(dllexport) void RTCDataChannel_UnregisterObserver(void* ptr);
}