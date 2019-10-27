using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relywisdom
{
    /// <summary>
    /// 要求对方发送的媒体
    /// </summary>
    public class MediaQuery
    {
        /// <summary>
        /// 要求发送音频
        /// </summary>
        public bool Audio { get; set; }
        /// <summary>
        /// 要求发送视频
        /// </summary>
        public bool Video { get; set; }
    }
}
