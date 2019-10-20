using System.Collections.Generic;
using System.Linq;
using WebrtcSharp;

namespace Relywisdom
{
    public class MediaStream
    {
        public MediaStreamTrack[] getTracks()
        {
            return tracks.ToArray();
        }

        private List<MediaStreamTrack> tracks = new List<MediaStreamTrack>();

        public VideoTrack[] VideoTracks => tracks.Where(i => i.Kind == "video").Select(i => i as VideoTrack).ToArray();

        public AudioTrack[] AudioTracks => tracks.Where(i => i.Kind == "audio").Select(i => i as AudioTrack).ToArray();

        public void AddTrack(MediaStreamTrack track)
        {
            tracks.Add(track);
        }
    }
}
