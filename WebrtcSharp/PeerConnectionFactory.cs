﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebrtcSharp
{
    /// <summary>
    /// 视频帧类型列表
    /// </summary>
    public enum VideoType
    {
        Unknown,
        I420,
        IYUV,
        RGB24,
        ABGR,
        ARGB,
        ARGB4444,
        RGB565,
        ARGB1555,
        YUY2,
        YV12,
        UYVY,
        MJPEG,
        NV21,
        NV12,
        BGRA,
    };
    /// <summary>
    /// 同步执行器
    /// </summary>
    public interface IDispatcher
    {
        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="action">要执行的方法</param>
        void Invoke(Action action);
    }
    /// <summary>
    /// p2p连接创建器
    /// </summary>
    public class PeerConnectionFactory : WebrtcObject
    {
        /// <summary>
        /// 创建一个P2P连接创建器
        /// </summary>
        public PeerConnectionFactory() : base(PeerConnectionFactory_new())
        {
            SignalingThread = new SignalingThreadDispatcher(this);
        }
        /// <summary>
        /// 信令线程
        /// </summary>
        private class SignalingThreadDispatcher : IDispatcher
        {
            /// <summary>
            /// 所属创建器
            /// </summary>
            private PeerConnectionFactory factory;
            /// <summary>
            /// 创建信令线程执行器
            /// </summary>
            /// <param name="factory">所属创建器</param>
            public SignalingThreadDispatcher(PeerConnectionFactory factory)
            {
                this.factory = factory;
            }
            /// <summary>
            /// 执行
            /// </summary>
            /// <param name="action">要执行的方法</param>
            public void Invoke(Action action)
            {
                if (factory.Handler == IntPtr.Zero) return;
                WebrtcUnityCallback native = () => action();
                var holder = new UnmanageHolder();
                holder.Hold(native);
                PeerConnectionFactory_SignalingThreadInvoke(factory.Handler, native);
                holder.StillHere();
            }
            /// <summary>
            /// C++ API：在信令线程上执行
            /// </summary>
            /// <param name="ptr">创建器指针</param>
            /// <param name="callback">要执行的方法</param>
            [DllImport(UnityPluginDll)] internal static extern void PeerConnectionFactory_SignalingThreadInvoke(IntPtr ptr, WebrtcUnityCallback callback);
        }
        /// <summary>
        /// 信令线程
        /// </summary>
        public IDispatcher SignalingThread { get; }
        /// <summary>
        /// 创建一个P2P连接
        /// </summary>
        /// <param name="configuration">连接设置</param>
        /// <param name="observe">事件捕获器</param>
        /// <returns>P2P连接</returns>
        public PeerConnection CreatePeerConnection(RTCConfiguration configuration)
        {
            try
            {
                var observe = new PeerConnection.PeerConnectionObserver();
                var handle = PeerConnectionFactory_CreatePeerConnection(Handler, configuration.Handler, observe.Handler);
                if (handle == IntPtr.Zero) return null;
                return new PeerConnection(handle, SignalingThread, observe);
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp);
                return null;
            }
        }
        /// <summary>
        /// 创建视频轨道
        /// </summary>
        /// <param name="label">视频轨道标识</param>
        /// <param name="source">视频源</param>
        /// <returns></returns>
        public VideoTrack CreateVideoTrack(string label, VideoSource source)
        {
            IntPtr ptr = default;
            SignalingThread.Invoke(() => ptr = PeerConnectionFactory_CreateVideoTrack(Handler, label, source.Handler));
            if (ptr == IntPtr.Zero) return null;
            return new VideoTrack(ptr, this.SignalingThread);
        }
        /// <summary>
        /// 创建音频轨道
        /// </summary>
        /// <param name="label">音频轨道标识</param>
        /// <param name="source">音频源</param>
        /// <returns></returns>
        public AudioTrack CreateAudioTrack(string label, AudioSource source)
        {
            IntPtr ptr = default;
            SignalingThread.Invoke(() => ptr = PeerConnectionFactory_CreateAudioTrack(Handler, label, source.Handler));
            if (ptr == IntPtr.Zero) return null;
            return new AudioTrack(ptr, this.SignalingThread);
        }
        /// <summary>
        /// 创建视频源
        /// </summary>
        /// <param name="index">摄像头顺序</param>
        /// <param name="width">支持的分辨率宽度</param>
        /// <param name="height">支持的分辨率高度</param>
        /// <param name="fps">支持的帧率</param>
        /// <returns>视频源</returns>
        public VideoSource CreateVideoSource(int index, int width, int height, int fps)
        {
            IntPtr ptr = default;
            SignalingThread.Invoke(() => ptr = PeerConnectionFactory_CreateVideoSource(Handler, index, width, height, fps));
            if (ptr == IntPtr.Zero) return null;
            return new VideoSource(ptr, SignalingThread);
        }
        /// <summary>
        /// 创建逐帧视频源
        /// </summary>
        /// <returns>逐帧视频源</returns>
        public FrameVideoSource CreateFrameVideoSource()
        {
            FrameVideoSource source = default;
            SignalingThread.Invoke(() => source = new FrameVideoSource(this.SignalingThread));
            if (source.Handler == IntPtr.Zero) return null;
            return source;
        }
        /// <summary>
        /// 创建音频源
        /// </summary>
        /// <returns>音频源</returns>
        public AudioSource CreateAudioSource()
        {
            IntPtr ptr = default;
            SignalingThread.Invoke(() => ptr = PeerConnectionFactory_CreateAudioSource(Handler));
            if (ptr == IntPtr.Zero) return null;
            return new AudioSource(ptr, SignalingThread);
        }
        /// <summary>
        /// 创建音频源
        /// </summary>
        /// <returns>音频源</returns>
        public FrameAudioSource CreateFrameAudioSource()
        {
            FrameAudioSource source = default;
            SignalingThread.Invoke(() => source = new FrameAudioSource(SignalingThread));
            if (source.Handler == IntPtr.Zero) return null;
            return source;
        }
        /// <summary>
        /// 获取所有视频设备
        /// </summary>
        /// <returns>所有视频设备</returns>
        public unsafe static VideoDeviceInfo[] GetDeviceInfo()
        {
            try
            {
                var ptrs = PeerConnectionFactory_GetDeviceInfo();
                if (ptrs == null) return new VideoDeviceInfo[0];
                var buffer = new PointerArray(ptrs);
                byte** pointer = (byte**)buffer.GetBuffer();
                byte** it = pointer;
                var list = new List<VideoDeviceInfo>();
                var idx = 0;
                while (*it != null)
                {
                    var name = new string((sbyte*)(*it));
                    ++it;
                    var id = new string((sbyte*)(*it));
                    ++it;
                    var pid = new string((sbyte*)(*it));
                    ++it;
                    var en = *(int*)*it;
                    ++it;
                    list.Add(new VideoDeviceInfo
                    {
                        DeviceName = name,
                        DeviceId = id,
                        ProductId = pid,
                        Enable = en,
                        Index = idx
                    });
                    ++idx;
                }
                return list.ToArray();
            }
            catch (Exception exp)
            {
                return new VideoDeviceInfo[0];
            }
        }
        /// <summary>
        /// 获取单个视频设备支持的所有分辨率
        /// </summary>
        /// <param name="index">视频设备顺序</param>
        /// <returns>视频设备支持的所有分辨率</returns>
        public unsafe static VideoDeviceCapabilities[] GetDeviceCapabilities(int index)
        {
            try
            {
                var ptrs = PeerConnectionFactory_GetDeviceCapabilities(index);
                if (ptrs == IntPtr.Zero) return new VideoDeviceCapabilities[0];
                var buffer = new PointerArray(ptrs);
                byte** pointer = (byte**)buffer.GetBuffer();
                byte** it = pointer;
                var list = new List<VideoDeviceCapabilities>();
                while (*it != null)
                {
                    int* info = (int*)*it;
                    ++it;
                    int width = *info;
                    ++info;
                    int height = *info;
                    ++info;
                    int fps = *info;
                    ++info;
                    int type = *info;
                    ++info;
                    int interlaced = *info;
                    ++info;
                    list.Add(new VideoDeviceCapabilities
                    {
                        Width = width,
                        Height = height,
                        Fps = fps,
                        VideoType = (VideoType)type,
                        Interlaced = interlaced > 0
                    });
                }
                return list.ToArray();
            }
            catch (Exception exp)
            {
                return new VideoDeviceCapabilities[0];
            }
        }
        /// <summary>
        /// C++ API：创建一个P2P连接创建器 
        /// </summary>
        /// <returns>P2P连接创建器指针</returns>
        [DllImport(UnityPluginDll)]
        internal static extern IntPtr PeerConnectionFactory_new();
        /// <summary>
        /// C++ API：创建一个P2P连接
        /// </summary>
        /// <param name="ptr">P2P连接创建器指针</param>
        /// <param name="config">配置指针</param>
        /// <param name="observe">事件指针</param>
        /// <returns>P2P连接指针</returns>
        [DllImport(UnityPluginDll)]
        internal static extern IntPtr PeerConnectionFactory_CreatePeerConnection(IntPtr ptr, IntPtr config, IntPtr observe);
        /// <summary>
        /// C++ API：创建一个视频轨道
        /// </summary>
        /// <param name="ptr">P2P连接创建器指针</param>
        /// <param name="label">视频轨道标识</param>
        /// <param name="source">视频源</param>
        /// <returns>视频轨道指针</returns>
        [DllImport(UnityPluginDll)] internal static extern IntPtr PeerConnectionFactory_CreateVideoTrack(IntPtr ptr, string label, IntPtr source);
        /// <summary>
        /// C++ API：创建一个音频轨道
        /// </summary>
        /// <param name="ptr">P2P连接创建器指针</param>
        /// <param name="label">音频轨道标识</param>
        /// <param name="source">音频源</param>
        /// <returns>音频轨道指针</returns>
        [DllImport(UnityPluginDll)] internal static extern IntPtr PeerConnectionFactory_CreateAudioTrack(IntPtr ptr, string label, IntPtr source);
        /// <summary>
        /// C++ API：创建视频源
        /// </summary>
        /// <param name="ptr">P2P连接创建器指针</param>
        /// <param name="index">摄像头顺序</param>
        /// <param name="width">支持的分辨率宽度</param>
        /// <param name="height">支持的分辨率高度</param>
        /// <param name="fps">支持的帧率</param>
        /// <returns>视频源</returns>
        [DllImport(UnityPluginDll)] internal static extern IntPtr PeerConnectionFactory_CreateVideoSource(IntPtr ptr, int index, int width, int height, int fps);
        /// <summary>
        /// C++ API：创建音频源
        /// </summary>
        /// <param name="ptr">P2P连接创建器指针</param>
        /// <returns>音频源</returns>
        [DllImport(UnityPluginDll)] internal static extern IntPtr PeerConnectionFactory_CreateAudioSource(IntPtr ptr);
        /// <summary>
        /// C++ API：获取所有视频设备
        /// </summary>
        /// <returns>所有视频设备描述的字节缓冲指针</returns>
        [DllImport(UnityPluginDll)] internal static extern IntPtr PeerConnectionFactory_GetDeviceInfo();
        /// <summary>
        /// C++ API：获取单个视频设备支持的所有分辨率
        /// </summary>
        /// <param name="index">视频设备顺序</param>
        /// <returns>视频设备支持的所有分辨率描述的字节缓冲指针</returns>
        [DllImport(UnityPluginDll)] internal static extern IntPtr PeerConnectionFactory_GetDeviceCapabilities(int index);
    }
}
