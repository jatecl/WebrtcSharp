namespace WebrtcSharp
{
    /// <summary>
    /// 视频设备描述
    /// </summary>
    public class VideoDeviceInfo
    {
        /// <summary>
        /// 设备名称
        /// </summary>
        public string DeviceName { get; internal set; }
        /// <summary>
        /// 设备Id
        /// </summary>
        public string DeviceId { get; internal set; }
        /// <summary>
        /// 设备型号
        /// </summary>
        public string ProductId { get; internal set; }
        /// <summary>
        /// 是否可用
        /// </summary>
        public int Enable { get; internal set; }
        /// <summary>
        /// 设备顺序
        /// </summary>
        public int Index { get; internal set; }
    }
}