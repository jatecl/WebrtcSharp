#include "rtp_receiver.h"

RtpReceiverObserverProxy::~RtpReceiverObserverProxy()
{
	FirstPacketReceived = nullptr;
}

void RtpReceiverObserverProxy::OnFirstPacketReceived(cricket::MediaType media_type)
{
	if (FirstPacketReceived) FirstPacketReceived(media_type);
}

void* RtpReceiver_GetMediaStreamTrack(void* ptr)
{
	auto typed = (webrtc::RtpReceiverInterface*)(ptr);
	auto track = typed->track();
	track->AddRef();
	return track;
}

void RtpReceiver_SetFirstPacketReceivedObserve(void* ptr, WebrtcUnityStateCallback FirstPacketReceived)
{
	auto typed = (webrtc::RtpReceiverInterface*)(ptr);
	if (FirstPacketReceived == nullptr) typed->SetObserver(nullptr);
	else {
		auto observe = new RtpReceiverObserverProxy();
		observe->FirstPacketReceived = FirstPacketReceived;
		typed->SetObserver(observe);
	}
}
