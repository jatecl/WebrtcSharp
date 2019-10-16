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
    /// P2P连接事件
    /// </summary>
    public class PeerConnectionObserve : WebrtcObject
    {
        /// <summary>
        /// 创建一个P2P连接事件
        /// </summary>
        public PeerConnectionObserve() : base(default)
        {
            NativeSignalingChange = val => SignalingChange?.Invoke((SignalingState)val);
            NativeDataChannel = val => OnDataChannel(val);
            NativeRenegotiationNeeded = () => RenegotiationNeeded?.Invoke();
            NativeIceConnectionChange = val => IceConnectionChange?.Invoke((IceConnectionState)val);
            NativeStandardizedIceConnectionChange = val => StandardizedIceConnectionChange?.Invoke((IceConnectionState)val);
            NativeConnectionChange = val => ConnectionChange?.Invoke((PeerConnectionState)val);
            NativeIceGatheringChange = val => IceGatheringChange?.Invoke((IceGatheringState)val);
            NativeIceCandidate = val => OnIceCandidate(val);
            NativeIceCandidatesRemoved = val => OnIceCandidatesRemoved(val);
            NativeIceConnectionReceivingChange = val => IceConnectionReceivingChange?.Invoke(val > 0);
            NativeAddTrack = val => OnAddTrack(val);
            NativeRemoveTrack = val => OnRemoveTrack(val);
            NativeInterestingUsage = val => InterestingUsage?.Invoke(val);

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
        /// 转发OnDataChannel
        /// </summary>
        /// <param name="channel">数据通道指针</param>
        private void OnDataChannel(IntPtr channel)
        {
            DataChannel?.Invoke(channel);
        }
        /// <summary>
        /// 转发OnIceCandidate
        /// </summary>
        /// <param name="candidate">IceCandidate</param>
        private unsafe void OnIceCandidate(IntPtr candidate)
        {
            if (IceCandidate != null)
            {
                void** ptrs = (void**)candidate.ToPointer();
                var sdp = new string((sbyte*)*ptrs);
                var sdp_index = *(int*)*(ptrs + 1);
                var sdp_mid = new string((sbyte*)*(ptrs + 2));
                IceCandidate(new IceCandidate(sdp_mid, sdp_index, sdp));
            }
        }
        /// <summary>
        /// 转发OnIceCandidatesRemoved
        /// </summary>
        /// <param name="candidates">IceCandidate List</param>
        private unsafe void OnIceCandidatesRemoved(IntPtr candidates)
        {
            if (IceCandidatesRemoved != null)
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
                IceCandidatesRemoved(list.ToArray());
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
            lock (trackMap)
            {
                var addr = ptr.ToInt64();
                if (!trackMap.ContainsKey(addr))
                {
                    if (!always) return null;
                    var track = Create<RtpReceiver>(ptr);
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
            if (AddTrack != null)
            {
                var trackObj = GetOrCreateTrack(track, true);
                AddTrack(trackObj);
            }
        }
        /// <summary>
        /// 转发OnRemoveTrack
        /// </summary>
        /// <param name="track">接收器指针</param>
        private void OnRemoveTrack(IntPtr track)
        {
            if (RemoveTrack != null)
            {
                var trackObj = GetOrCreateTrack(track, false);
                if (trackObj != null) RemoveTrack(trackObj);
            }
        }
        /// <summary>
        /// SignalingChange 事件
        /// </summary>
        public event Action<SignalingState> SignalingChange;
        /// <summary>
        /// DataChannel 事件
        /// </summary>
        public event WebrtcUnityResultCallback DataChannel;
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
        public event Action<RtpReceiver> AddTrack;
        /// <summary>
        /// RemoveTrack 事件
        /// </summary>
        public event Action<RtpReceiver> RemoveTrack;
        /// <summary>
        /// InterestingUsage 事件
        /// </summary>
        public event Action<int> InterestingUsage;
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
}
