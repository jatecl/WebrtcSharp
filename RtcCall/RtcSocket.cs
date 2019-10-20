using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebrtcSharp;

namespace Relywisdom
{
    /// <summary>
    /// WebSocket 连接
    /// </summary>
    public class RtcSocket : EventEmitter
    {
        /**
         * 创建一个连接
         * @param {String} server ws服务地址
         * @param {String} id 当前用户id
         */
        public RtcSocket(string server, string id, string token, Dictionary<string, object> info)
        {
            this.server = server;
            this.id = id;
            this.token = token;
            this.info = info;
            this.setState(new RtcSocketStoped(), true);
        }
        /**
         * ws服务地址
         */
        public string server { get; }
        /**
         * 状态
         */
        public RtcSocketState state { get; private set; }
        /**
         * 自己的ID
         */
        public string id { get; }
        /**
         * 登录用的token
         */
        public string token { get; }
        /**
         * 附加信息
         */
        public Dictionary<string, object> info { get; }
        /**
         * 消息分发器
         */
        private Dictionary<string, IMessageFilter> filters = new Dictionary<string, IMessageFilter>();
        /**
         * 连接中
         */
        private bool connecting = false;
        /**
         * 设置消息处理器
         * @param {String} key 消息类型
         * @param {MessageFilter} val 消息处理器
         */
        public void setFilter(string key, IMessageFilter val)
        {
            if (this.filters.ContainsKey(key)) throw new Exception($"分发器{ key }已存在");
            this.filters.Add(key, val);
        }
        /**
         * 移除消息处理器
         * @param {String} key 消息类型
         */
        public void removeFilter(string key)
        {
            this.filters.Remove(key);
        }
        /**
         * 设置状态
         * @param {Object} state 状态
         */
        public void setState(RtcSocketState state, bool noemit = false)
        {
            if (state == this.state) return;
            if (this.state != null) this.state.clear();
            state.socket = this;
            this.state = state;
            state.start();
            if (!noemit)
            {
                this.emit(state.kind, state);
                this.emit("changed", state);
            }
        }
        /**
         * 收到服务器信息
         * @param {Object} msg 信息
         */
        public void onmessage(Dictionary<string, object> msg)
        {
            if (this.state.kind == "connected"
                && msg.ContainsKey("kind")
                && this.filters.ContainsKey(msg.Get<string>("kind")))
            {
                this.filters[msg.Get<string>("kind")].onmessage(msg);
                return;
            }
            this.state.onmessage(msg);
        }
        /**
         * 收到关闭消息
         */
        public void onclose()
        {
            this.state.onclose();
            this.setState(new RtcSocketStoped());
        }
        /**
         * 发送消息
         * @param {Object} msg 消息
         */
        public bool send(Object msg)
        {
            if (null == this.state.link) return false;
            this.state.link.send(new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(msg));
            return true;
        }
        /**
         * 开始连接
         */
        public async Task connect()
        {
            if (this.connecting) return;
            this.connecting = true;
            while (this.connecting)
            {
                //尝试连接
                this.state.connect();
                //等待连接关闭
                await Promise.Await(cs => this.once("close", cs));
                //1秒后重试
                if (this.connecting) await Promise.Await(cs => Timeout.setTimeout(cs, 1000));
            }
        }
        /**
         * 结束连接
         */
        public void close()
        {
            this.connecting = false;
            this.onclose();
        }
    }
}
