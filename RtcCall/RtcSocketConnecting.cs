using System.Collections.Generic;
using WebSocketSharp;

namespace Relywisdom
{
    /**
     * 正在连接状态
     */
    class RtcSocketConnecting : RtcSocketState
    {
        private WebSocket _link;

        /**
        * 状态设置时被调用
        */
        public override void start()
        {
            this._link = new WebSocket(this.socket.server);
            //如果在连接中断开了，直接设置为连接失败
            this.link.OnClose += (sender, evt) => this.socket.onclose();
            //对认证消息进行响应
            this.link.OnMessage += (sender, evt) =>
            {
                this.socket.onmessage(new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, object>>(evt.Data));
            };
            //发送认证消息
            this.link.OnOpen += (sender, evt) =>
            {
                this.socket.setState(new RtcSocketLogin(this._link));
            };
            this._link.Connect();
        }
        /**
         * 类型
         */
        public override string kind => "connecting";
        /**
         * @type {WebSocket}
         */
        public override WebSocket link => _link;
    }
}
