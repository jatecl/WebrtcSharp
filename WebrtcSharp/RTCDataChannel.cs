using System;
using System.Runtime.InteropServices;

namespace WebrtcSharp
{
    /// <summary>
    /// 数据通道状态
    /// </summary>
    public enum RTCDataChannelState
    {
        Connecting,
        Open,  // The DataChannel is ready to send data.
        Closing,
        Closed
    }
    /// <summary>
    /// 数据通道
    /// </summary>
    public class RTCDataChannel : WebrtcObject
    {
        /// <summary>
        /// 响应器
        /// </summary>
        private WebrtcObject observer;
        /// <summary>
        /// 状态变化
        /// </summary>
        private WebrtcUnityCallback _stateChange;
        /// <summary>
        /// 收到消息
        /// </summary>
        private WebrtcUnityResultCallback _message;
        /// <summary>
        /// buffered amout change
        /// </summary>
        private WebrtcUnityResultCallback _bufferedAmoutChange;
        /// <summary>
        /// 持有一个数据通道
        /// </summary>
        /// <param name="handler">数据通道指针</param>
        internal protected RTCDataChannel(IntPtr handler) : base(handler)
        {
            _stateChange = () => this.OnStateChange();
            _message = buffer => this.OnMessage(buffer);
            _bufferedAmoutChange = state => this.OnBufferedAmoutChange(state);
            this.observer = new WebrtcObjectRef(RTCDataChannel_RegisterObserver(handler, _stateChange, _message, _bufferedAmoutChange), "RTCDataChannel Observer");
        }
        /// <summary>
        /// buffered amout change
        /// </summary>
        /// <param name="state"></param>
        private unsafe void OnBufferedAmoutChange(IntPtr state)
        {
            var stateLong = *(long*)state.ToPointer();
        }
        /// <summary>
        /// 搜到消息
        /// </summary>
        /// <param name="buffer">消息数据</param>
        private unsafe void OnMessage(IntPtr buffer)
        {
            if (buffer == IntPtr.Zero) return;
            var arr = new PointerArray(buffer);
            var ptr = arr.GetBuffer();
            int length = *(int*)*ptr;
            ++ptr;
            byte* data = (byte*)*ptr;
            ++ptr;
            int binnary = *(int*)*ptr;
            ++ptr;
            if (binnary > 0)
            {
                if (Data != null)
                {
                    byte[] dataArray = new byte[length];
                    Marshal.Copy(new IntPtr(data), dataArray, 0, length);
                    Data?.Invoke(dataArray);
                }
            }
            else
            {
                if (Message != null)
                {
                    byte[] dataArray = new byte[length];
                    Marshal.Copy(new IntPtr(data), dataArray, 0, length);
                    var msg = System.Text.Encoding.UTF8.GetString(dataArray);
                    Message?.Invoke(msg);
                }
            }
        }
        /// <summary>
        /// 收到文字消息
        /// </summary>
        public event Action<string> Message;
        /// <summary>
        /// 收到二进制消息
        /// </summary>
        public event Action<byte[]> Data;
        /// <summary>
        /// 销毁前被调用
        /// </summary>
        public override void Release()
        {
            if (Handler != IntPtr.Zero) RTCDataChannel_UnregisterObserver(this.Handler);
            base.Release();
        }
        /// <summary>
        /// 状态变化
        /// </summary>
        private void OnStateChange()
        {
            var state = (RTCDataChannelState)RTCDataChannel_State(this.Handler);
            if (state == RTCDataChannelState.Open)
            {
                this.Opened?.Invoke();
            }
            else if (state == RTCDataChannelState.Closing)
            {
                this.Closed?.Invoke();
            }
        }
        /// <summary>
        /// 通道已打开
        /// </summary>
        public event Action Opened;
        /// <summary>
        /// 通道已关闭
        /// </summary>
        public event Action Closed;
        /// <summary>
        /// 通道标识
        /// </summary>
        public string Label
        {
            get
            {
                var proxyPtr = RTCDataChannel_Label(Handler);
                if (proxyPtr == IntPtr.Zero) return null;
                var buffer = new PointerArray(proxyPtr);
                unsafe
                {
                    byte** pointer = (byte**)buffer.GetBuffer();
                    return new string((sbyte*)*pointer);
                }
            }
        }
        /// <summary>
        /// 发送字符串
        /// </summary>
        /// <param name="message">要发送的字符串</param>
        public unsafe void Send(string message)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(message);
            fixed (byte* ptr = bytes)
            {
                RTCDataChannel_Send(Handler, false, new IntPtr(ptr), bytes.Length);
            }
        }
        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="data">要发送的缓冲区</param>
        /// <param name="start">开始字节</param>
        /// <param name="length">发送长度</param>
        public unsafe void Send(byte[] data, int start, int length)
        {
            fixed (byte* ptr = data)
            {
                RTCDataChannel_Send(Handler, true, new IntPtr(ptr + start), length);
            }
        }
        /// <summary>
        /// 关闭数据通道
        /// </summary>
        public void Close()
        {
            RTCDataChannel_Close(Handler);
        }
        /// <summary>
        /// 关闭数据通道
        /// </summary>
        /// <param name="ptr">数据通道指针</param>
        [DllImport(UnityPluginDll)] internal static extern void RTCDataChannel_Close(IntPtr ptr);
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="ptr">数据通道指针</param>
        /// <param name="binnary">是否为二进制</param>
        /// <param name="buffer">数据指针</param>
        /// <param name="length">数据长度</param>
        [DllImport(UnityPluginDll)] internal static extern void RTCDataChannel_Send(IntPtr ptr, bool binnary, IntPtr buffer, int length);
        /// <summary>
        /// 获取数据通道标识
        /// </summary>
        /// <param name="ptr">数据通道指针</param>
        /// <returns>数据通道标识</returns>
        [DllImport(UnityPluginDll)] internal static extern IntPtr RTCDataChannel_Label(IntPtr ptr);
        /// <summary>
        /// 获取数据通道状态
        /// </summary>
        /// <param name="ptr">数据通道指针</param>
        /// <returns>数据通道状态</returns>
        [DllImport(UnityPluginDll)] internal static extern int RTCDataChannel_State(IntPtr ptr);
        /// <summary>
        /// 注册事件响应
        /// </summary>
        /// <param name="ptr">数据通道指针</param>
        /// <param name="stateChange">状态变化回调</param>
        /// <param name="message">收到消息回调</param>
        /// <param name="bufferedAmountChange">buffered amout change</param>
        /// <returns>响应器指针</returns>
        [DllImport(UnityPluginDll)] internal static extern IntPtr RTCDataChannel_RegisterObserver(IntPtr ptr, WebrtcUnityCallback stateChange, WebrtcUnityResultCallback message, WebrtcUnityResultCallback bufferedAmountChange);
        /// <summary>
        /// 取消事件响应
        /// </summary>
        /// <param name="ptr">数据通道指针</param>
        [DllImport(UnityPluginDll)] internal static extern void RTCDataChannel_UnregisterObserver(IntPtr ptr);
    }
}