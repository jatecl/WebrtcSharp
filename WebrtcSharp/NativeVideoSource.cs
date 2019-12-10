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
    internal class NativeVideoSource : VideoSource
    {
        /// <summary>
        /// 持有一个视频源
        /// </summary>
        /// <param name="handler">视频源指针</param>
        internal protected NativeVideoSource(IntPtr handler, IDispatcher dispatcher) : base(handler)
        {
            Dispatcher = dispatcher;
        }

        public IDispatcher Dispatcher { get; private set; }
        protected override void AddSink()
        {
            if (Handler == IntPtr.Zero) return;
            Dispatcher.Invoke(() => base.AddSink());
        }

        protected override void RemoveSink()
        {
            if (Handler == IntPtr.Zero) return;
            Dispatcher.Invoke(() => base.RemoveSink());
        }

        public override void Release()
        {
            if (Handler == IntPtr.Zero) return;
            Dispatcher.Invoke(() => base.Release());
        }
    }
}
