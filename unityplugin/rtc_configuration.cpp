#include "rtc_configuration.h"

void* RTCConfiguration_new(
	bool use_media_transport_for_data_channels,
	bool enable_rtp_data_channel, 
	bool enable_dtls_srtp)
{
	auto typed = new StructPointer<webrtc::PeerConnectionInterface::RTCConfiguration>();
	typed->data.use_media_transport_for_data_channels = use_media_transport_for_data_channels;
	typed->data.use_media_transport = use_media_transport_for_data_channels;
	typed->data.enable_rtp_data_channel = enable_rtp_data_channel;
	typed->data.enable_dtls_srtp = enable_dtls_srtp;
	return typed;
}

void RTCConfiguration_AddServer(void* ptr, const char** turn_urls, const int no_of_urls, const char* username, const char* credential)
{
	auto typed = (StructPointer<webrtc::PeerConnectionInterface::RTCConfiguration>*)(ptr);

	// Add the turn server.
	if (turn_urls != nullptr) {
		if (no_of_urls > 0) {
			webrtc::PeerConnectionInterface::IceServer turn_server;
			if (no_of_urls == 1) {
				std::string url(turn_urls[0]);
				if (url.length() > 0)
					turn_server.uri = url;
			}
			else {
				for (int i = 0; i < no_of_urls; i++) {
					std::string url(turn_urls[i]);
					if (url.length() > 0)
						turn_server.urls.push_back(url);
				}
			}
			if (username) {
				std::string user_name(username);
				if (user_name.length() > 0)
					turn_server.username = username;
			}
			if (credential) {
				std::string password(credential);
				if (password.length() > 0)
					turn_server.password = credential;
			}

			typed->data.servers.push_back(turn_server);
		}
	}
}
