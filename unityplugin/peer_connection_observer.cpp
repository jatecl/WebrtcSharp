#include "peer_connection_observer.h"
#include "pc/webrtc_sdp.h"

PeerConnectionObserverProxy::~PeerConnectionObserverProxy()
{
	SignalingChange = nullptr;
	DataChannel = nullptr;
	RenegotiationNeeded = nullptr;
	IceConnectionChange = nullptr;
	StandardizedIceConnectionChange = nullptr;
	ConnectionChange = nullptr;
	IceGatheringChange = nullptr;
	IceCandidate = nullptr;
	IceCandidatesRemoved = nullptr;
	IceConnectionReceivingChange = nullptr;
	Track = nullptr;
	RemoveTrack = nullptr;
	InterestingUsage = nullptr;
}

void PeerConnectionObserverProxy::OnSignalingChange(webrtc::PeerConnectionInterface::SignalingState new_state)
{
	if (SignalingChange != nullptr) SignalingChange((int)new_state);
}

void PeerConnectionObserverProxy::OnAddStream(rtc::scoped_refptr<MediaStreamInterface> stream)
{
}

void PeerConnectionObserverProxy::OnRemoveStream(rtc::scoped_refptr<MediaStreamInterface> stream)
{
}

void PeerConnectionObserverProxy::OnDataChannel(rtc::scoped_refptr<DataChannelInterface> data_channel)
{
	if (DataChannel != nullptr) DataChannel(data_channel.get());
}

void PeerConnectionObserverProxy::OnRenegotiationNeeded()
{
	if (RenegotiationNeeded != nullptr) RenegotiationNeeded();
}

void PeerConnectionObserverProxy::OnIceConnectionChange(PeerConnectionInterface::IceConnectionState new_state)
{
	if (IceConnectionChange != nullptr) IceConnectionChange((int)new_state);
}

void PeerConnectionObserverProxy::OnStandardizedIceConnectionChange(PeerConnectionInterface::IceConnectionState new_state)
{
	if (StandardizedIceConnectionChange != nullptr) StandardizedIceConnectionChange((int)new_state);
}

void PeerConnectionObserverProxy::OnConnectionChange(PeerConnectionInterface::PeerConnectionState new_state)
{
	if (ConnectionChange != nullptr) ConnectionChange((int)new_state);
}

void PeerConnectionObserverProxy::OnIceGatheringChange(PeerConnectionInterface::IceGatheringState new_state)
{
	if (IceGatheringChange != nullptr) IceGatheringChange((int)new_state);
}

void PeerConnectionObserverProxy::OnIceCandidate(const IceCandidateInterface* candidate)
{
	if (IceCandidate != nullptr) {
		std::string sdp;
		if (!candidate->ToString(&sdp)) {
			RTC_LOG(LS_ERROR) << "Failed to serialize candidate";
			return;
		}

		int sdp_index = candidate->sdp_mline_index();

		auto ptrs = new void* [3] { (void*)sdp.c_str(), &sdp_index, (void*)candidate->sdp_mid().c_str() };

		IceCandidate(ptrs);
		delete[] ptrs;
	}
}

void PeerConnectionObserverProxy::OnIceCandidatesRemoved(const std::vector<cricket::Candidate>& candidates)
{
	if (IceCandidatesRemoved != nullptr) {
		auto ptrs = new void* [candidates.size() * 2 + 1];
		auto index = ptrs;
		for (auto st : candidates) {
			*index = (void*)st.transport_name().c_str();
			++index;
			*index = (void*)SdpSerializeCandidate(st).c_str();
			++index;
		}
		*index = nullptr;
		IceCandidatesRemoved(ptrs);
		delete[] ptrs;
	}
}

void PeerConnectionObserverProxy::OnIceConnectionReceivingChange(bool receiving)
{
	if (IceConnectionReceivingChange != nullptr) IceConnectionReceivingChange(receiving);
}

void PeerConnectionObserverProxy::OnAddTrack(rtc::scoped_refptr<RtpReceiverInterface> receiver, const std::vector<rtc::scoped_refptr<MediaStreamInterface>>& streams)
{
	if (Track) Track(receiver.get());
}

void PeerConnectionObserverProxy::OnTrack(rtc::scoped_refptr<RtpTransceiverInterface> transceiver)
{
	//Î´±»µ÷ÓÃ
}

void PeerConnectionObserverProxy::OnRemoveTrack(rtc::scoped_refptr<RtpReceiverInterface> receiver)
{
	if (RemoveTrack != nullptr) RemoveTrack(receiver.get());
}

void PeerConnectionObserverProxy::OnInterestingUsage(int usage_pattern)
{
	if (InterestingUsage != nullptr) InterestingUsage(usage_pattern);
}

void* PeerConnectionObserver_new(WebrtcUnityStateCallback SignalingChange,
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
	WebrtcUnityStateCallback InterestingUsage)
{
	auto result = new PeerConnectionObserverProxy();
	result->SignalingChange = SignalingChange;
	result->DataChannel = DataChannel;
	result->RenegotiationNeeded = RenegotiationNeeded;
	result->IceConnectionChange = IceConnectionChange;
	result->StandardizedIceConnectionChange = StandardizedIceConnectionChange;
	result->ConnectionChange = ConnectionChange;
	result->IceGatheringChange = IceGatheringChange;
	result->IceCandidate = IceCandidate;
	result->IceCandidatesRemoved = IceCandidatesRemoved;
	result->IceConnectionReceivingChange = IceConnectionReceivingChange;
	result->Track = AddTrack;
	result->RemoveTrack = RemoveTrack;
	result->InterestingUsage = InterestingUsage;
	result->AddRef();
	return result;
}