using System;
using System.Runtime.InteropServices;

namespace WebrtcSharp
{
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