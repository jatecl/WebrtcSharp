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
        public int Width { get; set; }
        /// <summary>
        /// 分辨率高度
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// 帧率
        /// </summary>
        public int Fps { get; set; }
        /// <summary>
        /// 视频类型
        /// </summary>
        public VideoType VideoType { get; set; }
        /// <summary>
        /// Interlaced
        /// </summary>
        public bool Interlaced { get; set; }
    }
}