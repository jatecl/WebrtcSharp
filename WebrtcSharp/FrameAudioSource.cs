using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WebrtcSharp
{
    /// <summary>
    /// 自定义帧的音频源，可自由写入音频帧给对方
    /// </summary>
    public class FrameAudioSource : AudioSource
    {
        /// <summary>
        /// 创建一个自定义帧的音频源
        /// </summary>
        public FrameAudioSource() : base(FrameAudioSource_new()) { }
        /// <summary>
        /// 写入音频帧
        /// </summary>
        /// <param name="frame">音频帧数据</param>
        public void SendFrame(AudioFrame frame)
        {
            FrameAudioSource_SendFrame(Handler, frame.Data, frame.BitsPerSample, frame.SampleRate, frame.Channels, frame.Frames);
        }
        /// <summary>
        /// C++ API：创建一个自定义帧的音频源
        /// </summary>
        /// <returns>自定义帧的音频源的指针</returns>
        [DllImport(UnityPluginDll)] internal static extern IntPtr FrameAudioSource_new();
        /// <summary>
        /// 发送音频数据到音频源
        /// </summary>
        /// <param name="ptr">自定义帧的音频源的指针</param>
        /// <param name="audio_data">数据</param>
        /// <param name="bits_per_sample">bits per samplle</param>
        /// <param name="sample_rate">sample rate</param>
        /// <param name="number_of_channels">number of channels</param>
        /// <param name="number_of_frames">number of frames</param>
        [DllImport(UnityPluginDll)]
        internal static extern void FrameAudioSource_SendFrame(IntPtr ptr,
            IntPtr audio_data,
            int bits_per_sample,
            int sample_rate,
            int number_of_channels,
            int number_of_frames);
    }
}
