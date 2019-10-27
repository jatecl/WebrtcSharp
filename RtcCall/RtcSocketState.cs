using System.Collections.Generic;
using WebSocketSharp;

namespace Relywisdom
{
    public abstract class RtcSocketState
    {
        /**
         * 连接
         */
        public RtcSocket socket { get; internal set; }
        public abstract string kind { get; }
        public virtual WebSocket link { get { return null; } }

        public virtual void clear() { }

        public virtual void connect()
        {

        }

        public virtual void onclose()
        {

        }

        public virtual void onmessage(Dictionary<string, object> msg)
        {

        }

        public virtual void start()
        {

        }
    }
}
