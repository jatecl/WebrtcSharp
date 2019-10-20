namespace Relywisdom
{
    /**
     * websocket未连接状态
     */
    class RtcSocketStoped : RtcSocketState
    {
        /**
         * 类型
         */
        public override string kind => "close";
        /**
         * 开始连接
         */
        public override void connect()
        {
            this.socket.setState(new RtcSocketConnecting());
        }
    }
}
