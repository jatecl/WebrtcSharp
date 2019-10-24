using Relywisdom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenShare
{
    partial class Program
    {
        static void Main(string[] args)
        {
            Task.Factory.StartNew(Start);

            Console.WriteLine("http://localhost:8080/#/share/view/test-room");
            Console.ReadLine();
        }

        private static void Start()
        {
            var socket = new RtcSocket(
                "ws://localhost:8124/",
                Guid.NewGuid().ToString(),
                null,
                new Dictionary<string, object>{
                    { "name", "Hello" },
                    { "type", "device" }
                });

            //* 
            var videoSource = new LocalMediaSource("video");
            //screen-capture-recorder无法正常工作
            //videoSource.source = "screen-capture-recorder";
            /*/
            var videoSource = new ScreenSource();
            //*/
            var media = new LocalMedia(videoSource, new LocalMediaSource("audio"));
            var call = new RtcCall(socket, media, new[] {
                new RtcIceServer {
                    urls = new []{ "stun:stun.l.google.com:19302" }
                }
            });
            call.Query += (query, info) =>
            {
                query["video"] = false;
                query["audio"] = false;
            };

            call.on<RemoteMedia>("call", link =>
            {
                link.on("closed", () => { });
            });

            call.join("test-room");
            Task.Factory.StartNew(async () =>
            {
                await call.local.open();
                await socket.connect();
            });
        }
    }
}
