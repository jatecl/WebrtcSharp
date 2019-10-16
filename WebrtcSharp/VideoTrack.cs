using System;
using System.Runtime.InteropServices;

namespace WebrtcSharp
{
    /// <summary>
    /// 视频轨道
    /// </summary>
    public class VideoTrack : MediaStreamTrack
    {
        /// <summary>
        /// 当前轨道监听器
        /// </summary>
        private WebrtcObject sink;
        /// <summary>
        /// 持有一个视频轨道
        /// </summary>
        /// <param name="ptr">视频轨道指针</param>
        public VideoTrack(IntPtr ptr) : base(ptr)
        {
            NativeDataReady = val => OnDataReady(val);
        }
        /// <summary>
        /// C++持有的视频数据监听回调
        /// </summary>
        private WebrtcUnityResultCallback NativeDataReady;
        /// <summary>
        /// 轨道类型
        /// </summary>
        public override string Kind => "video";
        /// <summary>
        /// 私有的收到视频帧事件
        /// </summary>
        private event Action<VideoFrame> FramePrivate;
        /// <summary>
        /// 收到视频帧事件
        /// </summary>
        public event Action<VideoFrame> Frame
        {
            add
            {
                AddSink();
                FramePrivate += value;
            }
            remove
            {
                FramePrivate -= value;
                if (FramePrivate == null) RemoveSink();
            }
        }
        /// <summary>
        /// 添加监听器
        /// </summary>
        private void AddSink()
        {
            if (sink != null) return;
            sink = Create<WebrtcObject>(VideoTrack_AddSink(Handler, NativeDataReady));
        }
        /// <summary>
        /// 删除监听器
        /// </summary>
        private void RemoveSink()
        {
            if (sink == null) return;
            VideoTrack_RemoveSink(Handler, sink.Handler);
            sink = null;
        }
        /// <summary>
        /// 在销毁时，先删除监听器
        /// </summary>
        protected override void Delete()
        {
            RemoveSink();
            base.Delete();
        }
        /// <summary>
        /// 处理收到的视频帧
        /// </summary>
        /// <param name="val">视频帧指针</param>
        private unsafe void OnDataReady(IntPtr val)
        {
            if (FramePrivate == null) return;
            void** ptrs = (void**)val.ToPointer();
            byte* datay = (byte*)*ptrs;
            ++ptrs;
            byte* datau = (byte*)*ptrs;
            ++ptrs;
            byte* datav = (byte*)*ptrs;
            ++ptrs;
            byte* dataa = (byte*)*ptrs;
            ++ptrs;
            int stridey = *(int*)*ptrs;
            ++ptrs;
            int strideu = *(int*)*ptrs;
            ++ptrs;
            int stridev = *(int*)*ptrs;
            ++ptrs;
            int stridea = *(int*)*ptrs;
            ++ptrs;
            int width = *(int*)*ptrs;
            ++ptrs;
            int height = *(int*)*ptrs;
            ++ptrs;
            int rotation = *(int*)*ptrs;
            ++ptrs;
            long time = *(long*)*ptrs;
            ++ptrs;
            FramePrivate.Invoke(new VideoFrame
            {
                DataY = new IntPtr(datay),
                DataU = new IntPtr(datau),
                DataV = new IntPtr(datav),
                DataA = new IntPtr(dataa),
                StrideY = stridey,
                StrideU = strideu,
                StrideV = stridev,
                StrideA = stridea,
                Width = width,
                Height = height,
                Rotation = rotation,
                Timestamp = time
            });
        }
        /// <summary>
        /// C++ API：添加监听器
        /// </summary>
        /// <param name="ptr">视频媒体轨道指针</param>
        /// <param name="onI420FrameReady">收到视频的回调函数</param>
        /// <returns>监听器指针</returns>
        [DllImport(UnityPluginDll)] internal static extern IntPtr VideoTrack_AddSink(IntPtr ptr, WebrtcUnityResultCallback onI420FrameReady);
        /// <summary>
        /// C++ API：删除监听器
        /// </summary>
        /// <param name="ptr">视频媒体轨道指针</param>
        /// <param name="sink">监听器指针</param>
        [DllImport(UnityPluginDll)] internal static extern void VideoTrack_RemoveSink(IntPtr ptr, IntPtr sink);
    }
}