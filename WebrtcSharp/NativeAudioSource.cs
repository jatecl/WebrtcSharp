using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WebrtcSharp
{
    /// <summary>
    /// 视频源
    /// </summary>
    internal class NativeAudioSource : AudioSource
    {
        /// <summary>
        /// 持有一个视频源
        /// </summary>
        /// <param name="handler">视频源指针</param>
        internal protected NativeAudioSource(IntPtr handler, IDispatcher dispatcher) : base(handler)
        {
            Dispatcher = dispatcher;
        }

        public IDispatcher Dispatcher { get; }

        protected override void AddSink()
        {
            Dispatcher.Invoke(() => base.AddSink());
        }

        protected override void RemoveSink()
        {
            Dispatcher.Invoke(() => base.RemoveSink());
        }

        public override void Release()
        {
            Dispatcher.Invoke(() => base.Release());
        }
    }
}
