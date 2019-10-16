using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebrtcSharp
{
    /// <summary>
    /// 音频源
    /// </summary>
    public class AudioSource : WebrtcObject
    {
        /// <summary>
        /// 持有一个音频源
        /// </summary>
        /// <param name="handler">音频源指针</param>
        public AudioSource(IntPtr handler) : base(handler) { }
    }
}
