using System.Collections.Generic;
using System.Linq;
using WebrtcSharp;

namespace Relywisdom
{
    /// <summary>
    /// 为了统一和其他语言的接口，做的一个api转发
    /// </summary>
    public static class RtcNavigator
    {
        public static T Get<T>(this Dictionary<string, object> dic, string key)
        {
            if (!dic.ContainsKey(key)) return default(T);
            return (T)dic[key];
        }
        private static PeerConnectionFactory Facotry { get; } = new WebrtcSharp.PeerConnectionFactory();

        /// <summary>
        /// 创建媒体流
        /// </summary>
        /// <returns></returns>
        public static MediaStream createMediaStream()
        {
            return new MediaStream();
        }

        /// <summary>
        /// 创建视频轨道
        /// </summary>
        /// <param name="source">视频源描述</param>
        /// <returns></returns>
        public static MediaStreamTrack createVideoTrack(string source)
        {
            var index = GetVideoIndexByName(source);
            var caps = PeerConnectionFactory.GetDeviceCapabilities(index);
            /*
            var matchs = caps.Where(c => c.VideoType == VideoType.I420).ToArray();
            if (matchs.Length > 0) caps = matchs;
            VideoDeviceCapabilities cap = null;
            if (caps.Length > 0) cap = caps[0];
            else cap = new VideoDeviceCapabilities
            {
                Width = 640,
                Height = 480,
                Fps = 30
            };
            return createVideoTrack(Facotry.CreateVideoSource(index, cap.Width - 1, cap.Height - 1, 15) ?? new FrameVideoSource());
            */
            var video = Facotry.CreateVideoSource(index, 640, 480, 30);
            return createVideoTrack(video);
        }

        /// <summary>
        /// 创建音频轨道
        /// </summary>
        /// <param name="source">视频源描述</param>
        /// <returns></returns>
        public static MediaStreamTrack createAudioTrack(string source)
        {
            return createAudioTrack(createAudioSource(source));
        }

        /// <summary>
        /// 获取所有可用的视频源
        /// </summary>
        /// <returns></returns>
        public static string[] getVideoDevices()
        {
            return PeerConnectionFactory.GetDeviceInfo().Select(info => info.DeviceName).ToArray();
        }

        /// <summary>
        /// 获取所有可用的音频源
        /// </summary>
        /// <returns></returns>
        public static string[] getAudioDevices()
        {
            return new string[] { "default audio device" };
        }

        /// <summary>
        /// 创建一个P2P连接
        /// </summary>
        /// <param name="configuration">连接设置</param>
        /// <param name="observe">事件捕获器</param>
        /// <returns>P2P连接</returns>
        public static PeerConnection createPeerConnection(RTCConfiguration configuration)
        {
            return Facotry.CreatePeerConnection(configuration);
        }
        /// <summary>
        /// 创建视频轨道
        /// </summary>
        /// <param name="label">视频轨道标识</param>
        /// <param name="source">视频源</param>
        /// <returns></returns>
        public static VideoTrack createVideoTrack(VideoSource source)
        {
            return Facotry.CreateVideoTrack("video_label", source);
        }
        /// <summary>
        /// 创建音频轨道
        /// </summary>
        /// <param name="label">音频轨道标识</param>
        /// <param name="source">音频源</param>
        /// <returns></returns>
        public static AudioTrack createAudioTrack(AudioSource source)
        {
            return Facotry.CreateAudioTrack("audio_label", source);
        }
        /// <summary>
        /// 创建视频源
        /// </summary>
        /// <param name="index">摄像头顺序</param>
        /// <param name="width">支持的分辨率宽度</param>
        /// <param name="height">支持的分辨率高度</param>
        /// <param name="fps">支持的帧率</param>
        /// <returns>视频源</returns>
        public static VideoSource createVideoSource(string name, int width, int height, int fps)
        {
            var index = GetVideoIndexByName(name);
            return Facotry.CreateVideoSource(index, width, height, fps);
        }
        private static int GetVideoIndexByName(string name)
        {
            var devices = getVideoDevices();
            for (var i = 0; i < devices.Length; ++i)
            {
                if (devices[i] == name) return i;
            }
            return -1;
        }

        /// <summary>
        /// 创建音频源
        /// </summary>
        /// <returns>音频源</returns>
        public static AudioSource createAudioSource(string name)
        {
            return Facotry.CreateAudioSource();
        }
        /// <summary>
        /// 获取单个视频设备支持的所有分辨率
        /// </summary>
        /// <param name="name">视频设备</param>
        /// <returns>视频设备支持的所有分辨率</returns>
        public static VideoDeviceCapabilities[] getCapabilities(string name)
        {
            var index = GetVideoIndexByName(name);
            return PeerConnectionFactory.GetDeviceCapabilities(index);
        }
    }
}
