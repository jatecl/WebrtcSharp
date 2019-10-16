using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WebrtcSharp
{
    /// <summary>
    /// P2P连接参数
    /// </summary>
    public class RTCConfiguration : WebrtcObject
    {
        /// <summary>
        /// 创建P2P连接参数
        /// </summary>
        /// <param name="enable_rtp_data_channel">enable rtp data channel</param>
        /// <param name="enable_dtls_srtp">enable dtls srtp</param>
        public RTCConfiguration(bool enable_rtp_data_channel = true, bool enable_dtls_srtp = true)
            : base(RTCConfiguration_new(enable_rtp_data_channel, enable_dtls_srtp))
        {

        }
        /// <summary>
        /// 添加Ice服务器信息
        /// </summary>
        /// <param name="turn_urls">服务器uri</param>
        /// <param name="username">用户名</param>
        /// <param name="credential">密码</param>
        public void AddServer(string[] turn_urls, string username = null, string credential = null)
        {
            RTCConfiguration_AddServer(Handler, turn_urls, turn_urls.Length, username, credential);
        }
        /// <summary>
        /// 添加Ice服务器信息
        /// </summary>
        /// <param name="turn_urls">服务器uri</param>
        /// <param name="username">用户名</param>
        /// <param name="credential">密码</param>
        public void AddServer(string turn_url, string username = null, string credential = null)
        {
            RTCConfiguration_AddServer(Handler, new[] { turn_url }, 1, username, credential);
        }
        /// <summary>
        /// C++ API：创建P2P连接参数
        /// </summary>
        /// <param name="enable_rtp_data_channel">enable rtp data channel</param>
        /// <param name="enable_dtls_srtp">enable dtls srtp</param>
        /// <returns>P2P连接参数指针</returns>
        [DllImport(UnityPluginDll)]
        internal static extern IntPtr RTCConfiguration_new(bool enable_rtp_data_channel, bool enable_dtls_srtp);
        /// <summary>
        /// C++ API：添加Ice服务器信息
        /// </summary>
        /// <param name="pointer">P2P连接参数指针</param>
        /// <param name="turn_urls">服务器uri</param>
        /// <param name="no_of_urls">服务器uri数量</param>
        /// <param name="username">用户名</param>
        /// <param name="credential">密码</param>
        [DllImport(UnityPluginDll)]
        internal static extern void RTCConfiguration_AddServer(IntPtr pointer, string[] turn_urls, int no_of_urls, string username, string credential);
    }
}
