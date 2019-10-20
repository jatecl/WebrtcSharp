using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relywisdom
{
    /**
     * 多媒体源
     */
    public class LocalMedia : EventEmitter
    {
        public LocalMedia(ILocalMediaSource video, ILocalMediaSource audio)
        {
            this.video = video;
            this.audio = audio;
            this.all["video"] = this.video;
            this.all["audio"] = this.audio;
            this.video.on("changed", () => this._mediaChanged(this.video));
            this.audio.on("changed", () => this._mediaChanged(this.audio));
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
        private void _mediaChanged(ILocalMediaSource source)
        {
            this.emit("changed", source);
        }
    }
}
