#pragma once

#include "framework.h"

class CreateSessionDescriptionObserverCallback : 
	public rtc::RefCountedObject<webrtc::CreateSessionDescriptionObserver>{
public:
	CreateSessionDescriptionObserverCallback() {}
	virtual ~CreateSessionDescriptionObserverCallback();
	WebrtcUnityResult2Callback Success = nullptr;
	WebrtcUnityResultCallback Failure = nullptr;
	void OnSuccess(webrtc::SessionDescriptionInterface* desc) override;
	void OnFailure(webrtc::RTCError error) override;
	void OnFailure(const std::string& error) override;
};



class SetSessionDescriptionObserverCallback :
	public rtc::RefCountedObject<webrtc::SetSessionDescriptionObserver> {
public:
	SetSessionDescriptionObserverCallback() {}
	virtual ~SetSessionDescriptionObserverCallback();
	WebrtcUnityCallback Success = nullptr;
	WebrtcUnityResultCallback Failure = nullptr;
	void OnSuccess() override;
	void OnFailure(webrtc::RTCError error) override;
	void OnFailure(const std::string& error) override;
};

extern "C" {
	__declspec(dllexport) void* PeerConnection_CreateDataChannel(void* ptr,
		const char* label, 
		bool reliable, 
		bool ordered,
		int maxRetransmitTime, 
		int maxRetransmits, 
		const char* protocol, 
		bool negotiated, 
		int id);
	__declspec(dllexport) void PeerConnection_Close(void* ptr);
	__declspec(dllexport) void PeerConnection_CreateOffer(void* ptr,
		WebrtcUnityResult2Callback success,
		WebrtcUnityResultCallback failure,
		bool offer_to_receive_video,
		bool offer_to_receive_audio,
		bool voice_activity_detection,
		bool ice_restart,
		bool use_rtp_mux);
	__declspec(dllexport) void PeerConnection_CreateAnswer(void* ptr,
		WebrtcUnityResult2Callback success,
		WebrtcUnityResultCallback failure,
		bool offer_to_receive_video,
		bool offer_to_receive_audio,
		bool voice_activity_detection,
		bool ice_restart,
		bool use_rtp_mux);
	__declspec(dllexport) void PeerConnection_SetRemoteDescription(void* ptr,
		const char* type,
		const char* sdp,
		WebrtcUnityCallback success,
		WebrtcUnityResultCallback failure);
	__declspec(dllexport) void PeerConnection_SetLocalDescription(void* ptr,
		const char* type,
		const char* sdp,
		WebrtcUnityCallback success,
		WebrtcUnityResultCallback failure);
	__declspec(dllexport) bool PeerConnection_AddIceCandidate(void* ptr,
		const char* candidate,
		const int sdp_mlineindex,
		const char* sdp_mid);
	__declspec(dllexport) void* PeerConnection_AddTrack(void* ptr,
		void* track,
		const char** labels,
		int len);
	__declspec(dllexport) void* PeerConnection_RemoveTrack(void* ptr,
		void* sender);
	__declspec(dllexport) void* PeerConnection_GetSenders(void* ptr);
}