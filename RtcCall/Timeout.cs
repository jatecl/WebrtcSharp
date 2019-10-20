using System;

namespace Relywisdom
{
    /// <summary>
    /// js风格的定时器
    /// </summary>
    class Timeout
    {
        private System.Threading.Timer timer;

        /// <summary>
        /// 开始倒计时
        /// </summary>
        /// <param name="p">回调函数</param>
        /// <param name="v">时长</param>
        public Timeout(Action p, int v, bool once)
        {
            timer = new System.Threading.Timer(obj =>
            {
                if (once) timer.Dispose();
                p();
            }, null, v, v);
        }
        /// <summary>
        /// 开始倒计时
        /// </summary>
        /// <param name="p">回调函数</param>
        /// <param name="v">时长</param>
        public static Timeout setTimeout(Action p, int v)
        {
            return new Timeout(p, v, true);
        }
        /// <summary>
        /// 开始倒计时
        /// </summary>
        /// <param name="p">回调函数</param>
        /// <param name="v">时长</param>
        public static Timeout setInterval(Action p, int v)
        {
            return new Timeout(p, v, false);
        }
        /// <summary>
        /// 取消倒计时
        /// </summary>
        public void clearTimeout()
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
        }
        /// <summary>
        /// 取消倒计时
        /// </summary>
        public void clearInterval()
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
        }
    }
}
