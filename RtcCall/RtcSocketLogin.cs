using System.Collections.Generic;
using WebSocketSharp;

namespace Relywisdom
{
    /**
     * 正在登录状态
     */
    class RtcSocketLogin : RtcSocketState
    {
        private WebSocket _link;

        /**
        * 连上服务器后，启动登录
        * @param {WebSocket} link web socket
        */
        public RtcSocketLogin(WebSocket link)
        {
            this._link = link;
        }
        /**
         * 类型
         */
        public override string kind => "login";
        /**
         * @type {WebSocket}
         */
        public override WebSocket link => _link;
        /**
         * 状态设置后，初始化
         */
        public override void start()
        {
            this.socket.send(new
            {
                action = "login",
                id = this.socket.id,
                token = this.socket.token,
                info = this.socket.info
            });
            this._timer = Timeout.setTimeout(this._timeout, 10000);
        }
        /**
         * 登录失败定时器
         */
        private Timeout _timer;
        /**
         * 登录成功时，清除失败定时器
         */
        private void _clearTimer()
        {
            if (this._timer != null)
            {
                _timer.clearTimeout();
                this._timer = null;
            }
        }
        /**
         * 收到服务器消息
         * @param {Object} msg 服务器端消息
         */
        public override void onmessage(Dictionary<string, object> msg)
        {
            var action = msg.Get<string>("action");
            if (action == "login_success")
            {
                this._clearTimer();
                this.socket.setState(new RtcSocketConnected(this.link));
            }
            else if (action == "login_error")
            {
                this.onclose();
            }
        }
        /**
         * 如果连接关闭
         */
        public override void onclose()
        {
            this._clearTimer();
        }
        /**
         * 如果登录超时
         */
        private void _timeout()
        {
            this._timer = null;
            this.link.Close();
        }
    }
}
