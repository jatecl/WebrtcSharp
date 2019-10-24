using System;
using System.Runtime.InteropServices;

namespace WebrtcSharp
{
    /// <summary>
    /// YUVA420p视频帧
    /// </summary>
    public class VideoFrame
    {
        /// <summary>
        /// 创建一个空的视频帧
        /// </summary>
        public VideoFrame() { }
        /// <summary>
        /// 创建视频帧
        /// </summary>
        /// <param name="dataY">YUVA420p Y指针</param>
        /// <param name="dataU">YUVA420p U指针</param>
        /// <param name="dataV">YUVA420p V指针</param>
        /// <param name="dataA">YUVA420p A指针</param>
        /// <param name="strideY">YUVA420p Y宽度</param>
        /// <param name="strideU">YUVA420p U宽度</param>
        /// <param name="strideV">YUVA420p V宽度</param>
        /// <param name="strideA">YUVA420p A宽度</param>
        /// <param name="width">图片宽度</param>
        /// <param name="height">图片高度</param>
        /// <param name="rotation">旋转角度</param>
        /// <param name="timestamp">时间戳</param>
        public VideoFrame(IntPtr dataY, IntPtr dataU, IntPtr dataV, IntPtr dataA, int strideY, int strideU, int strideV, int strideA, int width, int height, int rotation, long timestamp)
        {
            this.DataY = dataY;
            this.DataU = dataU;
            this.DataV = dataV;
            this.DataA = dataA;
            this.StrideY = strideY;
            this.StrideU = strideU;
            this.StrideV = strideV;
            this.StrideA = strideA;
            this.Width = width;
            this.Height = height;
            this.Rotation = rotation;
            this.Timestamp = timestamp;
        }
        /// <summary>
        /// YUVA420p Y指针
        /// </summary>
        public IntPtr DataY { get; set; }
        /// <summary>
        /// YUVA420p U指针
        /// </summary>
        public IntPtr DataU { get; set; }
        /// <summary>
        /// YUVA420p V指针
        /// </summary>
        public IntPtr DataV { get; set; }
        /// <summary>
        /// YUVA420p A指针
        /// </summary>
        public IntPtr DataA { get; set; }
        /// <summary>
        /// YUVA420p Y宽度
        /// </summary>
        public int StrideY { get; set; }
        /// <summary>
        /// YUVA420p U宽度
        /// </summary>
        public int StrideU { get; set; }
        /// <summary>
        /// YUVA420p V宽度
        /// </summary>
        public int StrideV { get; set; }
        /// <summary>
        /// YUVA420p A宽度
        /// </summary>
        public int StrideA { get; set; }
        /// <summary>
        /// 图片宽度
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// 图片高度
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// 旋转角度
        /// </summary>
        public int Rotation { get; set; }
        /// <summary>
        /// 时间戳
        /// </summary>
        public long Timestamp { get; set; }
    }
    /// <summary>
    /// 音频帧
    /// </summary>
    public class AudioFrame
    {
        /// <summary>
        /// 创建一个空的音频帧
        /// </summary>
        public AudioFrame() { }
        /// <summary>
        /// 创建一个音频帧
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="bitsPerSample">bits per samplle</param>
        /// <param name="sampleRate">sample rate</param>
        /// <param name="numberOfChannels">number of channels</param>
        /// <param name="numberOfFrames">number of frames</param>
        public AudioFrame(IntPtr data, int bitsPerSample, int sampleRate, int numberOfChannels, int numberOfFrames)
        {
            Data = data;
            BitsPerSample = bitsPerSample;
            SampleRate = sampleRate;
            Channels = numberOfChannels;
            Frames = numberOfFrames;
        }
        /// <summary>
        /// 音频数据
        /// </summary>
        public IntPtr Data { get; set; }
        /// <summary>
        /// bits per sample
        /// </summary>
        public int BitsPerSample { get; set; }
        /// <summary>
        /// sample rate
        /// </summary>
        public int SampleRate { get; set; }
        /// <summary>
        /// number of channels
        /// </summary>
        public int Channels { get; set; }
        /// <summary>
        /// number of frames
        /// </summary>
        public int Frames { get; set; }
    }
    /// <summary>
    /// 媒体轨道状态
    /// </summary>
    public enum TrackState
    {
        /// <summary>
        /// 激活
        /// </summary>
        Live,
        /// <summary>
        /// 已结束
        /// </summary>
        Ended,
    }
    /// <summary>
    /// 媒体轨道
    /// </summary>
    public abstract class MediaStreamTrack : WebrtcObject
    {
        /// <summary>
        /// 持有一个媒体轨道
        /// </summary>
        /// <param name="handler">媒体轨道指针</param>
        public MediaStreamTrack(IntPtr handler) : base(handler) { }
        /// <summary>
        /// 轨道的媒体类型
        /// </summary>
        public abstract string Kind { get; }
        /// <summary>
        /// 获取或这种轨道的可用状态
        /// </summary>
        public bool Enabled
        {
            get
            {
                return MediaStreamTrack_GetEnabled(Handler);
            }
            set
            {
                MediaStreamTrack_SetEnabled(Handler, value);
                EnabledChanged?.Invoke(this, value);
            }
        }
        /// <summary>
        /// 可用状态发生变化时
        /// </summary>
        public event Action<MediaStreamTrack, bool> EnabledChanged;
        /// <summary>
        /// 轨道是否已结束
        /// </summary>
        public TrackState State
        {
            get
            {
                return (TrackState)MediaStreamTrack_GetState(Handler);
            }
        }
        /// <summary>
        /// C++ API：获得媒体轨道类型
        /// </summary>
        /// <param name="ptr">媒体轨道指针</param>
        /// <returns>字节缓冲指针</returns>
        [DllImport(UnityPluginDll)] internal static extern IntPtr MediaStreamTrack_GetKind(IntPtr ptr);
        /// <summary>
        /// C++ API：获取媒体轨道的可用状态
        /// </summary>
        /// <param name="ptr">媒体轨道指针</param>
        /// <returns>媒体轨道的可用状态</returns>
        [DllImport(UnityPluginDll)] internal static extern bool MediaStreamTrack_GetEnabled(IntPtr ptr);
        /// <summary>
        /// 获取媒体是否已结束
        /// </summary>
        /// <param name="ptr">媒体轨道指针</param>
        /// <returns>是否已结束</returns>
        [DllImport(UnityPluginDll)] internal static extern int MediaStreamTrack_GetState(IntPtr ptr);
        /// <summary>
        /// C++ API：设置媒体轨道的可用状态
        /// </summary>
        /// <param name="ptr">媒体轨道指针</param>
        /// <param name="enabled">媒体轨道的可用状态</param>
        [DllImport(UnityPluginDll)] internal static extern void MediaStreamTrack_SetEnabled(IntPtr ptr, bool enabled);


    }
}