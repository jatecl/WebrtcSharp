using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relywisdom
{
    /**
     * 发送查询并等待offer的状态
     */
    class ConnectionQuery : IConnectionState
    {
        /**
         * 创建状态
         * @param {Boolean} clear 是否设置超时
         */
        public ConnectionQuery(bool clear)
        {
            this._clear = clear;
        }
        /**
         * 初始化
         */
        public override void start()
        {
            base.start();
            var query = new Dictionary<string, object>();
            this.call.emit("query", query, this.remote);
            this.socket.send(new
            {
                kind = "webrtc",
                action = "query",
                to = this.remote.id,
                query
            });
        }
        /**
         * 收到消息
         * @param {Object} msg 消息
         */
        public override void onmessage(Dictionary<string, object> msg)
        {
            if (msg.Get<string>("action") == "offer")
            {
                var task = _sendAnswer(msg);
            }
        }

        private async Task _sendAnswer(Dictionary<string, object> msg)
        {
            var answer = await this.connection.createAnswer(msg);
            this.socket.send(new
            {
                kind = "webrtc",
                action = "answer",
                to = this.remote.id,
                answer
            });
            this.connection.setState(new ConnectionWaitForQuery());
        }
    }
}
