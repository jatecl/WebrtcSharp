using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebrtcSharp
{
    /// <summary>
    /// 视频源
    /// </summary>
    public class VideoSource : WebrtcObject
    {
        /// <summary>
        /// 持有一个视频源
        /// </summary>
        /// <param name="handler">视频源指针</param>
        public VideoSource(IntPtr handler) : base(handler) { }
    }
}
