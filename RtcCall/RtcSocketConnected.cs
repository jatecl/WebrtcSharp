namespace Relywisdom
{
    /**
     * 连接成功状态
     */
    class RtcSocketConnected : RtcSocketState
    {
        private WebSocket _link;

        /**
        * 创建连接成功状态
        * @param {WebSocket} link web socket
        */
        public RtcSocketConnected(WebSocket link)
        {
            this._link = link;
        }
        /**
         * 类型
         */
        public override string kind => "connected";
        /**
         * @type {WebSocket}
         */
        public override WebSocket link => _link;
    }
}
