using System.Collections.Generic;

namespace WebrtcSharp
{
    /// <summary>
    /// 为了保持一些对象不被回收
    /// </summary>
    public class UnmanageHolder
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
