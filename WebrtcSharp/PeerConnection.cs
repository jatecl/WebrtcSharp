using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WebrtcSharp
{
    /// <summary>
    /// Signaling State
    /// </summary>
    public enum SignalingState
    {
        Stable,
        HaveLocalOffer,
        HaveLocalPrAnswer,
        HaveRemoteOffer,
        HaveRemotePrAnswer,
        Closed,
    }

    /// <summary>
    /// Ice Gathering State
    /// See https://w3c.github.io/webrtc-pc/#dom-rtcicegatheringstate
    /// </summary>
    public enum IceGatheringState
    {
        IceGatheringNew,
        IceGatheringGathering,
        IceGatheringComplete
    }

    /// <summary>
    /// Peer Connection State
    /// See https://w3c.github.io/webrtc-pc/#dom-rtcpeerconnectionstate
    /// </summary>
    public enum PeerConnectionState
    {
        New,
        Connecting,
        Connected,
        Disconnected,
        Failed,
        Closed,
    }

    /// <summary>
    /// Ice Connection State
    /// See https://w3c.github.io/webrtc-pc/#dom-rtciceconnectionstate
    /// </summary>
    public enum IceConnectionState
    {
        New,
        Checking,
        Connected,
        Completed,
        Failed,
        Disconnected,
        Closed,
        Max,
    }
    /// <summary>
    /// P2P连接
    /// </summary>
    public class PeerConnection : WebrtcObject
    {
        public IDispatcher Dispatcher { get; }

        private PeerConnectionObserver observer;

        /// <summary>
        /// 持有一个P2P连接指针
        /// </summary>
        /// <param name="handler">P2P连接指针</param>
        internal PeerConnection(IntPtr handler, IDispatcher dispatcher, PeerConnectionObserver observer) : base(handler)
        {
            this.Dispatcher = dispatcher;
            this.observer = observer;
            observer.Connection = this;
        }
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
            var sender = UniqueNative<RtpSender>(handler);
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
            if (ptrList == IntPtr.Zero) return new RtpSender[0];
            var list = new PointerArray(ptrList);
            var items = list.GetBuffer();

            var ret = new List<RtpSender>();
            while (*items != null)
            {
                IntPtr senderPtr = new IntPtr(*items);
                var sender = UniqueNative<RtpSender>(senderPtr);
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
            if (channelPtr == IntPtr.Zero) return null;
            return new RTCDataChannel(channelPtr);
        }
        /// <summary>
        /// SignalingChange 事件
        /// </summary>
        public event Action<SignalingState> SignalingChange;
        /// <summary>
        /// DataChannel 事件
        /// </summary>
        public event Action<RTCDataChannel> DataChannel;
        /// <summary>
        /// RenegotiationNeeded 事件
        /// </summary>
        public event Action RenegotiationNeeded;
        /// <summary>
        /// IceConnectionChange 事件
        /// </summary>
        public event Action<IceConnectionState> IceConnectionChange;
        /// <summary>
        /// StandardizedIceConnectionChange 事件
        /// </summary>
        public event Action<IceConnectionState> StandardizedIceConnectionChange;
        /// <summary>
        /// ConnectionChange 事件
        /// </summary>
        public event Action<PeerConnectionState> ConnectionChange;
        /// <summary>
        /// IceGatheringChange 事件
        /// </summary>
        public event Action<IceGatheringState> IceGatheringChange;
        /// <summary>
        /// IceCandidate 事件
        /// </summary>
        public event Action<IceCandidate> IceCandidate;
        /// <summary>
        /// IceCandidatesRemoved 事件
        /// </summary>
        public event Action<IceCandidate[]> IceCandidatesRemoved;
        /// <summary>
        /// IceConnectionReceivingChange 事件
        /// </summary>
        public event Action<bool> IceConnectionReceivingChange;
        /// <summary>
        /// AddTrack 事件
        /// </summary>
        public event Action<RtpReceiver> TrackAdded;
        /// <summary>
        /// RemoveTrack 事件
        /// </summary>
        public event Action<RtpReceiver> TrackRemoved;

        /// <summary>
        /// P2P连接事件
        /// </summary>
        internal class PeerConnectionObserver : WebrtcObject
        {
            /// <summary>
            /// 创建一个P2P连接事件
            /// </summary>
            public PeerConnectionObserver() : base(default)
            {
                NativeSignalingChange = val => Connection?.SignalingChange?.Invoke((SignalingState)val);
                NativeDataChannel = val => OnDataChannel(val);
                NativeRenegotiationNeeded = () => Connection?.RenegotiationNeeded?.Invoke();
                NativeIceConnectionChange = val => Connection?.IceConnectionChange?.Invoke((IceConnectionState)val);
                NativeStandardizedIceConnectionChange = val => Connection?.StandardizedIceConnectionChange?.Invoke((IceConnectionState)val);
                NativeConnectionChange = val => Connection?.ConnectionChange?.Invoke((PeerConnectionState)val);
                NativeIceGatheringChange = val => Connection?.IceGatheringChange?.Invoke((IceGatheringState)val);
                NativeIceCandidate = val => OnIceCandidate(val);
                NativeIceCandidatesRemoved = val => OnIceCandidatesRemoved(val);
                NativeIceConnectionReceivingChange = val => Connection?.IceConnectionReceivingChange?.Invoke(val > 0);
                NativeAddTrack = val => OnAddTrack(val);
                NativeRemoveTrack = val => OnRemoveTrack(val);
                NativeInterestingUsage = val => Connection?.InterestingUsage?.Invoke(val);

                Handler = PeerConnectionObserver_new(NativeSignalingChange,
                    NativeDataChannel,
                    NativeRenegotiationNeeded,
                    NativeIceConnectionChange,
                    NativeStandardizedIceConnectionChange,
                    NativeConnectionChange,
                    NativeIceGatheringChange,
                    NativeIceCandidate,
                    NativeIceCandidatesRemoved,
                    NativeIceConnectionReceivingChange,
                    NativeAddTrack,
                    NativeRemoveTrack,
                    NativeInterestingUsage
                    );
            }
            /// <summary>
            /// 转发OnDataChannel
            /// </summary>
            /// <param name="channel">数据通道指针</param>
            private void OnDataChannel(IntPtr channel)
            {
                if (channel == IntPtr.Zero) return;
                var ch = new RTCDataChannel(channel);
                Connection?.DataChannel?.Invoke(ch);
            }
            /// <summary>
            /// 转发OnIceCandidate
            /// </summary>
            /// <param name="candidate">IceCandidate</param>
            private unsafe void OnIceCandidate(IntPtr candidate)
            {
                if (Connection?.IceCandidate != null)
                {
                    void** ptrs = (void**)candidate.ToPointer();
                    var sdp = new string((sbyte*)*ptrs);
                    var sdp_index = *(int*)*(ptrs + 1);
                    var sdp_mid = new string((sbyte*)*(ptrs + 2));
                    Connection?.IceCandidate(new IceCandidate(sdp_mid, sdp_index, sdp));
                }
            }
            /// <summary>
            /// 转发OnIceCandidatesRemoved
            /// </summary>
            /// <param name="candidates">IceCandidate List</param>
            private unsafe void OnIceCandidatesRemoved(IntPtr candidates)
            {
                if (Connection?.IceCandidatesRemoved != null)
                {
                    var list = new List<IceCandidate>();
                    var ptrs = (void**)candidates.ToPointer();
                    while (*ptrs != null)
                    {
                        var sdp_mid = new string((sbyte*)*ptrs);
                        ++ptrs;
                        var sdp = new string((sbyte*)*ptrs);
                        ++ptrs;
                        list.Add(new IceCandidate(sdp_mid, -1, sdp));
                    }
                    Connection?.IceCandidatesRemoved(list.ToArray());
                }
            }
            /// <summary>
            /// 接收器缓存
            /// </summary>
            private readonly Dictionary<long, RtpReceiver> trackMap = new Dictionary<long, RtpReceiver>();
            /// <summary>
            /// 映射接收器
            /// </summary>
            /// <param name="ptr">接收器指针</param>
            /// <param name="always">是否创建</param>
            /// <returns>接收器</returns>
            private RtpReceiver GetOrCreateTrack(IntPtr ptr, bool always)
            {
                if (ptr == IntPtr.Zero) return null;
                lock (trackMap)
                {
                    var addr = ptr.ToInt64();
                    if (!trackMap.ContainsKey(addr))
                    {
                        if (!always) return null;
                        var track = new RtpReceiver(ptr, Connection.Dispatcher);
                        trackMap.Add(addr, track);
                        return track;
                    }
                    return trackMap[addr];
                }
            }
            /// <summary>
            /// 转发OnAddTrack
            /// </summary>
            /// <param name="track">接收器指针</param>
            private void OnAddTrack(IntPtr track)
            {
                if (Connection?.TrackAdded != null)
                {
                    var trackObj = GetOrCreateTrack(track, true);
                    Connection?.TrackAdded(trackObj);
                }
            }
            /// <summary>
            /// 转发OnRemoveTrack
            /// </summary>
            /// <param name="track">接收器指针</param>
            private void OnRemoveTrack(IntPtr track)
            {
                if (Connection?.TrackRemoved != null)
                {
                    var trackObj = GetOrCreateTrack(track, false);
                    if (trackObj != null) Connection?.TrackRemoved(trackObj);
                }
            }
            public PeerConnection Connection { get; set; }
            /// <summary>
            /// C++持有的 SignalingChange 回调
            /// </summary>
            private WebrtcUnityStateCallback NativeSignalingChange;
            /// <summary>
            /// C++持有的 DataChannel 回调
            /// </summary>
            private WebrtcUnityResultCallback NativeDataChannel;
            /// <summary>
            /// C++持有的 RenegotiationNeeded 回调
            /// </summary>
            private WebrtcUnityCallback NativeRenegotiationNeeded;
            /// <summary>
            /// C++持有的 IceConnectionChange 回调
            /// </summary>
            private WebrtcUnityStateCallback NativeIceConnectionChange;
            /// <summary>
            /// C++持有的 StandardizedIceConnectionChange 回调
            /// </summary>
            private WebrtcUnityStateCallback NativeStandardizedIceConnectionChange;
            /// <summary>
            /// C++持有的 ConnectionChange 回调
            /// </summary>
            private WebrtcUnityStateCallback NativeConnectionChange;
            /// <summary>
            /// C++持有的 IceGatheringChange 回调
            /// </summary>
            private WebrtcUnityStateCallback NativeIceGatheringChange;
            /// <summary>
            /// C++持有的 IceCandidate 回调
            /// </summary>
            private WebrtcUnityResultCallback NativeIceCandidate;
            /// <summary>
            /// C++持有的 IceCandidatesRemoved 回调
            /// </summary>
            private WebrtcUnityResultCallback NativeIceCandidatesRemoved;
            /// <summary>
            /// C++持有的 IceConnectionReceivingChange 回调
            /// </summary>
            private WebrtcUnityStateCallback NativeIceConnectionReceivingChange;
            /// <summary>
            /// C++持有的 AddTrack 回调
            /// </summary>
            private WebrtcUnityResultCallback NativeAddTrack;
            /// <summary>
            /// C++持有的 RemoveTrack 回调
            /// </summary>
            private WebrtcUnityResultCallback NativeRemoveTrack;
            /// <summary>
            /// C++持有的 InterestingUsage 回调
            /// </summary>
            private WebrtcUnityStateCallback NativeInterestingUsage;
            /// <summary>
            /// C++ API：创建一个P2P连接的事件回调
            /// </summary>
            /// <param name="SignalingChange">SignalingChange 回调</param>
            /// <param name="DataChannel">DataChannel 回调</param>
            /// <param name="RenegotiationNeeded">RenegotiationNeeded 回调</param>
            /// <param name="IceConnectionChange">IceConnectionChange 回调</param>
            /// <param name="StandardizedIceConnectionChange">StandardizedIceConnectionChange 回调</param>
            /// <param name="ConnectionChange">ConnectionChange 回调</param>
            /// <param name="IceGatheringChange">IceGatheringChange 回调</param>
            /// <param name="IceCandidate">IceCandidate 回调</param>
            /// <param name="IceCandidatesRemoved">IceCandidatesRemoved 回调</param>
            /// <param name="IceConnectionReceivingChange">IceConnectionReceivingChange 回调</param>
            /// <param name="AddTrack">AddTrack 回调</param>
            /// <param name="RemoveTrack">RemoveTrack 回调</param>
            /// <param name="InterestingUsage">InterestingUsage 回调</param>
            /// <returns>P2P连接的事件回调的指针</returns>
            [DllImport(UnityPluginDll)]
            internal static extern IntPtr PeerConnectionObserver_new(WebrtcUnityStateCallback SignalingChange,
                WebrtcUnityResultCallback DataChannel,
                WebrtcUnityCallback RenegotiationNeeded,
                WebrtcUnityStateCallback IceConnectionChange,
                WebrtcUnityStateCallback StandardizedIceConnectionChange,
                WebrtcUnityStateCallback ConnectionChange,
                WebrtcUnityStateCallback IceGatheringChange,
                WebrtcUnityResultCallback IceCandidate,
                WebrtcUnityResultCallback IceCandidatesRemoved,
                WebrtcUnityStateCallback IceConnectionReceivingChange,
                WebrtcUnityResultCallback AddTrack,
                WebrtcUnityResultCallback RemoveTrack,
                WebrtcUnityStateCallback InterestingUsage);
        }
        /// <summary>
        /// InterestingUsage 事件
        /// </summary>
        public event Action<int> InterestingUsage;
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
