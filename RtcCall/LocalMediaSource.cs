using System;
using System.Threading.Tasks;
using WebrtcSharp;

namespace Relywisdom
{
    /**
     * 某种类型的媒体流来源
     */
    public class LocalMediaSource : ILocalMediaSource
    {
        public LocalMediaSource(string kind) : base(kind) { }
        /**
         * 媒体源描述
         */
        public string source;
        /**
         * 打开媒体流
         */
        public async Task<bool> open(string source)
        {
            try
            {
                if (source != null)
                {
                    if (source != this.source) this._justClose(true);
                    this.source = source;
                }
                else if (null == this.source)
                {
                    var vss = this.getDevices();
                    if (vss.Length > 0) this.source = vss[0];
                }
                if (!this.isEnabled)
                {
                    this._justClose();
                    return false;
                }
                if (null != this._track) return true;
                this._track = getUserMedia();
                this.emit("changed", true);
                this.emit("open");
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public override Task<bool> open()
        {
            return open(null);
        }

        /**
         * 拉取媒体流
         * @param {Object} opts 媒体选项
         */
        public MediaStreamTrack getUserMedia()
        {
            if (kind == "video")
            {
                return RtcNavigator.createVideoTrack(source);
            }
            else
            {
                return RtcNavigator.createAudioTrack(source);
            }
        }
        /**
         * 获得所有媒体设备
         */
        public string[] getDevices()
        {
            if (kind == "video") return RtcNavigator.getVideoDevices();
            return RtcNavigator.getAudioDevices();
        }
        /**
         * 是否可用
         */
        public override async Task<bool> usable()
        {
            if (!this.isEnabled) return false;
            var devs = this.getDevices();
            return devs.Length > 0;
        }
    }
}
