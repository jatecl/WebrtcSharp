using System;
using System.Threading.Tasks;
using WebrtcSharp;

namespace Relywisdom
{
    /**
     * 某种类型的媒体流来源
     */
    public abstract class ILocalMediaSource : System.IDisposable
    {
        public ILocalMediaSource(string kind)
        {
            this.kind = kind;
        }
        /**
         * 当前媒体流
         */
        protected MediaStreamTrack _track;
        /**
         * 媒体类型
         */
        public string kind { get; }
        /**
         * 设置当前媒体源是否可用
         */
        public bool isEnabled { get; private set; } = true;
        /**
         * 设置当前媒体源是否可用
         * @param {Boolean} enabled 设置当前媒体源是否可用
         */
        public void setEnabled(bool enabled)
        {
            if (enabled != this.isEnabled)
            {
                this.isEnabled = enabled;
                if (!enabled) this._justClose();
                this.Enabled?.Invoke(enabled);
            }
        }
        /// <summary>
        /// 媒体源是否可用事件
        /// </summary>
        public event Action<bool> Enabled;
        /**
         * 创建媒体通道
         */
        public async Task<MediaStreamTrack> getTrack()
        {
            if (null == this._track && this.isEnabled) await this.open();
            return this._track;
        }
        /**
         * 打开媒体流
         */
        public abstract Task<bool> open();
        /**
         * 关闭并通知变化
         */
        protected virtual void _justClose(bool nochange = false)
        {
            if (null != this._track)
            {
                this._track.Enabled = false;
                this._track = null;
                if (!nochange) this.Changed?.Invoke(false);
            }
        }
        /// <summary>
        /// 媒体源发生变化
        /// </summary>
        public event Action<bool> Changed;
        /// <summary>
        /// 媒体源发生变化
        /// </summary>
        /// <param name="changed"></param>
        protected void emitChanged(bool changed)
        {
            this.Changed?.Invoke(changed);
        }
        /**
         * 关闭媒体流
         */
        public void close()
        {
            this._justClose();
            this.Close?.Invoke();
        }
        /// <summary>
        /// 关闭事件
        /// </summary>
        public event Action Close;
        /**
         * 是否可用
         */
        public abstract Task<bool> usable();

        public virtual void Dispose()
        {
            this._justClose(true);
        }
    }
}
