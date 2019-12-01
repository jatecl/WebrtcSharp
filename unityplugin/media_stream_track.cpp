#include "media_stream_track.h"
#include "api/video/i420_buffer.h"

VideoObserver::~VideoObserver()
{
	std::unique_lock<std::mutex> lock(mutex);
	OnI420FrameReady = nullptr;
}

void VideoObserver::OnFrame(const webrtc::VideoFrame& frame)
{
	std::unique_lock<std::mutex> lock(mutex);
	if (!OnI420FrameReady)
		return;

	void* ptr = frame.video_frame_buffer().get();

	if (sizeof(ptr) == 8 && ptr == (void*)0xffffffffffffffff) return;
	if (sizeof(ptr) == 4 && ptr == (void*)0xffffffff) return;

	rtc::scoped_refptr<webrtc::VideoFrameBuffer> buffer(
		frame.video_frame_buffer());

	int32_t sy = 0, su = 0, sv = 0, sa = 0, w = (int32_t)frame.width(), h = (int32_t)frame.height(), r = frame.rotation();
	int64_t ts = frame.timestamp_us();
	void* ptrs[12] = { nullptr, nullptr, nullptr, nullptr, &sy, &su, &sv, &sa, &w, &h, &r, &ts };
	

	if (buffer->type() != webrtc::VideoFrameBuffer::Type::kI420A) {
		rtc::scoped_refptr<webrtc::I420BufferInterface> i420_buffer =
			buffer->ToI420();
		ptrs[0] = (void*)i420_buffer->DataY();
		ptrs[1] = (void*)i420_buffer->DataU();
		ptrs[2] = (void*)i420_buffer->DataV();
		sy = i420_buffer->StrideY();
		su = i420_buffer->StrideU();
		sv = i420_buffer->StrideV();

	}
	else {
		// The buffer has alpha channel.
		const webrtc::I420ABufferInterface* i420a_buffer = buffer->GetI420A();

		ptrs[0] = (void*)i420a_buffer->DataY();
		ptrs[1] = (void*)i420a_buffer->DataU();
		ptrs[2] = (void*)i420a_buffer->DataV();
		ptrs[3] = (void*)i420a_buffer->DataV();
		sy = i420a_buffer->StrideY();
		su = i420a_buffer->StrideU();
		sv = i420a_buffer->StrideV();
		sa = i420a_buffer->StrideA();
	}

	OnI420FrameReady(&ptrs);
}

void AudioObserver::OnData(const void* audio_data, int bits_per_sample, int sample_rate, size_t number_of_channels, size_t number_of_frames)
{
	std::unique_lock<std::mutex> lock(mutex);
	if (!OnDataReady)
		return;

	int32_t bps = bits_per_sample;
	int32_t sr = sample_rate;
	int32_t noc = number_of_channels;
	int32_t nof = number_of_frames;

	void* ptrs[5] = { (void*)audio_data, &bps, &sr, &noc, &nof };
	OnDataReady(&ptrs);
}

AudioObserver::~AudioObserver() {
	std::unique_lock<std::mutex> lock(mutex);
	OnDataReady = nullptr;
}

void FrameVideoSource::AddOrUpdateSink(
	rtc::VideoSinkInterface<webrtc::VideoFrame>* sink,
	const rtc::VideoSinkWants& wants) {
	std::unique_lock<std::mutex> lock(mutex);
	for (rtc::VideoSinkInterface<webrtc::VideoFrame>* s : sinks) {
		if (s == sink)
			return;
	}
	sinks.push_back(sink);
}

void FrameVideoSource::RemoveSink(
	rtc::VideoSinkInterface<webrtc::VideoFrame>* sink) {
	std::unique_lock<std::mutex> lock(mutex);
	sinks.remove(sink);
}

void FrameVideoSource::NewFrame(const uint8_t* data_y,
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
	int64_t timestamp) {
	std::unique_lock<std::mutex> lock(mutex);
	webrtc::VideoFrame::Builder buider;
	if (data_a == nullptr && stride_a == 0) {
		rtc::scoped_refptr<webrtc::I420BufferInterface> buffer =
			webrtc::I420Buffer::Copy(width, height, data_y, stride_y, data_u,
				stride_u, data_v, stride_v);
		buider.set_video_frame_buffer(buffer);
	}
	else {
		rtc::scoped_refptr<webrtc::I420BufferInterface> buffer =
			webrtc::I420Buffer::Copy(width, height, data_y, stride_y, data_u,
				stride_u, data_v, stride_v);
		buider.set_video_frame_buffer(buffer);
	}
	if (timestamp >= 0) buider.set_timestamp_us(timestamp);
	buider.set_rotation((webrtc::VideoRotation)rotation);
	webrtc::VideoFrame frame = buider.build();
	for (rtc::VideoSinkInterface<webrtc::VideoFrame>* s : sinks) {
		if (s != nullptr)
			s->OnFrame(frame);
	}
}

void FrameAudioSource::AddSink(webrtc::AudioTrackSinkInterface* sink) {
	rtc::CritScope lock(&sink_lock_);
	for (webrtc::AudioTrackSinkInterface* s : sinks) {
		if (s == sink)
			return;
	}
	sinks.push_back(sink);
}

void FrameAudioSource::RemoveSink(webrtc::AudioTrackSinkInterface* sink) {
	rtc::CritScope lock(&sink_lock_);
	sinks.remove(sink);
}

