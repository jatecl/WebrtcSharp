
#include "peer_connection.h"

void PeerConnection_Close(void* ptr)
{
	auto typed = (webrtc::PeerConnectionInterface*)(ptr);
	typed->Close();
}

void PeerConnection_CreateOffer(void* ptr,
	WebrtcUnityResult2Callback success,
	WebrtcUnityResultCallback failure,
	bool offer_to_receive_video,
	bool offer_to_receive_audio,
	bool voice_activity_detection,
	bool ice_restart,
	bool use_rtp_mux)
{
	auto typed = (webrtc::PeerConnectionInterface*)(ptr);

	webrtc::PeerConnectionInterface::RTCOfferAnswerOptions options(
		offer_to_receive_video,
		offer_to_receive_audio,
		voice_activity_detection,
		ice_restart,
		use_rtp_mux
	);
	
	auto callback = new CreateSessionDescriptionObserverCallback();
	callback->Success = success;
	callback->Failure = failure;
	typed->CreateOffer(callback, options);
}

void PeerConnection_CreateAnswer(void* ptr,
	WebrtcUnityResult2Callback success,
	WebrtcUnityResultCallback failure,
	bool offer_to_receive_video,
	bool offer_to_receive_audio,
	bool voice_activity_detection,
	bool ice_restart,
	bool use_rtp_mux)
{
	auto typed = (webrtc::PeerConnectionInterface*)(ptr);

	webrtc::PeerConnectionInterface::RTCOfferAnswerOptions options(
		offer_to_receive_video,
		offer_to_receive_audio,
		voice_activity_detection,
		ice_restart,
		use_rtp_mux
	);
	auto callback = new CreateSessionDescriptionObserverCallback();
	callback->Success = success;
	callback->Failure = failure;
	typed->CreateAnswer(callback, options);
}

void PeerConnection_SetRemoteDescription(void* ptr, 
	const char* type, 
	const char* sdp,
	WebrtcUnityCallback success,
	WebrtcUnityResultCallback failure)
{
	auto typed = (webrtc::PeerConnectionInterface*)(ptr);

	std::string remote_desc(sdp);
	std::string sdp_type(type);
	webrtc::SdpParseError error;
	webrtc::SessionDescriptionInterface* session_description(
		webrtc::CreateSessionDescription(sdp_type, remote_desc, &error));

	auto callback = new SetSessionDescriptionObserverCallback();
	callback->Success = success;
	callback->Failure = failure;
	typed->SetRemoteDescription(callback, session_description);
}

void PeerConnection_SetLocalDescription(void* ptr, 
	const char* type, 
	const char* sdp,
	WebrtcUnityCallback success,
	WebrtcUnityResultCallback failure)
{
	auto typed = (webrtc::PeerConnectionInterface*)(ptr);

	std::string remote_desc(sdp);
	std::string sdp_type(type);
	webrtc::SdpParseError error;
	webrtc::SessionDescriptionInterface* session_description(
		webrtc::CreateSessionDescription(sdp_type, remote_desc, &error));

	auto callback = new SetSessionDescriptionObserverCallback();
	callback->Success = success;
	callback->Failure = failure;
	typed->SetLocalDescription(callback, session_description);
}

bool PeerConnection_AddIceCandidate(void* ptr, const char* candidate, const int sdp_mlineindex, const char* sdp_mid)
{
	auto typed = (webrtc::PeerConnectionInterface*)(ptr);

	webrtc::SdpParseError error;
	std::unique_ptr<webrtc::IceCandidateInterface> ice_candidate(
		webrtc::CreateIceCandidate(sdp_mid, sdp_mlineindex, candidate, &error));
	if (!ice_candidate.get()) {
		return false;
	}
	if (!typed->AddIceCandidate(ice_candidate.get())) {
		return false;
	}
	return true;
}

void* PeerConnection_AddTrack(void* ptr, void* track, const char** labels, int len)
{
	auto typed = (webrtc::PeerConnectionInterface*)(ptr);
	auto trackPtr = (webrtc::MediaStreamTrackInterface*)(track);
	std::vector<std::string> stream_ids;
	for (int i = 0; i < len; ++i) {
		stream_ids.push_back(*(labels + i));
	}
	auto result = typed->AddTrack(trackPtr, stream_ids);
	if (!result.ok()) return nullptr;
	
	auto pointer = result.value();
	pointer->AddRef();
	return pointer;
}

void* PeerConnection_RemoveTrack(void* ptr, void* sender)
{
	auto typed = (webrtc::PeerConnectionInterface*)(ptr);
	auto senderPtr = (webrtc::RtpSenderInterface*)(sender);
	auto error = typed->RemoveTrackNew(senderPtr);
	if (error.ok()) return nullptr;
	return (void*)error.message();
}

void* PeerConnection_GetSenders(void* ptr)
{
	auto typed = (webrtc::PeerConnectionInterface*)(ptr);
	auto list = typed->GetSenders();
	auto ret = new PointerArray(list.size());
	for (auto i = 0; i < list.size(); ++i) {
		ret->pointer[i] = list.at(i).get();
	}
	ret->AddRef();
	return ret;
}

void* PeerConnection_CreateDataChannel(void* ptr, const char* label, bool reliable, bool ordered,
	int maxRetransmitTime, int maxRetransmits, const char* protocol, bool negotiated, int id)
{
	auto typed = (webrtc::PeerConnectionInterface*)(ptr);
	struct webrtc::DataChannelInit init;
	init.ordered = ordered;
	init.reliable = reliable;
	if (maxRetransmitTime >= 0) init.maxRetransmitTime = maxRetransmitTime;
	if (maxRetransmits >= 0)init.maxRetransmits = maxRetransmits;
	if (protocol != nullptr) init.protocol = std::string(protocol);
	init.negotiated = negotiated;
	if (id >= 0) init.id = id;

	auto data_channel_ = typed->CreateDataChannel(label, &init);
	if (!data_channel_.get()) {
		return nullptr;
	}
	data_channel_->AddRef();
	return data_channel_.get();
}

CreateSessionDescriptionObserverCallback::~CreateSessionDescriptionObserverCallback()
{
	Success = nullptr;
	Failure = nullptr;
}

void CreateSessionDescriptionObserverCallback::OnSuccess(webrtc::SessionDescriptionInterface* desc)
{
	std::string sdp;
	desc->ToString(&sdp);

	if (Success) Success((void*)desc->type().c_str(), (void*)sdp.c_str());
}

void CreateSessionDescriptionObserverCallback::OnFailure(webrtc::RTCError error)
{
	if (Failure) Failure((void*)error.message());
}

void CreateSessionDescriptionObserverCallback::OnFailure(const std::string& error)
{
	if (Failure) Failure((void*)error.c_str());
}

SetSessionDescriptionObserverCallback::~SetSessionDescriptionObserverCallback()
{
	Success = nullptr;
	Failure = nullptr;
}

void SetSessionDescriptionObserverCallback::OnSuccess()
{
	if (Success) Success();
}

void SetSessionDescriptionObserverCallback::OnFailure(webrtc::RTCError error)
{
	if (Failure) Failure((void*)error.message());
}

void SetSessionDescriptionObserverCallback::OnFailure(const std::string& error)
{
	if (Failure) Failure((void*)error.c_str());
}
