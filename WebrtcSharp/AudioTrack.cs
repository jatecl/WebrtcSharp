using System;
using System.Runtime.InteropServices;

namespace WebrtcSharp
{
    /// <summary>
    /// 音频轨道
    /// </summary>
    public class AudioTrack : MediaStreamTrack
    {
        /// <summary>
        /// 当前轨道监听器
        /// </summary>
        private WebrtcObject sink;
        /// <summary>
        /// 持有一个音频轨道
        /// </summary>
        /// <param name="handler"></param>
        public AudioTrack(IntPtr handler) : base(handler)
        {
            DataReadyCallback = ptr => this.OnDataReady(ptr);
        }
        /// <summary>
        /// 收到音频数据事件，内部使用
        /// </summary>
        private event Action<AudioFrame> FramePrivate;
        /// <summary>
        /// 收到音频数据事件
        /// </summary>
        public event Action<AudioFrame> Frame
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
        /// 添加监听器到C++
        /// </summary>
        private void AddSink()
        {
            if (sink != null) return;
            sink = Create<WebrtcObject>(AudioTrack_AddSink(Handler, DataReadyCallback));
        }
        /// <summary>
        /// 从C++移除监听器
        /// </summary>
        private void RemoveSink()
        {
            if (sink == null) return;
            AudioTrack_RemoveSink(Handler, sink.Handler);
            sink = null;
        }
        /// <summary>
        /// 在销毁前移除监听器
        /// </summary>
        protected override void Delete()
        {
            RemoveSink();
            base.Delete();
        }
        /// <summary>
        /// 收到音频数据的回调的C++包装
        /// </summary>
        private WebrtcUnityResultCallback DataReadyCallback;
        /// <summary>
        /// 轨道类型
        /// </summary>
        public override string Kind => "audio";
        /// <summary>
        /// 收到音频数据的回调
        /// </summary>
        /// <param name="val">音频数据指针</param>
        private unsafe void OnDataReady(IntPtr val)
        {
            if (FramePrivate == null) return;
            void** ptrs = (void**)val.ToPointer();
            byte* data = (byte*)*ptrs;
            ++ptrs;
            int bits_per_sample = *(int*)*ptrs;
            ++ptrs;
            int sample_rate = *(int*)*ptrs;
            ++ptrs;
            int number_of_channels = *(int*)*ptrs;
            ++ptrs;
            int number_of_frames = *(int*)*ptrs;
            ++ptrs;
            FramePrivate.Invoke(new AudioFrame
            {
                Data = new IntPtr(data),
                BitsPerSample = bits_per_sample,
                Channels = number_of_channels,
                Frames = number_of_frames,
                SampleRate = sample_rate
            });
        }
        /// <summary>
        /// C++ API：添加音频监听器
        /// </summary>
        /// <param name="ptr">音频轨道指针</param>
        /// <param name="onDataReady">回调函数</param>
        /// <returns>新建的监听器指针</returns>
        [DllImport(UnityPluginDll)] internal static extern IntPtr AudioTrack_AddSink(IntPtr ptr, WebrtcUnityResultCallback onDataReady);
        /// <summary>
        /// C++ API：移除音频监听器
        /// </summary>
        /// <param name="ptr">音频轨道指针</param>
        /// <param name="sink">监听器指针</param>
        [DllImport(UnityPluginDll)] internal static extern void AudioTrack_RemoveSink(IntPtr ptr, IntPtr sink);
    }
}