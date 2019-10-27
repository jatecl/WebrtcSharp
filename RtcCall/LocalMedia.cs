using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relywisdom
{
    /**
     * 多媒体源
     */
    public class LocalMedia : System.IDisposable
    {
        public LocalMedia(ILocalMediaSource video, ILocalMediaSource audio)
        {
            if (video != null)
            {
                this.video = video;
                this.all["video"] = this.video;
                this.video.Changed += enabled => this._mediaChanged(this.video, enabled);
            }
            if (audio != null)
            {
                this.audio = audio;
                this.all["audio"] = this.audio;
                this.audio.Changed += enabled => this._mediaChanged(this.audio, enabled);
            }
        }
        public LocalMedia() : this(new LocalMediaSource("video"), new LocalMediaSource("audio")) { }
        /**
         * 视频源
         */
        public readonly ILocalMediaSource video;
        /**
         * 音频源
         */
        public readonly ILocalMediaSource audio;
        /**
         * 所有媒体源
         */
        public readonly Dictionary<string, ILocalMediaSource> all = new Dictionary<string, ILocalMediaSource>();
        /**
         * 打开媒体
         */
        public async Task open()
        {
            foreach (var key in this.all.Keys) await this.all[key].open();
        }
        /**
         * 获得播放流
         */
        private void _mediaChanged(ILocalMediaSource source, bool enabled)
        {
            this.Changed?.Invoke(source, enabled);
        }
        /// <summary>
        /// 媒体源发生变化时
        /// </summary>
        public event Action<ILocalMediaSource, bool> Changed;

        public void Dispose()
        {
            foreach (var key in this.all.Keys) this.all[key].Dispose();
        }
    }
}
