using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relywisdom
{
    /**
     * 连接状态
     */
    abstract class IConnectionState
    {
        /**
         * P2P连接
         */
        public MediaConnection connection { get; internal set; }
        /**
         * 是否要设置超时
         */
        protected bool _clear = true;
        /**
         * 状态初始化
         */
        public virtual Task start()
        {
            if (this._clear) this._timer = Timeout.setTimeout(this._timeout, 10000);
            return null;
        }
        /**
         * 超时计时器
         */
        private Timeout _timer;
        /**
         * 结束时，清理超时计时器
         */
        public virtual void clear()
        {
            if (null == this._timer) return;
            this._timer.clearTimeout();
            this._timer = null;
        }
        /**
         * 触发超时，重置状态
         */
        public void _timeout()
        {
            this._timer = null;
            this.connection.resetState(true);
        }
        /**
         * 远程媒体
         */
        public RemoteMedia remote
        {
            get
            {
                return this.connection.remote;
            }
        }
        /**
         * 通话管理器
         */
        public RtcCall call
        {
            get
            {
                return this.connection.remote.call;
            }
        }
        /**
         * 信令连接
         */
        public RtcSocket socket
        {
            get
            {
                return this.connection.remote.call.socket;
            }
        }
        /**
         * 收到消息
         * @param {Object} msg 消息
         */
        public abstract Task onmessage(Dictionary<string, object> msg);
    }
}
