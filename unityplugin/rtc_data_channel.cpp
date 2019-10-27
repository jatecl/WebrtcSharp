#include "rtc_data_channel.h"

void RTCDataChannel_Close(void* ptr)
{
	auto typed = (webrtc::DataChannelInterface*)ptr;
	typed->Close();
}

void RTCDataChannel_Send(void* ptr, bool binnary, void* buffer, int length)
{
	auto typed = (webrtc::DataChannelInterface*)ptr;
	rtc::CopyOnWriteBuffer copy((char*)buffer, length);
	webrtc::DataBuffer data(copy, binnary);
	typed->Send(data);
}

void* RTCDataChannel_Label(void* ptr)
{
	auto typed = (webrtc::DataChannelInterface*)ptr;
	auto label = typed->label();
	auto obj = new BytesBuffer(1);
	*obj->pointer = new char[label.length() + 1];
	strcpy((char*)*obj->pointer, label.c_str());
	obj->AddRef();
	return obj;
}

int RTCDataChannel_State(void* ptr)
{
	auto typed = (webrtc::DataChannelInterface*)ptr;
	return typed->state();
}

void* RTCDataChannel_RegisterObserver(void* ptr, WebrtcUnityCallback stateChange, WebrtcUnityResultCallback message, WebrtcUnityResultCallback bufferedAmountChange)
{
	auto typed = (webrtc::DataChannelInterface*)ptr;
	RTCDataChannelObserver* observer = new RTCDataChannelObserver();
	observer->Message = message;
	observer->BufferedAmountChange = bufferedAmountChange;
	observer->StateChange = stateChange;
	typed->RegisterObserver(observer);
	observer->AddRef();
	return observer;
}

void RTCDataChannel_UnregisterObserver(void* ptr)
{
	auto typed = (webrtc::DataChannelInterface*)ptr;
	typed->UnregisterObserver();
}

void RTCDataChannelObserver::OnStateChange()
{
	if (StateChange != nullptr) StateChange();
}

void RTCDataChannelObserver::OnMessage(const webrtc::DataBuffer& buffer)
{
	if (Message != nullptr) {
		int32_t length = buffer.data.size();
		int32_t binnary = buffer.binary;
		auto ret = new PointerArray(3);
		ret->pointer[0] = &length;
		ret->pointer[1] = (void*)buffer.data.data<char>();
		ret->pointer[2] = &binnary;
		ret->AddRef(); 
		Message(ret);
	}
}

void RTCDataChannelObserver::OnBufferedAmountChange(uint64_t sent_data_size)
{
	if (BufferedAmountChange != nullptr) BufferedAmountChange(&sent_data_size);
}

RTCDataChannelObserver::~RTCDataChannelObserver()
{
	StateChange = nullptr;
	Message = nullptr;
	BufferedAmountChange = nullptr;
}
