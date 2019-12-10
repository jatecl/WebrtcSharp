using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WebrtcSharp
{
    /// <summary>
    /// 媒体类型
    /// </summary>
    public enum MediaType
    {
        /// <summary>
        /// 音频
        /// </summary>
        Audio,
        /// <summary>
        /// 视频
        /// </summary>
        Video,
        /// <summary>
        /// 数据
        /// </summary>
        Data
    }
    /// <summary>
    /// 未知类型的媒体轨道
    /// </summary>
    class UnkownMediaStreamTrack : MediaStreamTrack
    {
        /// <summary>
        /// 媒体类型
        /// </summary>
        private string kind;
        /// <summary>
        /// 创建媒体轨道
        /// </summary>
        /// <param name="handler"></param>
        public UnkownMediaStreamTrack(IntPtr handler, IDispatcher dispatcher) : base(handler, dispatcher) { }
        /// <summary>
        /// 媒体类型
        /// </summary>
        public override string Kind => kind;
        /// <summary>
        /// 设置媒体类型
        /// </summary>
        /// <param name="kind"></param>
        internal void SetKind(string kind)
        {
            this.kind = kind;
        }
    }
    /// <summary>
    /// 媒体接收器
    /// </summary>
    public class RtpReceiver : WebrtcObject
    {
        /// <summary>
        /// C++持有的第一个数据包到达的事件回调函数
        /// </summary>
        private WebrtcUnityStateCallback NativeFirstPacketReceived;
        /// <summary>
        /// 持有一个数据接收器
        /// </summary>
        /// <param name="handler">数据接收器指针</param>
        internal protected RtpReceiver(IntPtr handler, IDispatcher dispatcher) : base(handler)
        {
            IntPtr track = default;
            dispatcher.Invoke(() =>
            {
                WebrtcObject_AddRef(handler);
                track = RtpReceiver_GetMediaStreamTrack(handler);
            });
            Dispatcher = dispatcher;
            if (track == IntPtr.Zero) throw new Exception("无法获取媒体轨道");
            IntPtr proxyPtr = default;
            dispatcher.Invoke(() => proxyPtr = MediaStreamTrack.MediaStreamTrack_GetKind(track));
            if (proxyPtr == IntPtr.Zero) throw new Exception("无法获取媒体类型");
            var buffer = new PointerArray(proxyPtr);
            unsafe
            {
                byte** pointer = (byte**)buffer.GetBuffer();
                var kind = new string((sbyte*)*pointer);
                if (kind == "video") Track = new VideoTrack(track, dispatcher);
                else if (kind == "audio") Track = new AudioTrack(track, dispatcher);
                else
                {
                    var unkown = new UnkownMediaStreamTrack(track, dispatcher);
                    unkown.SetKind(kind);
                    Track = unkown;
                }
            }
            NativeFirstPacketReceived = state => FirstPacketReceived?.Invoke((MediaType)state);
            dispatcher.Invoke(() => RtpReceiver_SetFirstPacketReceivedObserve(handler, NativeFirstPacketReceived));
        }
        /// <summary>
        /// 同步销毁
        /// </summary>
        public override void Release()
        {
            if (Handler == IntPtr.Zero) return;
            Dispatcher.Invoke(() => base.Release());
        }
        /// <summary>
        /// 接收到的媒体轨道
        /// </summary>
        public MediaStreamTrack Track { get; }
        /// <summary>
        /// 同步执行器
        /// </summary>
        public IDispatcher Dispatcher { get; }

        /// <summary>
        /// 当第一个数据包收到时发生
        /// </summary>
        public event Action<MediaType> FirstPacketReceived;
        /// <summary>
        /// C++ API：获取接收器的媒体轨道
        /// </summary>
        /// <param name="ptr">接收器指针</param>
        /// <returns>媒体轨道指针</returns>
        [DllImport(UnityPluginDll)] internal static extern IntPtr RtpReceiver_GetMediaStreamTrack(IntPtr ptr);
        /// <summary>
        /// C++ API：设置首次接到数据包的回调
        /// </summary>
        /// <param name="ptr">接收器指针</param>
        /// <param name="FirstPacketReceived">首次接到数据包的回调</param>
        [DllImport(UnityPluginDll)] internal static extern void RtpReceiver_SetFirstPacketReceivedObserve(IntPtr ptr, WebrtcUnityStateCallback FirstPacketReceived);
    }
}
