using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebrtcSharp;

namespace Relywisdom
{
    /**
     * 远程媒体
     */
    public class RemoteMedia
    {
        /**
         * 创建远程媒体
         * @param {Boolean} master 是否主叫
         * @param {RtcCall} call 通话管理器
         * @param {String} id 对方id
         * @param {String} version 对方版本
         * @param {Object} info 对方附加信息
         */
        public RemoteMedia(bool master, RtcCall call, string id, string version, Dictionary<string, object> info)
        {
            this.master = master;
            this.call = call;
            this.id = id;
            this.version = version;
            this.info = info;
        }
        /**
         * 是否为主叫
         */
        public bool master { get; }
        /**
         * 通话
         */
        public RtcCall call { get; }
        /**
         * 连接
         */
        public RtcSocket socket { get { return call.socket; } }
        /**
         * 对方id
         */
        public string id { get; }
        /**
         * 对方版本编号
         */
        public string version { get; }
        /**
         * 对方信息
         */
        public Dictionary<string, object> info { get; }
        /**
         * 流缓存
         */
        public MediaStream stream { get; internal set; }
        /**
         * P2P连接
         */
        private MediaConnection connection;
        /**
         * 希望连接的数据通道，这里并不存储数据通道，请使用DataChannel += channel => {}接收数据通道
         */
        internal protected Dictionary<string, RTCDataChannelOptions> dataChannels = new Dictionary<string, RTCDataChannelOptions>();
        /**
         * 预注册数据通道。这里不会返回数据通道，请使用DataChannel += channel => {}接收数据通道
         * @param {String} label 数据通道标识
         * @param {Object} optional 选项
         */
        public void registerDataChannel(string label, RTCDataChannelOptions optional = null)
        {
            this.dataChannels[label] = optional;
        }
        /// <summary>
        /// 新的数据通道打开了
        /// </summary>
        public event Action<RTCDataChannel> DataChannel;
        /// <summary>
        /// 新的数据通道打开了
        /// </summary>
        /// <param name="channel">新的数据通道</param>
        internal void emitDataChannel(RTCDataChannel channel)
        {
            DataChannel?.Invoke(channel);
        }
        /**
         * 收到新消息
         * @param {Object} msg 消息
         */
        public void onmessage(Dictionary<string, object> msg)
        {
            if (this.connection != null) this.connection.onmessage(msg);
        }
        /**
         * 是否可以重连
         */
        private bool _connecting()
        {
            return this.socket.state.kind == "connected"
                && this.call.remotes.ContainsKey(this.id);
        }
        /**
         * 连接对方并保持
         */
        public void connect()
        {
            this.connection = new MediaConnection(this);
            Task.Factory.StartNew(connectAsync);
        }
        /// <summary>
        /// 连接对方并保持
        /// </summary>
        /// <returns></returns>
        private async Task connectAsync()
        {
            while (this._connecting())
            {
                if (this.connection == null) this.connection = new MediaConnection(this);
                await Promise.Await(cs =>
                {
                    Action<string> stateChanged = null;
                    stateChanged = state =>
                    {
                        if (state != "closed") return;
                        this.connection.StateChanged -= stateChanged;
                        cs();
                    };
                    this.connection.StateChanged += stateChanged;
                });
                this.connection = null;
            }
            this.Closed?.Invoke();
        }
        /// <summary>
        /// 关闭事件
        /// </summary>
        public event Action Closed;
        /**
         * 本地媒体变化时被调用
         */
        public void localChanged()
        {
            this.connection.tryReoffer();
        }
        /**
         * 结束远程P2P连接
         */
        public void close()
        {
            this.connection.close();
        }

        /// <summary>
        /// 产生了新的媒体轨道
        /// </summary>
        public event Action<MediaStream> AddTrack;

        /// <summary>
        /// 产生了新的媒体轨道
        /// </summary>
        /// <param name="stream"></param>
        internal void emitAddtrack(MediaStream stream)
        {
            AddTrack?.Invoke(stream);
        }
    }
}
