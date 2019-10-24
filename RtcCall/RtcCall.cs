using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Relywisdom
{
    /**
     * 通话
     */
    public class RtcCall : EventEmitter, IMessageFilter
    {
        /**
         * ICE服务器
         */
        public RtcIceServer[] iceServers { get; }
        /**
         * 本地媒体源
         */
        public LocalMedia local { get; }
        /**
         * 连接
         */
        public RtcSocket socket { get; }
        /**
         * 是否可以发送加入事件
         */
        private bool _canbejoin = false;
        /**
         * 创建一个通话
         * @param {RtcSocket} socket 连接
         * @param {LocalMedia} local_media 本地媒体源
         * @param {RtcIceServer[]}} iceServers ICE服务器
         */
        public RtcCall(RtcSocket socket, LocalMedia local_media, RtcIceServer[] iceServers)
        {
            this.socket = socket;
            this.local = local_media;
            this.iceServers = iceServers;
            this.socket.setFilter("webrtc", this);
            this.socket.on("connected", this._connected);
            this.socket.on("changed", this._socketchanged);
            this.local.on("changed", this._localChanged);
        }

        public event Action<Dictionary<string, object>, RemoteMedia> Query;

        internal void emitQuery(Dictionary<string, object> query, RemoteMedia remote)
        {
            Query?.Invoke(query, remote);
        }

        /**
* 状态改变时发生
*/
        private void _socketchanged()
        {
            this._canbejoin = false;
        }
        /**
         * 连接成功时
         */
        private void _connected()
        {
            if (0 == this.remotes.Count)
            {
                this._startCall();
                return;
            }
            var list = new List<string>();
            foreach (var id in this.remotes.Keys) list.Add(id);
            this.socket.send(new
            {
                kind = "webrtc",
                action = "offeline",
                list = list.ToArray()
            });
        }
        /**
         * 通话列表
         */
        public Dictionary<string, RemoteMedia> remotes { get; } = new Dictionary<string, RemoteMedia>();
        /**
         * 房间索引
         */
        private Dictionary<string, bool> rooms = new Dictionary<string, bool>();
        /**
         * 加入房间
         * @param {String} id 房间id
         */
        public bool join(string id)
        {
            if (this.rooms.ContainsKey(id)) return false;
            this.rooms.Add(id, true);
            if (this._canbejoin)
            {
                this.socket.send(new
                {
                    kind = "webrtc",
                    action = "join",
                    list = new[] { id }
                });
            }
            return true;
        }
        /**
         * 收到的消息
         * @param {Object} msg 消息
         */
        public void onmessage(Dictionary<string, object> msg)
        {
            var action = msg.Get<string>("action");
            if (action == "join")
            {
                this._callMedia(msg, true);
            }
            else if (action == "leave")
            {
                this._closeMedia(msg.Get<string>("id"));
            }
            else if (action == "offeline")
            {
                foreach (var id in msg.Get<string[]>("list"))
                {
                    this._closeMedia(id);
                }
                this._startCall();
            }
            else
            {
                if (!msg.ContainsKey("from")) return;
                if (action == "query")
                {
                    this._callMedia(msg, false);
                }
                RemoteMedia now;
                this.remotes.TryGetValue(msg.Get<string>("from"), out now);
                if (now != null) now.onmessage(msg);
            }
        }
        /**
         * 关掉通话
         * @param {String} id 通话id
         */
        public void _closeMedia(string id)
        {
            RemoteMedia now;
            this.remotes.TryGetValue(id, out now);
            if (now != null) return;
            this.remotes.Remove(id);
            now.close();
        }
        /**
         * 开始通话
         * @param {Object}} msg 通话配置
         * @param {Boolean} master 是否主叫
         */
        public void _callMedia(Dictionary<string, object> msg, bool master)
        {
            var from = msg.Get<string>("from");
            var version = msg.Get<string>("version");
            var info = msg.Get<Dictionary<string, object>>("info");
            RemoteMedia now;
            this.remotes.TryGetValue(from, out now);
            if (null == now || now.version != version)
            {
                if (now != null) now.close();
                now = new RemoteMedia(master, this, from, version, info);
                this.remotes[from] = now;
                Task.Factory.StartNew(now.connect);
                this.emit("call", now);
            }
        }
        /**
         * 本地媒体变化时，改变推送方式
         */
        private void _localChanged()
        {
            foreach (var remote in this.remotes.Values) remote.localChanged();
        }
        /**
         * 开始实际加入房间
         */
        private void _startCall()
        {
            this._canbejoin = true;
            var list = new List<string>();
            foreach (var r in this.rooms.Keys) list.Add(r);
            if (0 == list.Count) return;
            this.socket.send(new
            {
                kind = "webrtc",
                action = "join",
                list = list.ToArray()
            });
        }
        /**
         * 结束通话
         */
        public void close()
        {
            foreach (var c in this.rooms.Keys) this.socket.send(new
            {
                kind = "webrtc",
                action = "leave",
                id = c
            });
            this.rooms.Clear();
            var list = new List<string>();
            foreach (var c in this.remotes.Keys) list.Add(c);
            foreach (var c in list) this._closeMedia(c);
        }
    }
}
