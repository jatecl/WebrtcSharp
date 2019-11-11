using Relywisdom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebrtcSharp;

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
            //videoSource.source = "WN_L7501_V1";
            //screen-capture-recorder无法正常工作
            //videoSource.source = "screen-capture-recorder";
            /*/
            var videoSource = new ScreenSource();
            //*/
            var media = new LocalMedia(videoSource, new LocalMediaSource("audio"));
            var call = new RtcCall(new[] {
                new RtcIceServer {
                    urls = new []{ "stun:stun.l.google.com:19302" }
                }
            }, socket, media);
            call.Query += (query, info) =>
            {
                query.Video = false;
                query.Audio = false;
            };

            call.Call += link =>
            {
                link.registerDataChannel("data");
                link.DataChannel += channel =>
                {
                    channel.Message += msg => Console.WriteLine("datachannel: " + msg);
                    var timer = Timeout.setInterval(() => channel.Send(DateTime.Now.ToString()), 1000);
                    channel.Closed += () =>
                    {
                        timer.clearInterval();
                        Console.WriteLine("data channel closed.");
                    };
                };
                link.Closed += () => { };
            };

            call.join("test-room");
            Task.Factory.StartNew(async () =>
            {
                await call.local.open();
                await socket.connect();
            });
        }
    }
}
