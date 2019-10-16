#include "rtp_sender.h"

bool RtpSender_SetTrack(void* ptr, void* track)
{
	auto typed = (webrtc::RtpSenderInterface*)(ptr);
	return typed->SetTrack((webrtc::MediaStreamTrackInterface*)(track));
}
