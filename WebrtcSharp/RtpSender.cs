using System;
using System.Runtime.InteropServices;

namespace WebrtcSharp
{
    /// <summary>
    /// 媒体发送器
    /// </summary>
    public class RtpSender : WebrtcObject
    {
        /// <summary>
        /// 持有一个媒体发送器
        /// </summary>
        /// <param name="handler">媒体发送器指针</param>
        public RtpSender(IntPtr handler) : base(handler) { }
        /// <summary>
        /// 发送的媒体轨道
        /// </summary>
        public MediaStreamTrack Track { get; internal set; }
        /// <summary>
        /// 设置要发送的媒体轨道
        /// </summary>
        /// <param name="track">媒体轨道</param>
        /// <returns>是否设置成功</returns>
        public bool SetTrack(MediaStreamTrack track)
        {
            Track = track;
            return RtpSender_SetTrack(Handler, track == null ? default : track.Handler);
        }
        /// <summary>
        /// C++ API：设置要发送的媒体轨道
        /// </summary>
        /// <param name="ptr">媒体发送器指针</param>
        /// <param name="track">媒体轨道</param>
        /// <returns>是否设置成功</returns>
        [DllImport(UnityPluginDll)]
        internal static extern bool RtpSender_SetTrack(IntPtr ptr, IntPtr track);
    }
}