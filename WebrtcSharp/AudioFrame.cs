using System;

namespace WebrtcSharp
{
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
}