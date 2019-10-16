#pragma once
#include "framework.h"
using namespace webrtc;

class PeerConnectionObserverProxy : public rtc::RefCountedObject<webrtc::PeerConnectionObserver>
{
public:
	~PeerConnectionObserverProxy();
	// Triggered when the SignalingState changed.
	void OnSignalingChange(
		PeerConnectionInterface::SignalingState new_state) override;

	WebrtcUnityStateCallback SignalingChange = nullptr;

	// Triggered when media is received on a new stream from remote peer.
	void OnAddStream(rtc::scoped_refptr<MediaStreamInterface> stream) override;

	// Triggered when a remote peer closes a stream.
	void OnRemoveStream(rtc::scoped_refptr<MediaStreamInterface> stream) override;

	// Triggered when a remote peer opens a data channel.
	void OnDataChannel(
		rtc::scoped_refptr<DataChannelInterface> data_channel) override;

	WebrtcUnityResultCallback DataChannel = nullptr;
	// Triggered when renegotiation is needed. For example, an ICE restart
	// has begun.
	void OnRenegotiationNeeded() override;
	WebrtcUnityCallback RenegotiationNeeded;
	// Called any time the legacy IceConnectionState changes.
	//
	// Note that our ICE states lag behind the standard slightly. The most
	// notable differences include the fact that "failed" occurs after 15
	// seconds, not 30, and this actually represents a combination ICE + DTLS
	// state, so it may be "failed" if DTLS fails while ICE succeeds.
	//
	// TODO(jonasolsson): deprecate and remove this.
	void OnIceConnectionChange(
		PeerConnectionInterface::IceConnectionState new_state) override;
	WebrtcUnityStateCallback IceConnectionChange = nullptr;

	// Called any time the standards-compliant IceConnectionState changes.
	void OnStandardizedIceConnectionChange(
		PeerConnectionInterface::IceConnectionState new_state) override;
	WebrtcUnityStateCallback StandardizedIceConnectionChange = nullptr;

	// Called any time the PeerConnectionState changes.
	void OnConnectionChange(
		PeerConnectionInterface::PeerConnectionState new_state) override;
	WebrtcUnityStateCallback ConnectionChange = nullptr;

	// Called any time the IceGatheringState changes.
	void OnIceGatheringChange(
		PeerConnectionInterface::IceGatheringState new_state) override;
	WebrtcUnityStateCallback IceGatheringChange = nullptr;

	// A new ICE candidate has been gathered.
	void OnIceCandidate(const IceCandidateInterface* candidate) override;
	WebrtcUnityResultCallback IceCandidate = nullptr;

	// Ice candidates have been removed.
	// TODO(honghaiz): Make this a pure method when all its subclasses
	// implement it.
	void OnIceCandidatesRemoved(
		const std::vector<cricket::Candidate>& candidates) override;
	WebrtcUnityResultCallback IceCandidatesRemoved = nullptr;

	// Called when the ICE connection receiving status changes.
	void OnIceConnectionReceivingChange(bool receiving) override;
	WebrtcUnityStateCallback IceConnectionReceivingChange = nullptr;

	// This is called when a receiver and its track are created.
	// TODO(zhihuang): Make this pure when all subclasses implement it.
	// Note: This is called with both Plan B and Unified Plan semantics. Unified
	// Plan users should prefer OnTrack, OnAddTrack is only called as backwards
	// compatibility (and is called in the exact same situations as OnTrack).
	void OnAddTrack(
		rtc::scoped_refptr<RtpReceiverInterface> receiver,
		const std::vector<rtc::scoped_refptr<MediaStreamInterface>>& streams) override;

	// This is called when signaling indicates a transceiver will be receiving
	// media from the remote endpoint. This is fired during a call to
	// SetRemoteDescription. The receiving track can be accessed by:
	// |transceiver->receiver()->track()| and its associated streams by
	// |transceiver->receiver()->streams()|.
	// Note: This will only be called if Unified Plan semantics are specified.
	// This behavior is specified in section 2.2.8.2.5 of the "Set the
	// RTCSessionDescription" algorithm:
	// https://w3c.github.io/webrtc-pc/#set-description
	void OnTrack(
		rtc::scoped_refptr<RtpTransceiverInterface> transceiver) override;
	WebrtcUnityResultCallback Track = nullptr;

	// Called when signaling indicates that media will no longer be received on a
	// track.
	// With Plan B semantics, the given receiver will have been removed from the
	// PeerConnection and the track muted.
	// With Unified Plan semantics, the receiver will remain but the transceiver
	// will have changed direction to either sendonly or inactive.
	// https://w3c.github.io/webrtc-pc/#process-remote-track-removal
	// TODO(hbos,deadbeef): Make pure when all subclasses implement it.
	void OnRemoveTrack(
		rtc::scoped_refptr<RtpReceiverInterface> receiver) override;
	WebrtcUnityResultCallback RemoveTrack = nullptr;

	// Called when an interesting usage is detected by WebRTC.
	// An appropriate action is to add information about the context of the
	// PeerConnection and write the event to some kind of "interesting events"
	// log function.
	// The heuristics for defining what constitutes "interesting" are
	// implementation-defined.
	void OnInterestingUsage(int usage_pattern) override;
	WebrtcUnityStateCallback InterestingUsage = nullptr;
};


extern "C" {
	__declspec(dllexport) void* PeerConnectionObserver_new(WebrtcUnityStateCallback SignalingChange,
		WebrtcUnityResultCallback DataChannel,
		WebrtcUnityCallback RenegotiationNeeded,
		WebrtcUnityStateCallback IceConnectionChange,
		WebrtcUnityStateCallback StandardizedIceConnectionChange,
		WebrtcUnityStateCallback ConnectionChange,
		WebrtcUnityStateCallback IceGatheringChange,
		WebrtcUnityResultCallback IceCandidate,
		WebrtcUnityResultCallback IceCandidatesRemoved,
		WebrtcUnityStateCallback IceConnectionReceivingChange,
		WebrtcUnityResultCallback AddTrack,
		WebrtcUnityResultCallback RemoveTrack,
		WebrtcUnityStateCallback InterestingUsage);

}