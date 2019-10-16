#include "framework.h"


void WebrtcObject_delete(void* ptr)
{
	if (!ptr) return;
	auto obj = (rtc::RefCountInterface*)(ptr);
	obj->Release();
}

void* StringBuffer_GetBuffer(void* ptr)
{
	auto typed = (StringBuffer*)(ptr);
	return typed->pointer;
}

void* PeerConnectionFactory_GetDeviceInfo() {
	auto info = webrtc::VideoCaptureFactory::CreateDeviceInfo();
	auto count = info->NumberOfDevices();
	auto buffer = new StringBuffer(count * 4);
	auto ptrs = buffer->pointer;
	for (int i = 0; i < count; ++i) {
		int len = 128;
		auto name = new char[len];
		name[0] = 0;
		auto id = new char[len];
		id[0] = 0;
		auto pid = new char[len];
		pid[0] = 0;
		auto en = new char[4];
		*(int32_t*)en = info->GetDeviceName(i, name, len, id, len, pid, len);
		*ptrs = name;
		++ptrs;
		*ptrs = id;
		++ptrs;
		*ptrs = pid;
		++ptrs;
		*ptrs = en;
		++ptrs;

	}
	delete info;
	buffer->AddRef();
	return buffer;
}

void* PeerConnectionFactory_GetDeviceCapabilities(int index) {
	auto info = webrtc::VideoCaptureFactory::CreateDeviceInfo();
	auto count = info->NumberOfDevices();
	if (index < 0) index = 0;
	else if (index >= count) index = 0;
	int len = 128;
	auto name = new char[len];
	auto id = new char[len];
	info->GetDeviceName(index, name, len, id, len);

	auto size = info->NumberOfCapabilities(id);
	auto buffer = new StringBuffer(size);
	auto ptrs = buffer->pointer;
	webrtc::VideoCaptureCapability capability;
	for (int i = 0; i < size; ++i) {
		info->GetCapability(id, i, capability);
		auto ca = new char[5 * sizeof(int32_t)];
		auto cap = (int32_t*)ca;
		*cap = capability.width;
		++cap;
		*cap = capability.height;
		++cap;
		*cap = capability.maxFPS;
		++cap;
		*cap = (int32_t)capability.videoType;
		++cap;
		*cap = capability.interlaced;
		++cap;
		*ptrs = ca;
		++ptrs;
	}

	delete[] name;
	delete[] id;
	delete info;

	buffer->AddRef();
	return buffer;
}

StringBuffer::StringBuffer(int length)
{
	pointer = new char* [length + 1];
	pointer[length] = nullptr;
}

StringBuffer::~StringBuffer()
{
	char** it = pointer;
	while (*it) {
		delete[](*it);
		++it;
	}
	delete[] pointer;
}