void FrameAudioSource::OnData(const void* audio_data,
	int32_t bits_per_sample,
	int sample_rate,
	size_t number_of_channels,
	size_t number_of_frames) {
	rtc::CritScope lock(&sink_lock_);
	for (webrtc::AudioTrackSinkInterface* s : sinks) {
		if (s == nullptr)
			continue;
		s->OnData(audio_data, bits_per_sample, sample_rate, number_of_channels,
			number_of_frames);
	}
}

void* MediaStreamTrack_GetKind(void* ptr)
{
	auto track = (webrtc::MediaStreamTrackInterface*)(ptr);
	auto kind = track->kind();
	auto obj = new BytesBuffer(1);
	*obj->pointer = new char[kind.length() + 1];
	strcpy((char*)*obj->pointer, kind.c_str());
	obj->AddRef();
	return obj;
}

bool MediaStreamTrack_GetEnabled(void* ptr)
{
	auto track = (webrtc::MediaStreamTrackInterface*)(ptr);
	return track->enabled();
}

int MediaStreamTrack_GetState(void* ptr)
{
	auto track = (webrtc::MediaStreamTrackInterface*)(ptr);
	return track->state();
}

void MediaStreamTrack_SetEnabled(void* ptr, bool enabled)
{
	auto track = (webrtc::MediaStreamTrackInterface*)(ptr);
	track->set_enabled(enabled);
}

void* VideoSource_AddSink(void* ptr, WebrtcUnityResultCallback onI420FrameReady)
{
	auto observer = new rtc::RefCountedObject<VideoObserver>();
	observer->OnI420FrameReady = onI420FrameReady;

	webrtc::VideoTrackSourceInterface* videoTrack = (webrtc::VideoTrackSourceInterface*)(ptr);
	videoTrack->AddOrUpdateSink(observer, rtc::VideoSinkWants());

	//observer->AddRef();
	return observer;
}

void VideoSource_RemoveSink(void* ptr, void* sink)
{
	auto track = (webrtc::VideoTrackSourceInterface*)(ptr);
	auto sinkTyped = (VideoObserver*)sink;
	track->RemoveSink(sinkTyped);
	sinkTyped->OnI420FrameReady = nullptr;
}

void* AudioSource_AddSink(void* ptr, WebrtcUnityResultCallback onDataReady)
{
	auto observer = new rtc::RefCountedObject<AudioObserver>();
	observer->OnDataReady = onDataReady;

	webrtc::AudioSourceInterface* audioTrack =
		(webrtc::AudioSourceInterface*)(ptr);
	audioTrack->AddSink(observer);

	//observer->AddRef();
	return observer;
}

void AudioSource_RemoveSink(void* ptr, void* sink)
{
	auto track = (webrtc::AudioSourceInterface*)(ptr);
	auto sinkTyped = (AudioObserver*)sink;
	track->RemoveSink(sinkTyped);
	sinkTyped->OnDataReady = nullptr;
}

void* VideoTrack_AddSink(void* ptr, WebrtcUnityResultCallback onI420FrameReady)
{
	auto observer = new rtc::RefCountedObject<VideoObserver>();
	observer->OnI420FrameReady = onI420FrameReady;

	webrtc::VideoTrackInterface* videoTrack = (webrtc::VideoTrackInterface*)(ptr);
	videoTrack->AddOrUpdateSink(observer, rtc::VideoSinkWants());

	//observer->AddRef();
	return observer;
}

void VideoTrack_RemoveSink(void* ptr, void* sink)
{
	auto track = (webrtc::VideoTrackInterface*)(ptr);
	auto sinkTyped = (VideoObserver*)sink;
	track->RemoveSink(sinkTyped);
	sinkTyped->OnI420FrameReady = nullptr;
}

void* AudioTrack_AddSink(void* ptr, WebrtcUnityResultCallback onDataReady)
{
	auto observer = new rtc::RefCountedObject<AudioObserver>();
	observer->OnDataReady = onDataReady;

	webrtc::AudioTrackInterface* audioTrack =
		(webrtc::AudioTrackInterface*)(ptr);
	audioTrack->AddSink(observer);

	//observer->AddRef();
	return observer;
}

void AudioTrack_RemoveSink(void* ptr, void* sink)
{
	auto track = (webrtc::AudioTrackInterface*)(ptr);
	auto sinkTyped = (AudioObserver*)sink;
	track->RemoveSink(sinkTyped);
	sinkTyped->OnDataReady = nullptr;
}

void* FrameVideoSource_new()
{
	auto ptr = new FrameVideoSource();
	ptr->AddRef();
	return ptr;
}

void FrameVideoSource_SendFrame(void* ptr, uint8_t* data_y, uint8_t* data_u, uint8_t* data_v, uint8_t* data_a, int stride_y, int stride_u, int stride_v, int stride_a, uint32_t width, uint32_t height, int rotation, int64_t timestamp)
{
	auto typed = (FrameVideoSource*)(ptr);
	typed->NewFrame(data_y, data_u, data_v, data_a, stride_y, stride_u, stride_v, stride_a, width, height, rotation, timestamp);
}

void* FrameAudioSource_new()
{
	auto ptr = new FrameAudioSource();
	ptr->AddRef();
	return ptr;
}

void FrameAudioSource_SendFrame(void* ptr, void* audio_data, int bits_per_sample, int sample_rate, size_t number_of_channels, size_t number_of_frames)
{
	auto typed = (FrameAudioSource*)(ptr);
	typed->OnData(audio_data, bits_per_sample, sample_rate, number_of_channels, number_of_frames);
}
