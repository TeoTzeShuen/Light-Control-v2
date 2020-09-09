using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace WifiLedController
{
    internal class ScreenProcessor
    {
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(ref Point lpPoint);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

        public Point MouseLocPos()
        {
            Point cursor = new Point();
            GetCursorPos(ref cursor);
            // Debug.WriteLine("x={0} y={1}", cursor.X, cursor.Y);
            return cursor;
        }

        public Color MouseLocColor()
        {
            Point cursor = new Point();
            GetCursorPos(ref cursor);
            //Debug.WriteLine("x={0} y={1}",cursor.X,cursor.Y);
            return GetColorAt(cursor);
        }

        public (Color, Point) MouseLocPosColor()
        {
            Point cursor = new Point();
            GetCursorPos(ref cursor);
            //Debug.WriteLine("x={0} y={1}",cursor.X,cursor.Y);
            return (GetColorAt(cursor), cursor);
        }

        //https://stackoverflow.com/questions/1483928/how-to-read-the-color-of-a-screen-pixel
        //Gets color at point, actually working.
        public Color GetColorAt(Point location)
        {
            int xwidth = 1;
            int yheight = 1;
            Bitmap screenPixel = new Bitmap(xwidth, yheight, PixelFormat.Format32bppArgb);
            using (Graphics gdest = Graphics.FromImage(screenPixel))
            {
                using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();

                    int retval = BitBlt(hDC, 0, 0, xwidth, yheight, hSrcDC, location.X, location.Y, (int)CopyPixelOperation.SourceCopy);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }
            //            for (int i = 0; i < yheight; i++) {
            //                for (int j = 0; j < xwidth; j++) {
            //                    Debug.WriteLine("{0},{1} Color = {2}", i, j, screenPixel.GetPixel(j, i));
            //                }
            //            }
            return screenPixel.GetPixel(0, 0);
        }

        public Color AverageColorOfArea(int x, int y, int sizeX, int sizeY)
        {
            throw new NotImplementedException();
        }

        public Color GetAverageColorSection(int x, int y, int xwidth, int yheight, int xstride = 1, int ystride = 1, int red = 0, int green = 0, int blue = 0)
        {
            Bitmap screenPixels = new Bitmap(xwidth, yheight, PixelFormat.Format32bppArgb);
            using (Graphics gdest = Graphics.FromImage(screenPixels))
            {
                using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();

                    int retval = BitBlt(hDC, 0, 0, xwidth, yheight, hSrcDC, x, y, (int)CopyPixelOperation.SourceCopy);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }
            int r = 0, g = 0, b = 0;
            Color temp;

            for (int i = 0; i < yheight; i = i + ystride)
            {
                for (int j = 0; j < xwidth; j = j + xstride)
                {
                    //Debug.WriteLine("{0},{1} Color = {2}", i, j, screenPixel.GetPixel(j, i));
                    //https://sighack.com/post/averaging-rgb-colors-the-right-way
                    temp = screenPixels.GetPixel(j, i);
                    r += temp.R * temp.R;
                    g += temp.G * temp.G;
                    b += temp.B * temp.B;
                }
            }
            //Remember to dispose your pixels when you are done... Massive memory leak if you don't
            screenPixels.Dispose();
            //Cast r to double to more accurately divide and square root. Then back to int for the final part
            double points = Math.Truncate((double)xwidth / (double)xstride) * Math.Truncate((double)yheight / (double)ystride);
            r = Math.Max(Math.Min((int)Math.Sqrt((double)r / points) + red, 255), 0);
            g = Math.Max(Math.Min((int)Math.Sqrt((double)g / points) + green, 255), 0);
            b = Math.Max(Math.Min((int)Math.Sqrt((double)b / points) + blue, 255), 0);

            Debug.WriteLine("[SP]Average values r={0},g={1},b={2}", r, g, b);
            return Color.FromArgb(r, g, b);
        }

        public Color GetAverageColorSectionMulti(int x, int y, int xwidth, int yheight, int xstride = 1, int ystride = 1, float red = 1, float green = 1, float blue = 1)
        {
            Bitmap screenPixels = new Bitmap(xwidth, yheight, PixelFormat.Format32bppArgb);
            using (Graphics gdest = Graphics.FromImage(screenPixels))
            {
                using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();

                    int retval = BitBlt(hDC, 0, 0, xwidth, yheight, hSrcDC, x, y, (int)CopyPixelOperation.SourceCopy);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }
            int r = 0, g = 0, b = 0;
            Color temp;

            for (int i = 0; i < yheight; i = i + ystride)
            {
                for (int j = 0; j < xwidth; j = j + xstride)
                {
                    //Debug.WriteLine("{0},{1} Color = {2}", i, j, screenPixel.GetPixel(j, i));
                    //https://sighack.com/post/averaging-rgb-colors-the-right-way
                    temp = screenPixels.GetPixel(j, i);

                    r += temp.R * temp.R;
                    g += temp.G * temp.G;
                    b += temp.B * temp.B;
                }
            }
            //Remember to dispose your pixels when you are done... Massive memory leak if you don't
            screenPixels.Dispose();
            //Cast r to double to more accurately divide and square root. Then back to int for the final part
            double points = Math.Truncate((double)xwidth / (double)xstride) * Math.Truncate((double)yheight / (double)ystride);
            r = (int)Math.Max(Math.Min(Math.Sqrt(r / points) * red, 255), 0);
            g = (int)Math.Max(Math.Min(Math.Sqrt(g / points) * green, 255), 0);
            b = (int)Math.Max(Math.Min(Math.Sqrt(b / points) * blue, 255), 0);

            Debug.WriteLine("[SP][Mult]Average values r={0},g={1},b={2}", r, g, b);
            return Color.FromArgb(r, g, b);
        }
    }
}