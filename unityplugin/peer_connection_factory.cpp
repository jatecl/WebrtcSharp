
#include "peer_connection_factory.h"
#include "peer_connection.h"
#include "rtc_configuration.h"
#include <api\video\i420_buffer.h>
using namespace webrtc;

class SampleVideoCapturer : public rtc::VideoSourceInterface<VideoFrame> {
public:
	SampleVideoCapturer();
	~SampleVideoCapturer() override;

	void AddOrUpdateSink(rtc::VideoSinkInterface<VideoFrame>* sink,
		const rtc::VideoSinkWants& wants) override;
	void RemoveSink(rtc::VideoSinkInterface<VideoFrame>* sink) override;

protected:
	void OnFrame(const VideoFrame& frame);
	rtc::VideoSinkWants GetSinkWants();

private:
	void UpdateVideoAdapter();

	rtc::VideoBroadcaster broadcaster_;
	cricket::VideoAdapter video_adapter_;
};

class SampleVcmCapturer : public SampleVideoCapturer,
	public rtc::VideoSinkInterface<VideoFrame> {
public:
	static SampleVcmCapturer* Create(size_t width,
		size_t height,
		size_t target_fps,
		size_t capture_device_index);
	virtual ~SampleVcmCapturer();

	void OnFrame(const VideoFrame& frame) override;

private:
	SampleVcmCapturer();
	bool Init(size_t width,
		size_t height,
		size_t target_fps,
		size_t capture_device_index);
	void Destroy();

	rtc::scoped_refptr<VideoCaptureModule> vcm_;
	VideoCaptureCapability capability_;
};


class CapturerTrackSource : public webrtc::VideoTrackSource {
public:
	static rtc::scoped_refptr<CapturerTrackSource> Create(
		const size_t kDeviceIndex,
		const size_t kWidth,
		const size_t kHeight,
		const size_t kFps) {
		std::unique_ptr<SampleVcmCapturer> capturer = absl::WrapUnique(
			SampleVcmCapturer::Create(kWidth, kHeight, kFps, kDeviceIndex));
		if (!capturer) {
			return nullptr;
		}
		return new rtc::RefCountedObject<CapturerTrackSource>(std::move(capturer));
	}

protected:
	explicit CapturerTrackSource(
		std::unique_ptr<SampleVcmCapturer> capturer)
		: VideoTrackSource(/*remote=*/false), capturer_(std::move(capturer)) {}

private:
	rtc::VideoSourceInterface<webrtc::VideoFrame>* source() override {
		return capturer_.get();
	}
	std::unique_ptr<SampleVcmCapturer> capturer_;
};

void* PeerConnectionFactory_new()
{
	auto ptr = new PeerConnectionFactoryPointer();
	ptr->worker_thread = rtc::Thread::CreateWithSocketServer();
	ptr->worker_thread->Start();
	ptr->signaling_thread = rtc::Thread::CreateWithSocketServer();
	ptr->signaling_thread->Start();

	ptr->factory = webrtc::CreatePeerConnectionFactory(
		ptr->worker_thread.get(), ptr->worker_thread.get(), ptr->signaling_thread.get(),
		nullptr, webrtc::CreateBuiltinAudioEncoderFactory(),
		webrtc::CreateBuiltinAudioDecoderFactory(),
		std::unique_ptr<webrtc::VideoEncoderFactory>(
			new webrtc::MultiplexEncoderFactory(
				absl::make_unique<webrtc::InternalEncoderFactory>())),
		std::unique_ptr<webrtc::VideoDecoderFactory>(
			new webrtc::MultiplexDecoderFactory(
				absl::make_unique<webrtc::InternalDecoderFactory>())),
		nullptr, nullptr);

	ptr->AddRef();
	return ptr;
}

void* PeerConnectionFactory_CreatePeerConnection(void* ptr, void* config, void* observe)
{
	auto factory = (PeerConnectionFactoryPointer*)(ptr);
	auto config_ptr = (StructPointer<webrtc::PeerConnectionInterface::RTCConfiguration>*)(config);
	auto observe_ptr = (webrtc::PeerConnectionObserver*)(observe);

	auto peer_connection = factory->factory->CreatePeerConnection(
		config_ptr->data, nullptr, nullptr, observe_ptr);
	if (!peer_connection.get()) return nullptr;
	
	peer_connection->AddRef();
	return peer_connection;
}

void* PeerConnectionFactory_CreateVideoTrack(void* ptr, const char* label, void* source)
{
	auto factory = (PeerConnectionFactoryPointer*)(ptr);
	auto track = factory->factory->CreateVideoTrack(std::string(label), (webrtc::VideoTrackSourceInterface*)(source));
	track->AddRef();
	return track.get();
}

void* PeerConnectionFactory_CreateAudioTrack(void* ptr, const char* label, void* source)
{
	auto factory = (PeerConnectionFactoryPointer*)(ptr);
	auto track = factory->factory->CreateAudioTrack(std::string(label), (webrtc::AudioSourceInterface*)(source));
	track->AddRef();
	return track.get();
}

void* PeerConnectionFactory_CreateVideoSource(void* ptr, int index, int width, int height, int fps)
{
	auto source = CapturerTrackSource::Create(index, width, height, fps);
	if (source) source->AddRef();
	return source;
}

