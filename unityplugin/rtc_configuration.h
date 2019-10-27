#pragma once
#include "framework.h"

extern "C" {
	__declspec(dllexport) void* RTCConfiguration_new(
		bool use_media_transport_for_data_channels, 
		bool enable_rtp_data_channel, 
		bool enable_dtls_srtp);
	__declspec(dllexport) void RTCConfiguration_AddServer(void* ptr,
		const char** turn_urls,
		const int no_of_urls,
		const char* username,
		const char* credential);

}