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
    class MediaConnection
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
        /// 创建数据通道
        /// </summary>
        private Dictionary<string, RTCDataChannel> _dataChannels = new Dictionary<string, RTCDataChannel>();
        /// <summary>
        /// P2P连接
        /// </summary>
        private PeerConnection connection;
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
            this.connection = RtcNavigator.createPeerConnection(config);
            //当需要发送candidate的时候
            this.connection.IceCandidate += evt =>
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
            this.connection.TrackAdded += evt =>
            {
                this._tracks[evt.Track.Kind] = evt.Track;
                this._emitAddTrack();
            };
            //当媒体流被移除时
            this.connection.TrackRemoved += evt =>
            {
                this._tracks.Remove(evt.Track.Kind);
                this._emitAddTrack();
            };
            //当连接状态发生改变时
            this.connection.IceConnectionChange += state =>
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
                        if (this._candidates == null || this._candidates.Count > 0)
                        {
                            this._candidates = new List<IceCandidate>();
                        }
                        break;
                }
                var name = GetIceConnectionName(state);
                this.StateChanged?.Invoke(name);
                this._clearchecker();
            };
            this.connection.DataChannel += channel =>
            {
                this._registerDataChannel(channel);
            };
            //检查超时
            this._timer = Timeout.setTimeout(this._timeoutchecker, 10000);
            //初始化状态
            this.resetState(true);
        }

        /// <summary>
        /// 状态变好时发生
        /// </summary>
        public event Action<string> StateChanged;
        /// <summary>
        /// 状态转换为标准字符串
        /// </summary>
        /// <param name="state">状态</param>
        /// <returns></returns>
        private static string GetIceConnectionName(IceConnectionState state)
        {
            switch (state)
            {
                case IceConnectionState.New: return "new";
                case IceConnectionState.Connected: return "connected";
                case IceConnectionState.Completed: return "completed";
                case IceConnectionState.Failed: return "failed";
                case IceConnectionState.Disconnected: return "disconnected";
                case IceConnectionState.Closed: return "closed";
                case IceConnectionState.Max: return "max";
                case IceConnectionState.Checking: return "checking";
            }
            return "unkown";
        }
        /**
         * 创建数据通道
         */
        private void _createDataChannel(string label, RTCDataChannelOptions options)
        {
            if (this._dataChannels.ContainsKey(label)) return;
            var ch = this.connection.CreateDataChannel(label, options);
            this._registerDataChannel(ch);
        }
        /**
         * 缓存数据通道
         * @param {RTCDataChannel} ch 数据通道
         */
        private void _registerDataChannel(RTCDataChannel ch)
        {
            this._dataChannels[ch.Label] = ch;
            ch.Opened += () => this.remote.emitDataChannel(ch);
        }
        /**
        * 建立所有数据通道
        */
        private void _setDataChannels()
        {
            foreach (var label in this.remote.dataChannels.Keys)
            {
                if (this._dataChannels.ContainsKey(label)) continue;
                RTCDataChannelOptions optional;
                this.remote.dataChannels.TryGetValue(label, out optional);
                this._createDataChannel(label, optional);
            }
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
                Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        var task = this.state.onmessage(msg);
                        if (task != null) await task;
                    }
                    catch (Exception exp)
                    {
                        Console.WriteLine(exp.Message);
                        this.resetState(false);
                    }
                });
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
            this.remote.emitAddtrack(stream);
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
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    var task = state.start();
                    if (task != null) await task;
                }
                catch (Exception exp)
                {
                    Console.WriteLine(exp.Message);
                    this.resetState(false);
                }
            });
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
        public async Task<object> createOffer()
        {
            this._setDataChannels();
            var offer = await this.connection.CreateOffer(ice_restart: true);
            if (offer == null) throw new Exception("创建offer失败");
            await this.connection.SetLocalDescription("offer", offer);
            this._sendCachedCandidates();
            return new { sdp = offer, type = "offer" };
        }
        /**
         * 创建answer
         * @param {Object} json 对方offer
         */
        public async Task<object> createAnswer(Dictionary<string, object> json)
        {
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
        public async Task<Dictionary<string, object>> setLocalMedia(Dictionary<string, object> query)
        {
            var ret = new Dictionary<string, object>();
            if (this.local == null) return ret;
            var senders = this.connection.GetSenders();
            foreach (var key in this.local.all)
            {
                var exists = senders.Where(sd => sd != null && sd.Track.Kind == key.Key).ToArray();
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
                            this.connection.AddTrack(track, new[] { key.Key });
                        }
                        ret[key.Key] = true;
                        //如果已经设置或者替换，就继续
                        continue;
                    }
                }
                foreach (var sender in exists)
                {
                    this.connection.RemoveTrack(sender);
                }
            }
            return ret;
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