void* PeerConnectionFactory_CreateAudioSource(void* ptr)
{
	auto factory = (PeerConnectionFactoryPointer*)(ptr);
	cricket::AudioOptions option;
	auto source = factory->factory->CreateAudioSource(option);
	source->AddRef();
	return source.get();
}



PeerConnectionFactoryPointer::~PeerConnectionFactoryPointer()
{
	factory = nullptr;
	signaling_thread.reset();
	worker_thread.reset();
}


SampleVideoCapturer::SampleVideoCapturer() = default;
SampleVideoCapturer::~SampleVideoCapturer() = default;

void SampleVideoCapturer::OnFrame(const VideoFrame & frame) {
	int cropped_width = 0;
	int cropped_height = 0;
	int out_width = 0;
	int out_height = 0;

	if (!video_adapter_.AdaptFrameResolution(
		frame.width(), frame.height(), frame.timestamp_us() * 1000,
		&cropped_width, &cropped_height, &out_width, &out_height)) {
		// Drop frame in order to respect frame rate constraint.
		return;
	}

	if (out_height != frame.height() || out_width != frame.width()) {
		// Video adapter has requested a down-scale. Allocate a new buffer and
		// return scaled version.
		rtc::scoped_refptr<I420Buffer> scaled_buffer =
			I420Buffer::Create(out_width, out_height);
		scaled_buffer->ScaleFrom(*frame.video_frame_buffer()->ToI420());
		broadcaster_.OnFrame(VideoFrame::Builder()
			.set_video_frame_buffer(scaled_buffer)
			.set_rotation(kVideoRotation_0)
			.set_timestamp_us(frame.timestamp_us())
			.set_id(frame.id())
			.build());
	}
	else {
		// No adaptations needed, just return the frame as is.
		broadcaster_.OnFrame(frame);
	}
}

rtc::VideoSinkWants SampleVideoCapturer::GetSinkWants() {
	return broadcaster_.wants();
}

void SampleVideoCapturer::AddOrUpdateSink(
	rtc::VideoSinkInterface<VideoFrame> * sink,
	const rtc::VideoSinkWants & wants) {
	broadcaster_.AddOrUpdateSink(sink, wants);
	UpdateVideoAdapter();
}

void SampleVideoCapturer::RemoveSink(rtc::VideoSinkInterface<VideoFrame> * sink) {
	broadcaster_.RemoveSink(sink);
	UpdateVideoAdapter();
}

void SampleVideoCapturer::UpdateVideoAdapter() {
	rtc::VideoSinkWants wants = broadcaster_.wants();
	video_adapter_.OnResolutionFramerateRequest(
		wants.target_pixel_count, wants.max_pixel_count, wants.max_framerate_fps);
}


SampleVcmCapturer::SampleVcmCapturer() : vcm_(nullptr) {}

bool SampleVcmCapturer::Init(size_t width,
	size_t height,
	size_t target_fps,
	size_t capture_device_index) {
	std::unique_ptr<VideoCaptureModule::DeviceInfo> device_info(
		VideoCaptureFactory::CreateDeviceInfo());

	char device_name[256];
	char unique_name[256];
	if (device_info->GetDeviceName(static_cast<uint32_t>(capture_device_index),
		device_name, sizeof(device_name), unique_name,
		sizeof(unique_name)) != 0) {
		Destroy();
		return false;
	}

	vcm_ = webrtc::VideoCaptureFactory::Create(unique_name);
	if (!vcm_) {
		return false;
	}
	vcm_->RegisterCaptureDataCallback(this);

	device_info->GetCapability(vcm_->CurrentDeviceName(), 0, capability_);

	capability_.width = static_cast<int32_t>(width);
	capability_.height = static_cast<int32_t>(height);
	capability_.maxFPS = static_cast<int32_t>(target_fps);
	capability_.videoType = VideoType::kI420;

	if (vcm_->StartCapture(capability_) != 0) {
		Destroy();
		return false;
	}

	RTC_CHECK(vcm_->CaptureStarted());

	return true;
}

SampleVcmCapturer* SampleVcmCapturer::Create(size_t width,
	size_t height,
	size_t target_fps,
	size_t capture_device_index) {
	std::unique_ptr<SampleVcmCapturer> vcm_capturer(new SampleVcmCapturer());
	if (!vcm_capturer->Init(width, height, target_fps, capture_device_index)) {
		RTC_LOG(LS_WARNING) << "Failed to create VcmCapturer(w = " << width
			<< ", h = " << height << ", fps = " << target_fps
			<< ")";
		return nullptr;
	}
	return vcm_capturer.release();
}

void SampleVcmCapturer::Destroy() {
	if (!vcm_)
		return;

	vcm_->StopCapture();
	vcm_->DeRegisterCaptureDataCallback();
	// Release reference to VCM.
	vcm_ = nullptr;
}

SampleVcmCapturer::~SampleVcmCapturer() {
	Destroy();
}

void SampleVcmCapturer::OnFrame(const VideoFrame& frame) {
	SampleVideoCapturer::OnFrame(frame);
}