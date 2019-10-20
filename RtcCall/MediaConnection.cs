using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebrtcSharp;

namespace Relywisdom
{
    /**
     * P2P连接
     */
    class MediaConnection : EventEmitter
    {
        /**
         * 远程媒体
         */
        public RemoteMedia remote { get; }
        /**
         * 当前状态
         */
        private IConnectionState state;
        /**
         * 轨道缓存
         */
        private Dictionary<string, MediaStreamTrack> _tracks = new Dictionary<string, MediaStreamTrack>();
        /**
         * 超时检查
         */
        private Timeout _timer;
        /**
         * candidate缓存
         */
        private List<IceCandidate> _candidates = new List<IceCandidate>();
        /**
         * 连接
         */
        public RtcSocket socket
        {
            get
            {
                return this.remote.call.socket;
            }
        }
        /**
         * 本地媒体
         */
        public LocalMedia local
        {
            get
            {
                return this.remote.call.local;
            }
        }
        /**
         * 通话管理器
         */
        public RtcCall call
        {
            get
            {
                return this.remote.call;
            }
        }
        /// <summary>
        /// P2P连接
        /// </summary>
        private PeerConnection connection;
        /// <summary>
        /// P2P事件监听
        /// </summary>
        private PeerConnectionObserve observe;
        /// <summary>
        /// 所有的发送流
        /// </summary>
        private List<RtcSender> senders = new List<RtcSender>();
        /**
         * 创建一个P2P连接
         * @param {RemoteMedia}} remote 远程媒体
         */
        public MediaConnection(RemoteMedia remote)
        {
            this.remote = remote;
            var config = new RTCConfiguration();
            foreach (var c in this.call.iceServers)
            {
                config.AddServer(c.urls, c.username, c.credential);
            }
            this.connection = RtcNavigator.createPeerConnection(config, observe);
            //当需要发送candidate的时候
            this.observe.IceCandidate += evt =>
            {
                this.socket.send(new
                {
                    kind = "webrtc",
                    action = "candidate",
                    to = this.remote.id,
                    candidate = new
                    {
                        candidate = evt.Sdp,
                        sdpMid = evt.SdpMid,
                        sdpMLineIndex = evt.SdpMLineIndex
                    }
                });
            };
            //如果支持新的api，当收到track
            this.observe.AddTrack += evt =>
            {
                this._tracks[evt.Track.Kind] = evt.Track;
                this._emitAddTrack();
            };
            //当媒体流被移除时
            this.observe.RemoveTrack += evt =>
            {
                this._tracks.Remove(evt.Track.Kind);
                this._emitAddTrack();
            };
            //当连接状态发生改变时
            this.observe.IceConnectionChange += state =>
            {
                this.iceConnectionState = state;
                //new, checking, connected, completed, failed, disconnected, closed;
                switch (state)
                {
                    case IceConnectionState.Connected:
                    case IceConnectionState.Completed:
                    case IceConnectionState.Failed:
                    case IceConnectionState.Disconnected:
                    case IceConnectionState.Closed:
                        this._candidates = new List<IceCandidate>();
                        break;
                }
                this.emit("statechanged", state);
                this.emit("state_" + state);
            };
            //检查超时
            this._timer = Timeout.setTimeout(this._timeoutchecker, 10000);
            this.once("statechanged", this._clearchecker);
            //初始化状态
            this.resetState(true);
        }
        /**
         * 初始化状态
         * @param {Boolean} clear 是否超时重置
         */
        public void resetState(bool clear)
        {
            if (this.remote.master) this.setState(new ConnectionQuery(clear));
            else this.setState(new ConnectionWaitForQuery());
        }
        /**
         * 收到消息
         * @param {Object} msg 消息
         */
        public void onmessage(Dictionary<string, object> msg)
        {
            if (msg.Get<string>("action") == "candidate")
            {
                var json = msg.Get<Dictionary<string, object>>("candidate");
                var candidate = new IceCandidate(json.Get<string>("sdpMid"), json.Get<int>("sdpMLineIndex"), json.Get<string>("candidate"));
                if (this._candidates != null) this._candidates.Add(candidate);
                else this.connection.AddIceCandidate(candidate);
            }
            else
            {
                this.state.onmessage(msg);
            }
        }
        /**
         * 取消超时检查
         */
        private void _clearchecker()
        {
            if (null == this._timer) return;
            this._timer.clearTimeout();
            this._timer = null;
        }
        /**
         * 触发超时检查
         */
        private void _timeoutchecker()
        {
            this._timer = null;
            this.connection.Close();
        }
        /**
         * 重新发送offer
         */
        public void tryReoffer()
        {
            if (iceConnectionState == IceConnectionState.Connected || iceConnectionState == IceConnectionState.Completed)
            {
                this.setState(new ConnectionQuery(true));
            }
        }
        private IceConnectionState iceConnectionState;
        /**
         * 触发stream事件
         */
        private void _emitAddTrack()
        {
            var stream = new MediaStream();
            foreach (var key in this._tracks.Keys) stream.AddTrack(this._tracks[key]);
            this.remote.stream = stream;
            this.remote.emit("addtrack", stream);
        }
        /**
         * 设置当前状态
         * @param {Object} state 当前状态
         */
        public void setState(IConnectionState state)
        {
            if (state == this.state) return;
            if (this.state != null) this.state.clear();
            state.connection = this;
            this.state = state;
            state.start();
        }
        /**
         * 发送缓存的candidates
         */
        private void _sendCachedCandidates()
        {
            if (this._candidates == null) return;
            foreach (var c in this._candidates) this.connection.AddIceCandidate(c);
            this._candidates = null;
        }
        /**
         * 创建offer
         * @param {Object} query 对方要求
         */
        public async Task<object> createOffer(Dictionary<string, object> query)
        {
            await this._addTracks(query);
            var offer = await this.connection.CreateOffer(ice_restart: true);
            if (offer == null) throw new Exception("创建offer失败");
            await this.connection.SetLocalDescription("offer", offer);
            this._sendCachedCandidates();
            return new { sdp = offer, type = "offer" };
        }
        /**
         * 创建answer
         * @param {Object} msg 对方消息，包含对方要求和对方offer
         */
        public async Task<object> createAnswer(Dictionary<string, object> msg)
        {
            await this._addTracks(msg.Get<Dictionary<string, object>>("query"));
            var json = msg.Get<Dictionary<string, object>>("offer");
            await this.connection.SetRemoteDescription(json.Get<string>("type"), json.Get<string>("sdp"));
            var answer = await this.connection.CreateAnswer();
            if (answer == null) throw new Exception("创建answer失败");
            await this.connection.SetLocalDescription("answer", answer);
            this._sendCachedCandidates();
            return new { sdp = answer, type = "answer" };
        }
        /**
         * 设置对方回传的answer
         * @param {Object} answer 对方answer
         */
        public async Task setAnswer(Dictionary<string, object> answer)
        {
            await this.connection.SetRemoteDescription(answer.Get<string>("type"), answer.Get<string>("sdp"));
        }
        /**
         * 根据对方要求和自己的实际情况设置要发送的媒体
         * @param {Object} query 对方要求
         */
        public async Task _addTracks(Dictionary<string, object> query)
        {
            foreach (var key in this.local.all)
            {
                var exists = senders.Where(sd => sd.Track.Kind == key.Key).ToArray();
                if (null == query || query.Get<bool>(key.Key))
                {
                    var track = await this.local.all[key.Key].getTrack();
                    if (track != null)
                    {
                        if (exists.Length > 0)
                        {
                            foreach (var sender in exists)
                            {
                                if (sender.Track == track) continue;
                                sender.SetTrack(track);
                            }
                        }
                        else
                        {
                            var sender = this.connection.AddTrack(track, new[] { key.Key });
                            senders.Add(sender);
                        }
                        //如果已经设置或者替换，就继续
                        continue;
                    }
                }
                foreach (var sender in exists)
                {
                    this.connection.RemoveTrack(sender);
                    this.senders.RemoveAll(s => s == sender);
                }
            }
        }

        /**
        * 关闭P2P连接
        */
        public void close()
        {
            this.connection.Close();
        }
    }
}
