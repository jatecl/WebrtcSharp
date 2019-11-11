using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using WebrtcSharp;

namespace WebCamera
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            OpenCamera();
        }

        private PeerConnectionFactory factory = new PeerConnectionFactory();

        private void OpenCamera()
        {
            source?.Release();
            source = factory.CreateVideoSource(cameraIndex, 1600, 1200, 30);
            if (source == null) return;
            source.Frame += Source_Frame;
        }

        [DllImport("gdi32.dll")]
        private extern static bool DeleteObject(IntPtr hObject);

        private void Source_Frame(VideoFrame obj)
        {
            var bitmap = YUV420ToBitmap(obj);
            Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    IntPtr myImagePtr = bitmap.GetHbitmap();
                    try
                    {
                        var imgsource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(myImagePtr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());  //创建imgSource
                        display.Source = imgsource;
                    }
                    finally
                    {
                        if (myImagePtr != IntPtr.Zero) DeleteObject(myImagePtr);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    bitmap.Dispose();
                }
            }));
        }
        unsafe Bitmap YUV420ToBitmap(VideoFrame scaled)
        {
            int width = scaled.Width;
            int height = scaled.Height;
            byte* ydata = (byte*)scaled.DataY.ToPointer();
            byte* udata = (byte*)scaled.DataU.ToPointer();
            byte* vdata = (byte*)scaled.DataV.ToPointer();

            int Y, U, V;

            var bmp = new Bitmap(width, height);

            BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            byte* ptr = (byte*)data.Scan0.ToPointer();

            for (var j = 0; j < height; ++j)
            {
                var sj = j * width;
                var ssj = (j / 2) * (width / 2);
                var start = sj * 3;
                for (int i = 0; i < width; i++)
                {
                    var si = ssj + i / 2;
                    Y = ydata[sj + i];
                    U = udata[si];
                    V = vdata[si];
                    var index = i * 3;
                    ptr[start + index + 2] = (byte)Math.Max(0, Math.Min(255, (int)Math.Round(Y + 1.4075 * (V - 128))));
                    ptr[start + index + 1] = (byte)Math.Max(0, Math.Min(255, (int)Math.Round(Y - 0.3455 * (U - 128) - 0.7169 * (V - 128))));
                    ptr[start + index + 0] = (byte)Math.Max(0, Math.Min(255, (int)Math.Round(Y + 1.779 * (U - 128))));
                    if (j == height - 1 && i == width - 1)
                    {
                        break;
                    }
                }
            }
            bmp.UnlockBits(data);

            return bmp;
        }

        private VideoSource source;

        private int cameraIndex = 0;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var devices = PeerConnectionFactory.GetDeviceInfo();
            ++cameraIndex;
            if (cameraIndex >= devices.Length) cameraIndex = 0;
            OpenCamera();
        }
    }
}
