using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebrtcSharp
{
    /// <summary>
    /// IceCandidate
    /// </summary>
    public class IceCandidate
    {
        public IceCandidate(string sdpMid, int sdpIndex, string sdp)
        {
            Sdp = sdp;
            SdpMLineIndex = sdpIndex;
            SdpMid = sdpMid;
        }
        /// <summary>
        /// sdp
        /// </summary>
        public string Sdp { get; set; }
        /// <summary>
        /// SdpMid
        /// </summary>
        public string SdpMid { get; set; }
        /// <summary>
        /// sdpMineIndex
        /// </summary>
        public int SdpMLineIndex { get; set; }
        /// <summary>
        /// ServerUrl
        /// </summary>
        public string ServerUrl { get; set; } = "";
    }
}
