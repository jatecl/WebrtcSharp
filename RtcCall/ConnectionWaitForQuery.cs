using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relywisdom
{
    /**
     * 等待查询状态
     */
    class ConnectionWaitForQuery : IConnectionState
    {
        public ConnectionWaitForQuery()
        {
            _clear = false;
        }
        /**
         * 收到消息
         * @param {Object} msg 消息
         */
        public override void onmessage(Dictionary<string, object> msg)
        {
            if (msg.Get<string>("action") == "query")
            {
                Task.Factory.StartNew(async () => {
                    var media = await this.connection.setLocalMedia(msg.Get<Dictionary<string, object>>("query"));
                    if (!media.Get<bool>("video") || !media.Get<bool>("audio")) this.connection.setState(new ConnectionMedia(media));
                    else this.connection.setState(new ConnectionOffer());
                });
            }
        }
    }
}
