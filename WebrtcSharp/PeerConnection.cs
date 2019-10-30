using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WebrtcSharp
{
    /// <summary>
    /// P2P连接
    /// </summary>
    public class PeerConnection : WebrtcObject
    {
        /// <summary>
        /// 持有一个P2P连接指针
        /// </summary>
        /// <param name="handler">P2P连接指针</param>
        public PeerConnection(IntPtr handler) : base(handler) { }
        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            PeerConnection_Close(Handler);
        }
        /// <summary>
        /// 创建Offer
        /// </summary>
        /// <param name="ice_restart">ice restart</param>
        /// <param name="voice_activity_detection">voice activity detection</param>
        /// <param name="use_rtp_mux">use rtp mux</param>
        /// <returns>offer 字符串</returns>
        public async Task<string> CreateOffer(
            bool ice_restart = false,
            bool voice_activity_detection = true,
            bool use_rtp_mux = true)
        {
            var holder = new UnmanageHolder();
            var offer = await Promise<string>.Await((cs, ce) =>
            {
                WebrtcUnityResult2Callback done = (type, sdp) =>
                {
                    unsafe
                    {
                        //var strType = new string((sbyte*)type.ToPointer());
                        var strSdp = new string((sbyte*)sdp.ToPointer());
                        cs(strSdp);
                    }
                };
                WebrtcUnityResultCallback error = msg =>
                {
                    unsafe
                    {
                        ce(new Exception(new string((sbyte*)msg.ToPointer())));
                    }
                };
                holder.Hold(done, error);
                PeerConnection_CreateOffer(Handler,
                    done,
                    error,
                    true,
                    true,
                    voice_activity_detection,
                    ice_restart,
                    use_rtp_mux);

            });
            holder.StillHere();
            return offer;
        }
        /// <summary>
        /// 创建Answer
        /// </summary>
        /// <param name="ice_restart">ice restart</param>
        /// <param name="voice_activity_detection">voice activity detection</param>
        /// <param name="use_rtp_mux">use rtp mux</param>
        /// <returns>Answer 字符串</returns>
        public async Task<string> CreateAnswer(
            bool ice_restart = false,
            bool voice_activity_detection = true,
            bool use_rtp_mux = true)
        {
            var holder = new UnmanageHolder();
            var answer = await Promise<string>.Await((cs, ce) =>
            {
                WebrtcUnityResult2Callback done = (type, sdp) =>
                {
                    unsafe
                    {
                        //var strType = new string((sbyte*)type.ToPointer());
                        var strSdp = new string((sbyte*)sdp.ToPointer());
                        cs(strSdp);
                    }
                };
                WebrtcUnityResultCallback error = msg =>
                {
                    unsafe
                    {
                        ce(new Exception(new string((sbyte*)msg.ToPointer())));
                    }
                };
                holder.Hold(done, error);
                PeerConnection_CreateAnswer(Handler,
                    done,
                    error,
                    true,
                    true,
                    voice_activity_detection,
                    ice_restart,
                    use_rtp_mux);

            });
            holder.StillHere();
            return answer;
        }
        /// <summary>
        /// 设置远程描述
        /// </summary>
        /// <param name="type">描述类型</param>
        /// <param name="sdp">描述</param>
        /// <returns></returns>
        public async Task SetRemoteDescription(string type, string sdp)
        {
            var holder = new UnmanageHolder();
            await Promise.Await((cs, ce) =>
            {
                WebrtcUnityCallback done = () => cs();
                WebrtcUnityResultCallback error = msg =>
                {
                    unsafe
                    {
                        ce(new Exception(new string((sbyte*)msg.ToPointer())));
                    }
                };
                holder.Hold(done, error);
                PeerConnection_SetRemoteDescription(Handler,
                    type,
                    sdp,
                    done,
                    error);

            });
            holder.StillHere();
        }
        /// <summary>
        /// 设置本地描述
        /// </summary>
        /// <param name="type">描述类型</param>
        /// <param name="sdp">描述</param>
        /// <returns></returns>
        public async Task SetLocalDescription(string type, string sdp)
        {
            var holder = new UnmanageHolder();
            await Promise.Await((cs, ce) =>
            {
                WebrtcUnityCallback done = () => cs();
                WebrtcUnityResultCallback error = msg =>
                {
                    unsafe
                    {
                        ce(new Exception(new string((sbyte*)msg.ToPointer())));
                    }
                };
                holder.Hold(done, error);
                PeerConnection_SetLocalDescription(Handler,
                    type,
                    sdp,
                    done,
                    error);

            });
            holder.StillHere();
        }
        /// <summary>
        /// 添加对方发过来的IceCandidate
        /// </summary>
        /// <param name="candidate">IceCandidate</param>
        /// <returns>是否添加成功</returns>
        public bool AddIceCandidate(IceCandidate candidate)
        {
            return PeerConnection_AddIceCandidate(Handler, candidate.Sdp, candidate.SdpMLineIndex, candidate.SdpMid);
        }
        /// <summary>
        /// 添加媒体轨道
        /// </summary>
        /// <param name="track">媒体轨道</param>
        /// <param name="labels">轨道标识</param>
        /// <returns>发送当前媒体轨道的发送器</returns>
        public RtpSender AddTrack(MediaStreamTrack track, string[] labels)
        {
            var handler = PeerConnection_AddTrack(Handler, track.Handler, labels, labels.Length);
            var sender = Create<RtpSender>(handler, true);
            if (sender != null) sender.Track = track;
            return sender;
        }
        /// <summary>
        /// 获取所有发送器
        /// </summary>
        /// <returns></returns>
        public unsafe RtpSender[] GetSenders()
        {
            var ptrList = PeerConnection_GetSenders(this.Handler);
            var list = Create<PointerArray>(ptrList);
            var items = list.GetBuffer();

            var ret = new List<RtpSender>();
            while (*items != null)
            {
                IntPtr senderPtr = new IntPtr(*items);
                var sender = Create<RtpSender>(senderPtr, true);
                if (sender != null) ret.Add(sender);
                ++items;
            }
            return ret.ToArray();
        }
        /// <summary>
        /// 移除媒体轨道
        /// </summary>
        /// <param name="sender">轨道所在的发送器</param>
        public void RemoveTrack(RtpSender sender)
        {
            var error = PeerConnection_RemoveTrack(Handler, sender.Handler);
            if (error != null) throw new Exception(error);
        }
        /// <summary>
        /// 添加数据通道
        /// </summary>
        /// <param name="label">数据通道</param>
        /// <param name="options">选项</param>
        /// <returns>数据通道</returns>
        public RTCDataChannel CreateDataChannel(string label, RTCDataChannelOptions options)
        {
            if (options == null) options = new RTCDataChannelOptions();
            var channelPtr = PeerConnection_CreateDataChannel(Handler, label,
                options.Reliable,
                options.Ordered,
                options.MaxRetransmitTime ?? -1,
                options.MaxRetransmits ?? -1,
                options.Protocol,
                options.Negotiated,
                options.Id
                );
            return Create<RTCDataChannel>(channelPtr);
        }
        /// <summary>
        /// C++ API：创建数据通道
        /// </summary>
        /// <param name="ptr">p2p连接指针</param>
        /// <param name="label">通道标识</param>
        /// <param name="reliable">reliable</param>
        /// <param name="ordered">ordered</param>
        /// <param name="maxRetransmitTime">max retransmit time</param>
        /// <param name="maxRetransmits">max retransmits</param>
        /// <param name="protocol">protocol</param>
        /// <param name="negotiated">negotiated</param>
        /// <param name="id">id</param>
        /// <returns>数据通道指针</returns>
        [DllImport(UnityPluginDll)]
        internal static extern IntPtr PeerConnection_CreateDataChannel(IntPtr ptr,
            string label,
            bool reliable,
            bool ordered,
            int maxRetransmitTime,
            int maxRetransmits,
            string protocol,
            bool negotiated,
            int id);
        /// <summary>
        /// C++ API：关闭p2p连接
        /// </summary>
        /// <param name="ptr">p2p连接指针</param>
        [DllImport(UnityPluginDll)] internal static extern void PeerConnection_Close(IntPtr ptr);
        /// <summary>
        /// C++ API：创建offer
        /// </summary>
        /// <param name="ptr">p2p连接指针</param>
        /// <param name="success">创建成功时的回调</param>
        /// <param name="failure">创建失败时的回调</param>
        /// <param name="offer_to_receive_video">offer to receive video</param>
        /// <param name="offer_to_receive_audio">offer to receive audio</param>
        /// <param name="voice_activity_detection">voice activity detection</param>
        /// <param name="ice_restart">ice restart</param>
        /// <param name="use_rtp_mux">use rtp mux</param>
        [DllImport(UnityPluginDll)]
        internal static extern void PeerConnection_CreateOffer(IntPtr ptr,
            WebrtcUnityResult2Callback success,
            WebrtcUnityResultCallback failure,
            bool offer_to_receive_video,
            bool offer_to_receive_audio,
            bool voice_activity_detection,
            bool ice_restart,
            bool use_rtp_mux);
        /// <summary>
        /// C++ API：创建answer
        /// </summary>
        /// <param name="ptr">p2p连接指针</param>
        /// <param name="success">创建成功时的回调</param>
        /// <param name="failure">创建失败时的回调</param>
        /// <param name="offer_to_receive_video">offer to receive video</param>
        /// <param name="offer_to_receive_audio">offer to receive audio</param>
        /// <param name="voice_activity_detection">voice activity detection</param>
        /// <param name="ice_restart">ice restart</param>
        /// <param name="use_rtp_mux">use rtp mux</param>
        [DllImport(UnityPluginDll)]
        internal static extern void PeerConnection_CreateAnswer(IntPtr ptr,
            WebrtcUnityResult2Callback success,
            WebrtcUnityResultCallback failure,
            bool offer_to_receive_video,
            bool offer_to_receive_audio,
            bool voice_activity_detection,
            bool ice_restart,
            bool use_rtp_mux);
        /// <summary>
        /// C++ API：设置远程描述
        /// </summary>
        /// <param name="ptr">p2p连接指针</param>
        /// <param name="type">描述类型</param>
        /// <param name="sdp">描述</param>
        /// <param name="success">设置成功时的回调</param>
        /// <param name="failure">设置失败时的回调</param>
        [DllImport(UnityPluginDll)]
        internal static extern void PeerConnection_SetRemoteDescription(IntPtr ptr,
            string type,
            string sdp,
            WebrtcUnityCallback success,
            WebrtcUnityResultCallback failure);
        /// <summary>
        /// C++ API：设置本地描述
        /// </summary>
        /// <param name="ptr">p2p连接指针</param>
        /// <param name="type">描述类型</param>
        /// <param name="sdp">描述</param>
        /// <param name="success">设置成功时的回调</param>
        /// <param name="failure">设置失败时的回调</param>
        [DllImport(UnityPluginDll)]
        internal static extern void PeerConnection_SetLocalDescription(IntPtr ptr,
            string type,
            string sdp,
            WebrtcUnityCallback success,
            WebrtcUnityResultCallback failure);
        /// <summary>
        /// C++ API：添加IceCandidate
        /// </summary>
        /// <param name="ptr">p2p连接指针</param>
        /// <param name="candidate">sdp</param>
        /// <param name="sdp_mlineindex">sdp mlineindex</param>
        /// <param name="sdp_mid">sdp mid</param>
        /// <returns>是否添加成功</returns>
        [DllImport(UnityPluginDll)]
        internal static extern bool PeerConnection_AddIceCandidate(IntPtr ptr,
            string candidate,
            int sdp_mlineindex,
            string sdp_mid);
        /// <summary>
        /// C++ API：添加媒体轨道
        /// </summary>
        /// <param name="ptr">p2p连接指针</param>
        /// <param name="track">媒体轨道</param>
        /// <param name="labels">轨道标识</param>
        /// <param name="len">轨道标识数量</param>
        /// <returns>发送媒体轨道的发送器指针</returns>
        [DllImport(UnityPluginDll)]
        internal static extern IntPtr PeerConnection_AddTrack(IntPtr ptr, IntPtr track, string[] labels, int len);
        /// <summary>
        /// C++ API：移除媒体轨道
        /// </summary>
        /// <param name="ptr">p2p连接指针</param>
        /// <param name="sender">发送媒体轨道的发送器指针</param>
        /// <returns>错误信息的字符串</returns>
        [DllImport(UnityPluginDll)]
        internal static extern string PeerConnection_RemoveTrack(IntPtr ptr, IntPtr sender);
        /// <summary>
        /// 获得所有发送器
        /// </summary>
        /// <param name="ptr">发送器列表指针</param>
        /// <returns></returns>
        [DllImport(UnityPluginDll)]
        internal static extern IntPtr PeerConnection_GetSenders(IntPtr ptr);
    }
}
