using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WebrtcSharp
{
    /// <summary>
    /// 异步接口，和js的Promise功能类似。使用Promise.Await代替new Promsie
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    public class Promise<T>
    {
        /// <summary>
        /// 得到一个异步回调，这个回调可能成功，也可能失败
        /// </summary>
        /// <param name="callback">异步回调</param>
        /// <returns></returns>
        public static async Task<T> Await(Action<Action<T>, Action<Exception>> callback)
        {
            var task = new TaskCompletionSource<T>();
            callback(res => task.SetResult(res), err => task.SetException(err));
            return await task.Task;
        }
    }

    /// <summary>
    /// 异步接口，和js的Promise功能类似。使用Promise.Await代替new Promsie
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    public class Promise
    {
        /// <summary>
        /// 得到一个异步回调，这个回调可能成功，也可能失败
        /// </summary>
        /// <param name="callback">异步回调</param>
        /// <returns></returns>
        public static async Task Await(Action<Action, Action<Exception>> callback)
        {
            var task = new TaskCompletionSource<object>();
            callback(() => task.SetResult(null), err => task.SetException(err));
            await task.Task;
        }
    }
}
