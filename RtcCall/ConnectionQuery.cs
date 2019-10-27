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
        public override Task start()
        {
            var task = base.start();
            var query = new Dictionary<string, object>();
            this.call.emitQuery(query, this.remote);
            this.socket.send(new
            {
                kind = "webrtc",
                action = "query",
                to = this.remote.id,
                query
            });
            return task;
        }
        /**
         * 收到消息
         * @param {Object} msg 消息
         */
        public override async Task onmessage(Dictionary<string, object> msg)
        {
            if (msg.Get<string>("action") == "media")
            {
                var media = await this.connection.setLocalMedia(msg.Get<Dictionary<string, object>>("query"));
                var rm = msg.Get<Dictionary<string, object>>("media");
                if (media.Get<bool>("video") || (media.Get<bool>("audio") && !rm.Get<bool>("video"))) this.connection.setState(new ConnectionOffer());
                else
                {
                    this.socket.send(new
                    {
                        kind = "webrtc",
                        action = "require",
                        to = this.remote.id
                    });
                }
            }
            else if (msg.Get<string>("action") == "offer")
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
}
