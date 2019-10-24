using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Relywisdom
{
    /// <summary>
    /// 事件触发
    /// </summary>
    public class EventEmitter
    {
        //事件注册列表
        private Dictionary<string, List<EmitCount>> events = new Dictionary<string, List<EmitCount>>();
        private class EmitCount
        {
            public Delegate Event;
            public int Count = 0;

            private ParameterInfo[] Arguments;

            internal ParameterInfo[] GetParameters()
            {
                if (Arguments == null) Arguments = Event.Method.GetParameters();
                return Arguments;
            }
        }

        //同步锁
        private readonly object locker = new object();

        /// <summary>
        /// 注册事件通知
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">回调函数</param>
        public void on(string eventName, Delegate listener)
        {
            this.on(eventName, listener, -1);
        }
        /// <summary>
        /// 注册事件通知
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">回调函数</param>
        public void on(string eventName, Action listener)
        {
            this.on(eventName, listener, -1);
        }
        /// <summary>
        /// 注册事件通知
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">回调函数</param>
        public void on<T>(string eventName, Action<T> listener)
        {
            this.on(eventName, listener, -1);
        }
        /// <summary>
        /// 注册事件通知
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">回调函数</param>
        public void on<T, U>(string eventName, Action<T, U> listener)
        {
            this.on(eventName, listener, -1);
        }
        /// <summary>
        /// 注册事件通知
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">回调函数</param>
        public void on<T, U, V>(string eventName, Action<T, U, V> listener)
        {
            this.on(eventName, listener, -1);
        }
        /// <summary>
        /// 注册事件通知
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">回调函数</param>
        private void on(string eventName, Delegate listener, int count)
        {
            lock (locker)
            {
                List<EmitCount> list = null;
                if (!events.ContainsKey(eventName))
                {
                    list = new List<EmitCount>();
                    events.Add(eventName, list);
                }
                else
                {
                    list = events[eventName];
                    if (list.Any(item => item.Event == listener)) return;
                }
                list.Add(new EmitCount { Event = listener, Count = count });
            }
        }
        /// <summary>
        /// 注册事件通知。只响应一次
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">回调函数</param>
        public void once(string eventName, Delegate listener)
        {
            this.on(eventName, listener, 1);
        }
        /// <summary>
        /// 注册事件通知。只响应一次
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">回调函数</param>
        public void once(string eventName, Action listener)
        {
            this.on(eventName, listener, 1);
        }
        /// <summary>
        /// 注册事件通知。只响应一次
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">回调函数</param>
        public void once<T>(string eventName, Action<T> listener)
        {
            this.on(eventName, listener, 1);
        }
        /// <summary>
        /// 注册事件通知。只响应一次
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">回调函数</param>
        public void once<T, U>(string eventName, Action<T, U> listener)
        {
            this.on(eventName, listener, 1);
        }
        /// <summary>
        /// 注册事件通知。只响应一次
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">回调函数</param>
        public void once<T, U, V>(string eventName, Action<T, U, V> listener)
        {
            this.on(eventName, listener, 1);
        }
        /// <summary>
        /// 取消事件注册
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">回调函数</param>
        public void removeListener(string eventName, Delegate listener)
        {
            lock (locker)
            {
                removeListenerPrivate(eventName, listener);
            }
        }
        /// <summary>
        /// 取消事件注册
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">回调函数</param>
        public void removeListener(string eventName, Action listener)
        {
            removeListener(eventName, listener);
        }
        /// <summary>
        /// 取消事件注册
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">回调函数</param>
        public void removeListener<T>(string eventName, Action<T> listener)
        {
            removeListener(eventName, listener);
        }
        /// <summary>
        /// 取消事件注册
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">回调函数</param>
        public void removeListener<T, U>(string eventName, Action<T, U> listener)
        {
            removeListener(eventName, listener);
        }
        /// <summary>
        /// 取消事件注册
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">回调函数</param>
        public void removeListener<T, U, V>(string eventName, Action<T, U, V> listener)
        {
            removeListener(eventName, listener);
        }
        /// <summary>
        /// 取消事件注册
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">回调函数</param>
        public void removeListenerPrivate(string eventName, Delegate listener)
        {
            if (!events.ContainsKey(eventName)) return;
            List<EmitCount> list = events[eventName];
            list.RemoveAll(item => item.Event == listener);
        }
        /// <summary>
        /// 取消所有事件注册
        /// </summary>
        /// <param name="eventName">事件名称</param>
        public void removeAllListeners(string eventName)
        {
            events.Remove(eventName);
        }
        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="args">参数</param>
        public void emit(string eventName, params object[] args)
        {
            if (!events.ContainsKey(eventName)) return;
            EmitCount[] list = null;
            lock (locker)
            {
                list = events[eventName].ToArray();
            }
            foreach (var listener in list)
            {
                var types = listener.GetParameters();
                var vals = args;
                if (types.Length != args.Length)
                {
                    vals = new object[types.Length];
                    for (var i = 0; i < vals.Length; ++i)
                    {
                        if (i < args.Length) vals[i] = args[i];
                        else vals[i] = types[i].DefaultValue;
                    }
                }
                if (listener.Count != 0)
                {
                    listener.Event.DynamicInvoke(vals);
                }
                lock (locker)
                {
                    if (listener.Count > 0) --listener.Count;
                    if (listener.Count == 0) this.removeListenerPrivate(eventName, listener.Event);
                }
            }
        }
    }
}
