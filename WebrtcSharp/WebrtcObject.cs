using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WebrtcSharp
{
    /// <summary>
    /// 给dll调用的无参回调
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void WebrtcUnityCallback();
    /// <summary>
    /// 给dll调用的状态回调
    /// </summary>
    /// <param name="state"></param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void WebrtcUnityStateCallback(int state);
    /// <summary>
    /// 给dll调用用的数据回调
    /// </summary>
    /// <param name="val"></param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void WebrtcUnityResultCallback(IntPtr val);
    /// <summary>
    /// 给dll用的带两个数据的回调
    /// </summary>
    /// <param name="val"></param>
    /// <param name="val2"></param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void WebrtcUnityResult2Callback(IntPtr val, IntPtr val2);
    /// <summary>
    /// dll指针对象
    /// </summary>
    public class WebrtcObject
    {
        /// <summary>
        /// 指针
        /// </summary>
        protected internal IntPtr Handler { get; protected set; }
        /// <summary>
        /// 删除c++中指针对应的对象
        /// </summary>
        protected virtual void Delete()
        {
            if (Handler != IntPtr.Zero)
            {
                WebrtcObject_delete(Handler);
                Handler = IntPtr.Zero;
            }
        }
        /// <summary>
        /// 创建一个指针对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="handler">指针</param>
        /// <returns>如果指针为空，就返回空；否则返回对应的对象</returns>
        public static T Create<T>(IntPtr handler) where T : WebrtcObject
        {
            if (handler == IntPtr.Zero) return null;
            var constructor = typeof(T).GetConstructor(new Type[] { typeof(IntPtr) });
            if (constructor == null) throw new Exception("不能用Create方法创建类型" + typeof(T).Name);
            return (T)constructor.Invoke(new object[] { handler });
        }
        /// <summary>
        /// 创建一个C++指针对象
        /// </summary>
        /// <param name="handler">指针</param>
        public WebrtcObject(IntPtr handler)
        {
            Handler = handler;
        }
        /// <summary>
        /// 对象被销毁时，同时销毁相关C++指针对象
        /// </summary>
        ~WebrtcObject()
        {
            if (Handler != IntPtr.Zero)
            {
                Delete();
                Handler = IntPtr.Zero;
            }
        }
        /// <summary>
        /// C++ API：销毁C++指针对象
        /// </summary>
        /// <param name="ptr"></param>
        /// <returns></returns>
        [DllImport(UnityPluginDll)] internal static extern IntPtr WebrtcObject_delete(IntPtr ptr);
        /// <summary>
        /// C++ API所在dll
        /// </summary>
        public const string UnityPluginDll = "unityplugin.dll";
    }
    /// <summary>
    /// 字节缓冲区，用于自定义协议的数据交换
    /// </summary>
    class StringBuffer : WebrtcObject
    {
        /// <summary>
        /// 索引一个新的字节缓冲区
        /// </summary>
        /// <param name="handler">字节缓存区指针</param>
        public StringBuffer(IntPtr handler) : base(handler) { }
        /// <summary>
        /// 得到缓冲区字节指针
        /// </summary>
        /// <returns></returns>
        public unsafe byte** GetBuffer()
        {
            return (byte**)StringBuffer_GetBuffer(Handler).ToPointer();
        }
        /// <summary>
        /// C++ API：得到缓冲区字节指针
        /// </summary>
        /// <param name="ptr">缓冲区对象指针</param>
        /// <returns></returns>
        [DllImport(UnityPluginDll)] internal static extern IntPtr StringBuffer_GetBuffer(IntPtr ptr);
    }
    /// <summary>
    /// 为了保持一些对象不被回收
    /// </summary>
    class UnmanageHolder
    {
        /// <summary>
        /// 用户保存对象的列表
        /// </summary>
        private readonly List<object> Holder = new List<object>();
        /// <summary>
        /// 持有对象使其不被销毁
        /// </summary>
        /// <param name="list">对象列表</param>
        public void Hold(params object[] list)
        {
            Holder.AddRange(list);
        }
        /// <summary>
        /// 告诉编译器和运行时，我还在呢，别回收我
        /// </summary>
        public void StillHere()
        {
            this.Holder.Add("still here");
        }
    }
}
