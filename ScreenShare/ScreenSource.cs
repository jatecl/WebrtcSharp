using Relywisdom;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebrtcSharp;

namespace ScreenShare
{
    class ScreenSource : ILocalMediaSource
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr ptr);
        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        [DllImport("user32.dll")]
        private static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);

        const int HORZRES = 118;
        const int VERTRES = 117;

        private Thread thread;

        public ScreenSource() : base("video")
        {
            this.on<bool>("changed", this._changed);
            this.thread = new Thread(this._runing);
            this.thread.Start();
        }

        private bool _captching = false;
        private bool _working = true;
        private FrameVideoSource videoSource = new FrameVideoSource();

        unsafe byte[] bitmapToYUV420(Bitmap scaled)
        {
            int width = scaled.Width;
            int height = scaled.Height;
            byte[] yuv420sp = new byte[width * height * 3 / 2];

            int uIndex = width * height;
            int vIndex = uIndex * 5 / 4;

            int R, G, B;
            int Y, U, V;

            BitmapData data = scaled.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            byte* ptr = (byte*)data.Scan0.ToPointer();

            for (var j = 0; j < height; ++j)
            {
                var sj = j * width;
                var start = sj * 3;
                var uvsj = sj / 4;
                var next = (j + 1) * width * 3;
                var isj = j % 2 == 0;
                var isn = j + 1 >= height;
                for (int i = 0; i < width; i++)
                {
                    var index = i * 3;
                    R = ptr[start + index];
                    G = ptr[start + index + 1];
                    B = ptr[start + index + 2];

                    Y = ((66 * R + 129 * G + 25 * B + 128) >> 8) + 16;

                    Y = Math.Max(0, Math.Min(255, Y));

                    yuv420sp[sj + i] = (byte)Y;
                    if (isj && i % 2 == 0)
                    {
                        R += ptr[start + index + 3];
                        G += ptr[start + index + 4];
                        B += ptr[start + index + 5];
                        if (isn)
                        {
                            R += ptr[next + index];
                            G += ptr[next + index + 1];
                            B += ptr[next + index + 2];
                            R += ptr[next + index + 3];
                            G += ptr[next + index + 4];
                            B += ptr[next + index + 5];

                            R /= 4;
                            G /= 4;
                            B /= 4;
                        }
                        else
                        {
                            R /= 2;
                            G /= 2;
                            B /= 2;
                        }

                        V = ((-38 * R - 74 * G + 112 * B + 128) >> 8) + 128;
                        U = ((112 * R - 94 * G - 18 * B + 128) >> 8) + 128;
                        U = Math.Max(0, Math.Min(255, U));
                        V = Math.Max(0, Math.Min(255, V));

                        var uvi = uvsj + i / 2;
                        var ui = uvi + uIndex;
                        if (ui < vIndex)
                        {
                            yuv420sp[ui] = (byte)(U);
                            var vi = uvi + vIndex;
                            yuv420sp[vi] = (byte)(V);
                        }
                    }
                }
            }

            scaled.UnlockBits(data);

            return yuv420sp;
        }

        private void _runing()
        {
            while (_working)
            {
                if (_captching)
                {
                    IntPtr hdc = GetDC(IntPtr.Zero);
                    Size size = new Size();
                    size.Width = GetDeviceCaps(hdc, HORZRES);
                    size.Height = GetDeviceCaps(hdc, VERTRES);
                    ReleaseDC(IntPtr.Zero, hdc);

                    using (Bitmap bmp = new Bitmap(size.Width, size.Height))
                    {
                        using (Graphics graphics = Graphics.FromImage(bmp))
                        {
                            graphics.CopyFromScreen(0, 0, 0, 0, size);
                        }
                        SendFrame(bmp);
                    }

                }
                Thread.Sleep(40);
            }
        }

        private unsafe void SendFrame(Bitmap bmp)
        {
            var yuv = bitmapToYUV420(bmp);
            fixed (byte* ptr = yuv)
            {
                var y = new IntPtr(ptr);
                var u = new IntPtr(ptr + (bmp.Width * bmp.Height));
                var v = new IntPtr(ptr + (bmp.Width * bmp.Height * 5 / 4));
                videoSource.SendFrame(new VideoFrame(
                    y, u, v, IntPtr.Zero,
                    bmp.Width, bmp.Width / 2, bmp.Width / 2, 0,
                    bmp.Width, bmp.Height,
                    0, -1));
            }
        }

        private void _changed(bool ok)
        {
            _captching = ok;
        }

        public override async Task<bool> open()
        {
            this._track = RtcNavigator.createVideoTrack(videoSource);
            this.emit("changed", true);
            return true;
        }

        public override async Task<bool> usable()
        {
            return true;
        }

        protected override void _justClose(bool nochange = false)
        {
            _working = false;
            base._justClose(nochange);
        }
    }
}
