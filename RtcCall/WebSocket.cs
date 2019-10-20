using System;

namespace Relywisdom
{
    /// <summary>
    /// 接口和js websocket差不多的类
    /// </summary>
    public class WebSocket
    {
        private string url;
        public WebSocket(string url)
        {
            this.url = url;
        }
        public Action<string> onmessage;
        public Action onopen;
        public Action onclose;
        private WebSocketSharp.WebSocket ws;
        public int readyState
        {
            get
            {
                if (this.ws == null) return 0;
                if (this.ws.IsAlive) return 1;
                else return 2;
            }
        }
        public void close()
        {
            if (this.ws == null) return;
            ws.Close();
        }
        public void send(string message)
        {
            Console.WriteLine("发送>>>>:" + message);
            if (ws == null) return;
            if (!ws.IsAlive) return;
            ws.Send(System.Text.Encoding.UTF8.GetBytes(message));
        }
        public void open()
        {
            if (this.ws != null) throw new Exception("opened before");
            try
            {
                this.ws = new WebSocketSharp.WebSocket(url);
                ws.OnMessage += Ws_OnMessage;
                ws.OnOpen += Ws_OnOpen;
                ws.OnClose += Ws_OnClose;
                ws.OnError += Ws_OnError;
                ws.Connect();
            }
            catch
            {
                this.close();
                return;
            }
        }
        private void Ws_OnOpen(object sender, EventArgs e)
        {
            onopen?.Invoke();
        }
        /**
         * 连接关闭
         */
        private void Ws_OnClose(object sender, WebSocketSharp.CloseEventArgs e)
        {
            ws = null;
            this.onclose?.Invoke();
        }
        private void Ws_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
        }
        private void Ws_OnMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {
            Console.WriteLine("收到<<<<:" + e.Data);
            onmessage?.Invoke(e.Data);
        }
    }
}
