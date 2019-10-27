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
         * 初始化
         */
        public override async Task start()
        {
            var task = base.start();
            if (task != null) await task;
            var offer = await this.connection.createOffer();
            this.socket.send(new
            {
                kind = "webrtc",
                action = "offer",
                to = this.remote.id,
                offer
            });
        }
        /**
        * 收到消息
        * @param {Object} msg 消息
        */
        public override async Task onmessage(Dictionary<string, object> msg)
        {
            if (msg.Get<string>("action") == "answer")
            {
                await this.connection.setAnswer(msg.Get<Dictionary<string, object>>("answer"));
                this.connection.setState(new ConnectionWaitForQuery());
            }
        }
    }
}
