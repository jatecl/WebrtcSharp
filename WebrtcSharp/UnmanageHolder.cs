using System;
using System.Collections.Generic;

namespace WebrtcSharp
{
    /// <summary>
    /// 为了保持一些对象不被回收
    /// </summary>
    public class UnmanageHolder
    {
        /// <summary>
        /// 保持列表
        /// </summary>
        private static readonly List<UnmanageHolder> holders = new List<UnmanageHolder>();
        /// <summary>
        /// 要保持的对象列表
        /// </summary>
        private readonly List<object> holding = new List<object>();
        /// <summary>
        /// 创建保持器
        /// </summary>
        public UnmanageHolder()
        {
            lock (holders) holders.Add(this);
        }
        /// <summary>
        /// 持有对象使其不被销毁
        /// </summary>
        /// <param name="list"
        internal void Hold(params object[] list)
        {
            holding.AddRange(list);
        }
        /// <summary>
        /// 告诉编译器和运行时，我还在呢，别回收我
        /// </summary>
        public void Release()
        {
            this.holding.Clear();
            lock (holders) holders.Remove(this);
        }
    }
}
