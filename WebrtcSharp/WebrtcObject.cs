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
    internal delegate void WebrtcUnityCallback();
    /// <summary>
    /// 给dll调用的状态回调
    /// </summary>
    /// <param name="state"></param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void WebrtcUnityStateCallback(int state);
    /// <summary>
    /// 给dll调用用的数据回调
    /// </summary>
    /// <param name="val"></param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void WebrtcUnityResultCallback(IntPtr val);
    /// <summary>
    /// 给dll用的带两个数据的回调
    /// </summary>
    /// <param name="val"></param>
    /// <param name="val2"></param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void WebrtcUnityResult2Callback(IntPtr val, IntPtr val2);
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
                BeforeDelete?.Invoke(this);
                WebrtcObject_delete(Handler);
                Handler = IntPtr.Zero;
            }
        }
        /// <summary>
        /// 内存回收前被调用
        /// </summary>
        public event Action<WebrtcObject> BeforeDelete;
        /// <summary>
        /// 缓存
        /// </summary>
        private static Dictionary<long, WebrtcObject> senderCache = new Dictionary<long, WebrtcObject>();
        /// <summary>
        /// 创建一个指针对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="handler">指针</param>
        /// <returns>如果指针为空，就返回空；否则返回对应的对象</returns>
        public static T Create<T>(IntPtr handler, bool cache = false) where T : WebrtcObject
        {
            if (handler == IntPtr.Zero) return null;
            if (cache)
            {
                lock (senderCache)
                {
                    var key = handler.ToInt64();
                    if (senderCache.ContainsKey(key)) return (T)senderCache[key];
                    var val = Create<T>(handler);
                    val.BeforeDelete += Val_BeforeDelete;
                    senderCache[key] = val;
                    return val;
                }
            }
            var constructor = typeof(T).GetConstructor(new Type[] { typeof(IntPtr) });
            if (constructor == null) throw new Exception("不能用Create方法创建类型" + typeof(T).Name);
            return (T)constructor.Invoke(new object[] { handler });
        }
        /// <summary>
        /// 检查删除指针时，是否需要从缓存里面干掉
        /// </summary>
        /// <param name="val"></param>
        private static void Val_BeforeDelete(WebrtcObject val)
        {
            val.BeforeDelete -= Val_BeforeDelete;
            lock (senderCache)
            {
                var key = val.Handler.ToInt64();
                senderCache.Remove(key);
            }
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
}
