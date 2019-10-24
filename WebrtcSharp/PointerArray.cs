using System;
using System.Runtime.InteropServices;

namespace WebrtcSharp
{
    /// <summary>
    /// 字节缓冲区，用于自定义协议的数据交换
    /// </summary>
    class PointerArray : WebrtcObject
    {
        /// <summary>
        /// 索引一个新的字节缓冲区
        /// </summary>
        /// <param name="handler">字节缓存区指针</param>
        public PointerArray(IntPtr handler) : base(handler) { }
        /// <summary>
        /// 得到缓冲区字节指针
        /// </summary>
        /// <returns></returns>
        public unsafe void** GetBuffer()
        {
            return (void**)PointerArray_GetBuffer(Handler).ToPointer();
        }
        /// <summary>
        /// C++ API：得到缓冲区字节指针
        /// </summary>
        /// <param name="ptr">缓冲区对象指针</param>
        /// <returns></returns>
        [DllImport(UnityPluginDll)] internal static extern IntPtr PointerArray_GetBuffer(IntPtr ptr);
    }
}
