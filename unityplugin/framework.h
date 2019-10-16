#pragma once

#define WIN32_LEAN_AND_MEAN             // 从 Windows 头文件中排除极少使用的内容
// Windows 头文件
#include <Windows.h>

#include "absl/memory/memory.h"
#include "api/audio_codecs/builtin_audio_decoder_factory.h"
#include "api/audio_codecs/builtin_audio_encoder_factory.h"
#include "api/create_peerconnection_factory.h"
#include "media/engine/internal_decoder_factory.h"
#include "media/engine/internal_encoder_factory.h"
#include "media/engine/multiplex_codec_factory.h"
#include "modules/audio_device/include/audio_device.h"
#include "modules/audio_processing/include/audio_processing.h"
#include "modules/video_capture/video_capture_factory.h"
#include "pc/video_track_source.h"
#include "test/vcm_capturer.h"
#include "rtc_base/ref_counted_object.h"


#pragma comment(lib, "winmm.lib")
#pragma comment(lib, "secur32.lib")
#pragma comment(lib, "dmoguids.lib")
#pragma comment(lib, "wmcodecdspuuid.lib")
#pragma comment(lib, "msdmo.lib")
#pragma comment(lib, "strmiids.lib")
#if _DEBUG
#pragma comment(lib, "../webrtc/src/out/Debug/obj/webrtc.lib")
#else
#pragma comment(lib, "../webrtc/src/out/Default/obj/webrtc.lib")
#endif

//无参回调
typedef void (*WebrtcUnityCallback)();
//通知状态的回调
typedef void (*WebrtcUnityStateCallback)(int state);
//返回数据的回调
typedef void (*WebrtcUnityResultCallback)(void* val);
//返回两个数据的回调
typedef void (*WebrtcUnityResult2Callback)(void* val, void* val2);

extern "C" {
	//删除对象引用
	__declspec(dllexport) void WebrtcObject_delete(void* ptr);
	//返回字节数组的指针
	__declspec(dllexport) void* StringBuffer_GetBuffer(void* ptr);
	//拉取视频设备信息
	__declspec(dllexport) void* PeerConnectionFactory_GetDeviceInfo();
	//拉取某个视频设备支持的分辨率列表
	__declspec(dllexport) void* PeerConnectionFactory_GetDeviceCapabilities(int index);
}

//字节数组持有者
class StringBuffer : public rtc::RefCountedObject<rtc::RefCountInterface> {
public:
	//字节数组
	char** pointer;
	//生成一个新的字节数组
	StringBuffer(int length);
	//销毁字节数组
	~StringBuffer();
};

//结构体持有者
template <class T>
class StructPointer : public rtc::RefCountedObject<rtc::RefCountInterface> {
public:
	T data;
};