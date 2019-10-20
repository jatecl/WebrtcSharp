using System.Collections.Generic;

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
                this.connection.setState(new ConnectionOffer(msg.Get<Dictionary<string, object>>("query")));
            }
        }
    }
}
