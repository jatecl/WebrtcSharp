using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WebrtcSharp
{
    /// <summary>
    /// 自由写入视频帧的视频源
    /// </summary>
    public class FrameVideoSource : VideoSource
    {
        /// <summary>
        /// 创建一个自由写入帧的视频源
        /// </summary>
        public FrameVideoSource(IDispatcher dispatcher) : base(FrameVideoSource_new(), dispatcher) { }
        /// <summary>
        /// 发送视频帧
        /// </summary>
        /// <param name="frame">视频帧</param>
        public void SendFrame(VideoFrame frame)
        {
            FrameVideoSource_SendFrame(Handler, frame.DataY, frame.DataU, frame.DataV, frame.DataA, frame.StrideY, frame.StrideU, frame.StrideV, frame.StrideA, frame.Width, frame.Height, frame.Rotation, frame.Timestamp);
        }
        /// <summary>
        /// C++ API：创建一个自由写入帧的视频源
        /// </summary>
        /// <returns>自由写入帧的视频源的指针</returns>
        [DllImport(UnityPluginDll)] internal static extern IntPtr FrameVideoSource_new();
        /// <summary>
        /// 发送视频帧
        /// </summary>
        /// <param name="ptr">自由写入帧的视频源的指针</param>
        /// <param name="data_y">YUVA420p Y指针</param>
        /// <param name="data_u">YUVA420p U指针</param>
        /// <param name="data_v">YUVA420p V指针</param>
        /// <param name="data_a">YUVA420p A指针</param>
        /// <param name="stride_y">YUVA420p Y宽度</param>
        /// <param name="stride_u">YUVA420p U宽度</param>
        /// <param name="stride_v">YUVA420p V宽度</param>
        /// <param name="stride_a">YUVA420p A宽度</param>
        /// <param name="width">图片宽度</param>
        /// <param name="height">图片高度</param>
        /// <param name="rotation">旋转角度</param>
        /// <param name="timestamp">时间戳</param>
        [DllImport(UnityPluginDll)]
        internal static extern void FrameVideoSource_SendFrame(IntPtr ptr,
            IntPtr data_y,
            IntPtr data_u,
            IntPtr data_v,
            IntPtr data_a,
            int stride_y,
            int stride_u,
            int stride_v,
            int stride_a,
            int width,
            int height,
            int rotation,
            long timestamp);
    }
}
