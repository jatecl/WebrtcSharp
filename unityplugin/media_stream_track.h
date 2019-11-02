#pragma once
#include "framework.h"
#include <list>
#include <mutex>

class VideoObserver : public rtc::VideoSinkInterface<webrtc::VideoFrame> {
public:
	VideoObserver() {}
	virtual ~VideoObserver();
	WebrtcUnityResultCallback OnI420FrameReady = nullptr;

protected:
	// VideoSinkInterface implementation
	void OnFrame(const webrtc::VideoFrame& frame) override;

private:
	std::mutex mutex;
};

class AudioObserver : public webrtc::AudioTrackSinkInterface {
public:
	AudioObserver() {}
	virtual ~AudioObserver();
	WebrtcUnityResultCallback OnDataReady = nullptr;

protected:
	// AudioTrackSinkInterface implementation.
	void OnData(const void* audio_data,
		int bits_per_sample,
		int sample_rate,
		size_t number_of_channels,
		size_t number_of_frames) override;

private:
	std::mutex mutex;
};

class FrameVideoSource
	: public rtc::RefCountedObject<webrtc::Notifier<webrtc::VideoTrackSourceInterface> > {
public:
	FrameVideoSource() {}
	~FrameVideoSource() {}
	SourceState state() const override { return kLive; }
	bool remote() const override { return true; }
	virtual void AddOrUpdateSink(
		rtc::VideoSinkInterface<webrtc::VideoFrame>* sink,
		const rtc::VideoSinkWants& wants) override;
	virtual void RemoveSink(
		rtc::VideoSinkInterface<webrtc::VideoFrame>* sink) override;
	virtual bool is_screencast() const override { return false; }
	virtual absl::optional<bool> needs_denoising() const override {
		return absl::optional<bool>();
	}
	virtual bool GetStats(Stats* stats) override { return false; }

public:
	void NewFrame(const uint8_t* data_y,
		const uint8_t* data_u,
		const uint8_t* data_v,
		const uint8_t* data_a,
		int stride_y,
		int stride_u,
		int stride_v,
		int stride_a,
		uint32_t width,
		uint32_t height,
		int rotation,
		int64_t timestamp);

private:
	std::list<rtc::VideoSinkInterface<webrtc::VideoFrame>*> sinks;
	std::mutex mutex;
};

class FrameAudioSource : public rtc::RefCountedObject<webrtc::Notifier<webrtc::AudioSourceInterface> > {
public:
	FrameAudioSource() {}
	~FrameAudioSource() {}
	SourceState state() const override { return kLive; }
	bool remote() const override { return true; }

	void AddSink(webrtc::AudioTrackSinkInterface* sink) override;
	void RemoveSink(webrtc::AudioTrackSinkInterface* sink) override;

	void OnData(const void* audio_data,
		int bits_per_sample,
		int sample_rate,
		size_t number_of_channels,
		size_t number_of_frames);

private:
	std::list<webrtc::AudioTrackSinkInterface*> sinks;
	rtc::CriticalSection sink_lock_;
};


extern "C" {

	__declspec(dllexport) void* MediaStreamTrack_GetKind(void* ptr);
	__declspec(dllexport) bool MediaStreamTrack_GetEnabled(void* ptr);
	__declspec(dllexport) int MediaStreamTrack_GetState(void* ptr);
	__declspec(dllexport) void MediaStreamTrack_SetEnabled(void* ptr, bool enabled);


	__declspec(dllexport) void* VideoSource_AddSink(void* ptr, WebrtcUnityResultCallback onI420FrameReady);
	__declspec(dllexport) void VideoSource_RemoveSink(void* ptr, void* sink);

	__declspec(dllexport) void* AudioSource_AddSink(void* ptr, WebrtcUnityResultCallback onDataReady);
	__declspec(dllexport) void AudioSource_RemoveSink(void* ptr, void* sink);

	__declspec(dllexport) void* VideoTrack_AddSink(void* ptr, WebrtcUnityResultCallback onI420FrameReady);
	__declspec(dllexport) void VideoTrack_RemoveSink(void* ptr, void* sink);

	__declspec(dllexport) void* AudioTrack_AddSink(void* ptr, WebrtcUnityResultCallback onDataReady);
	__declspec(dllexport) void AudioTrack_RemoveSink(void* ptr, void* sink);

	__declspec(dllexport) void* FrameVideoSource_new();
	__declspec(dllexport) void FrameVideoSource_SendFrame(void* ptr,
		uint8_t* data_y,
		uint8_t* data_u,
		uint8_t* data_v,
		uint8_t* data_a,
		int stride_y,
		int stride_u,
		int stride_v,
		int stride_a,
		uint32_t width,
		uint32_t height,
		int rotation,
		int64_t timestamp);

	__declspec(dllexport) void* FrameAudioSource_new();
	__declspec(dllexport) void FrameAudioSource_SendFrame(void* ptr,
		void* audio_data,
		int bits_per_sample,
		int sample_rate,
		size_t number_of_channels,
		size_t number_of_frames);
}