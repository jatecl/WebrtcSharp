namespace WebrtcSharp
{
    /// <summary>
    /// 视频设备特征描述
    /// </summary>
    public class VideoDeviceCapabilities
    {
        /// <summary>
        /// 分辨率宽度
        /// </summary>
        public int Width { get; internal set; }
        /// <summary>
        /// 分辨率高度
        /// </summary>
        public int Height { get; internal set; }
        /// <summary>
        /// 帧率
        /// </summary>
        public int Fps { get; internal set; }
        /// <summary>
        /// 视频类型
        /// </summary>
        public VideoType VideoType { get; internal set; }
        /// <summary>
        /// Interlaced
        /// </summary>
        public bool Interlaced { get; internal set; }
    }
}