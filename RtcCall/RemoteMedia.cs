using System.Collections.Generic;
using System.Threading.Tasks;
using WebrtcSharp;

namespace Relywisdom
{
    /**
     * 远程媒体
     */
    public class RemoteMedia : EventEmitter
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
        public async Task connect()
        {
            while (this._connecting())
            {
                this.connection = new MediaConnection(this);
                await Promise.Await(cs => this.connection.once("state_closed", cs));
            }
            this.emit("closed");
        }
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
    }
}
