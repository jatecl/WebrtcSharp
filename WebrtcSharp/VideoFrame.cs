using System;

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
}