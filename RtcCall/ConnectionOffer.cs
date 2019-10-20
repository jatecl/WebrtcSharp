using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relywisdom
{
    /**
     * 接收查询，并发送查询与offer然后等待answer
     */
    class ConnectionOffer : IConnectionState
    {
        /**
         * 创建状态
         * @param {Object} query 对方的查询
         */
        public ConnectionOffer(Dictionary<string, object> query)
        {
            this.query = query;
        }
        /**
         * 对方查询
         */
        private Dictionary<string, object> query;
        /**
         * 初始化
         */
        public override void start()
        {
            base.start();
            var task = this.startSendQuery();
        }

        private async Task startSendQuery()
        {
            var offer = await this.connection.createOffer(this.query);
            var query = new Dictionary<string, object>();
            this.call.emit("query", query, this.remote);
            this.socket.send(new
            {
                kind = "webrtc",
                action = "offer",
                to = this.remote.id,
                query,
                offer
            });
        }

        /**
        * 收到消息
        * @param {Object} msg 消息
        */
        public override void onmessage(Dictionary<string, object> msg)
        {
            if (msg.Get<string>("action") == "answer")
            {
                var task = this._setAnswer(msg.Get<Dictionary<string, object>>("answer"));
            }
        }

        private async Task _setAnswer(Dictionary<string, object> answer)
        {
            await this.connection.setAnswer(answer);
            this.connection.setState(new ConnectionWaitForQuery());
        }
    }
}
