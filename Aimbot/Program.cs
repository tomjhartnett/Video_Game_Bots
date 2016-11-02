using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Aimbot
{
    class Program
    {
        public static Win32.POINT x1;
        private static readonly Stopwatch sw = new Stopwatch();
        private static int xpos;
        private static int ypos;
        private static int xposchange = 0;
        private static int yposchange = 0;
        private static bool[] arr;
        private static Random rand = new Random();
        static void Main(string[] args)
        {
            int width = 10;
            while (true)
            {
                x1 = new Win32.POINT();
                x1.x = Convert.ToInt16(Cursor.Position.X);
                x1.y = Convert.ToInt16(Cursor.Position.Y);

                //Win32.SetCursorPos(x1.x, x1.y);
                //sw.Restart();
                //average time of below two functions is about 70-100 ms, 99% of that is the screen capture
                xpos = Cursor.Position.X;
                xposchange = 0;
                ypos = Cursor.Position.Y;
                yposchange = 0;
                Bitmap b = CaptureImage();

                ProcessUsingLockbitsAndUnsafeAndParallel(b);
                //sw.Stop();
                //Console.WriteLine(sw.ElapsedMilliseconds);


                //int counter = 0;
                //foreach (bool b in testc)
                //    if (b)
                //        counter++;
                //if (counter > 0)
                //{
                //    Cursor.Hide();
                //    DoMouseClick();
                //    Thread.Sleep(500);
                //    Cursor.Show();
                //}
            }

            
        }

        private static Bitmap CaptureImage()
        {
            Bitmap b = new Bitmap(60, 1);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.CopyFromScreen(x1.x - 60 / 2, x1.y, 0, 0, b.Size, CopyPixelOperation.SourceCopy);
            }
            return b;
        }

        public static bool isRed(Color c)
        {
            return (c.R > 130 && c.B < 100 && c.G < 100); ;
        }
        public static bool isRed(int red, int blue, int green)
        {
            return (red > 130 && blue < 100 && green < 100); ;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        public static void DoMouseClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }
        public class Win32
        {
            [DllImport("User32.Dll")]
            public static extern long SetCursorPos(int x, int y);

            [DllImport("User32.Dll")]
            public static extern bool ClientToScreen(IntPtr hWnd, ref POINT point);

            [StructLayout(LayoutKind.Sequential)]
            public struct POINT
            {
                public int x;
                public int y;
            }
        }

        public static bool isWithin(Point p)
        {
            //if
            return false;
        }

        /*Note unsafe keyword*/
        private static void ProcessUsingLockbitsAndUnsafeAndParallel(Bitmap processedBitmap)
        {
            unsafe
            {
                BitmapData bitmapData = processedBitmap.LockBits(new Rectangle(0, 0, 10, 1), ImageLockMode.ReadOnly, processedBitmap.PixelFormat);

                int bytesPerPixel = System.Drawing.Bitmap.GetPixelFormatSize(processedBitmap.PixelFormat) / 8;
                int heightInPixels = 1;
                int widthInBytes = 10 * bytesPerPixel;
                byte* PtrFirstPixel = (byte*)bitmapData.Scan0;

                arr = new bool[60];
                Parallel.For(0, heightInPixels, y =>
                {
                    byte* currentLine = PtrFirstPixel + (y * bitmapData.Stride);
                    //bool canFire = false;
                    for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        int oldBlue = currentLine[x];
                        int oldGreen = currentLine[x + 1];
                        int oldRed = currentLine[x + 2];

                        currentLine[x] = (byte)oldBlue;
                        currentLine[x + 1] = (byte)oldGreen;
                        currentLine[x + 2] = (byte)oldRed;

                        //get outline
                        //if mouse is in outline, fire
                        if (isRed(oldRed, oldBlue, oldGreen))
                            arr[x/bytesPerPixel] = true;

                    }
                    if (xpos != Cursor.Position.X)
                        xposchange = Cursor.Position.X - xpos;
                    if (ypos != Cursor.Position.Y)
                        xposchange = Cursor.Position.Y - ypos;  dfgh
                    bool redLeft = false;
                    bool redright = false;
                    int stop = arr.Length / 2 + xposchange;
                    if (stop > arr.Length - 1)
                        stop = arr.Length - 1;
                    if (stop < 0)
                        stop = 0;
                    //Console.WriteLine(stop);
                    for (int i = 0; i < stop; i++)
                        if (arr[i])
                        {

                            redLeft = true;
                        }
                    stop = arr.Length / 2 - xposchange;
                    if (stop > arr.Length - 1)
                        stop = arr.Length - 1;
                    if (stop < 0)
                        stop = 0;
                    for (int i = arr.Length - 1; i > stop; i--)
                        if (arr[i])
                        {
                            redright = true;
                            Console.WriteLine("Got right");
                        }
                    if (redLeft && redright)
                    {
                        DoMouseClick();
                        Thread.Sleep(rand.Next(500, 550));
                    }
                });
                processedBitmap.UnlockBits(bitmapData);
            }
        }

    }

}
