using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Pictory
{
    public unsafe class ImageDispatcher
    {
        public static LockBitmapInfo LockBitmap(Bitmap B)
        {
            return LockBitmap(B, PixelFormat.Format32bppRgb, 4);
        }

        public static Bitmap BitmapSourceToBitmap(BitmapSource bitmapSource)
        {
            int width = bitmapSource.PixelWidth;
            int height = bitmapSource.PixelHeight;
            int stride = width * ((bitmapSource.Format.BitsPerPixel + 7) / 8);
            var memoryBlockPointer = Marshal.AllocHGlobal(height * stride);
            bitmapSource.CopyPixels(new Int32Rect(0, 0, width, height), memoryBlockPointer, height * stride, stride);
            var bitmap = new Bitmap(width, height, stride, PixelFormat.Format32bppPArgb, memoryBlockPointer);
            return bitmap;
        }

        public static LockBitmapInfo LockBitmap(Bitmap B, PixelFormat pf, int pixelsize)
        {
            LockBitmapInfo lbi;
            GraphicsUnit unit = GraphicsUnit.Pixel;
            RectangleF boundsF = B.GetBounds(ref unit);
            Rectangle bounds = new Rectangle((int)boundsF.X,
                (int)boundsF.Y,
                (int)boundsF.Width,
                (int)boundsF.Height);
            lbi.B = B;
            lbi.Width = (int)boundsF.Width;
            lbi.Height = (int)boundsF.Height;
            lbi.bitmapData = B.LockBits(bounds, ImageLockMode.ReadWrite, pf);
            lbi.linewidth = lbi.bitmapData.Stride;
            lbi.data = (byte*)lbi.bitmapData.Scan0.ToPointer();
            return lbi;
        }

        public static void UnlockBitmap(LockBitmapInfo lbi)
        {
            lbi.B.UnlockBits(lbi.bitmapData);
            lbi.bitmapData = null;
            lbi.data = null;
        }

        public static Bitmap ImageToBitmap(ByteImage image)
        {
            Bitmap B = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb);

            LockBitmapInfo lbi = LockBitmap(B);
            try
            {
                for (int j = 0; j < image.Height; j++)
                    for (int i = 0; i < image.Width; i++)
                    {
                        ColorBytePixel p = image[i, j];
                        lbi.data[lbi.linewidth * j + i * 4] = p.b < 0.0f ? (byte)0 : p.b > 255.0f ? (byte)255 : (byte)p.b;
                        lbi.data[lbi.linewidth * j + i * 4 + 1] = p.g < 0.0f ? (byte)0 : p.g > 255.0f ? (byte)255 : (byte)p.g;
                        lbi.data[lbi.linewidth * j + i * 4 + 2] = p.r < 0.0f ? (byte)0 : p.r > 255.0f ? (byte)255 : (byte)p.r;
                    }
            }
            finally
            {
                UnlockBitmap(lbi);
            }

            return B;
        }

        internal static Bitmap GetBitmap(byte[] bytes, int width, int height)
        {
            int ch = 4; //number of channels (ie. assuming 24 bit RGB in this case)

            byte[] imageData = new byte[width * height * ch]; //you image data here
            imageData = bytes;
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData bmData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            IntPtr pNative = bmData.Scan0;
            Marshal.Copy(imageData, 0, pNative, width * height * ch);
            bmData.Scan0 = pNative;
            return bitmap;
        }

        public static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        public static ByteImage BitmapToByteImage(Bitmap B)
        {
            int W = B.Width, H = B.Height;
            ByteImage res = new ByteImage(W, H);

            if (B.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                Color[] pi = B.Palette.Entries;
                byte[] pal = new byte[1024];
                for (int i = 0; i < pi.Length; i++)
                {
                    Color C = pi[i];
                    pal[i * 4] = C.B;
                    pal[i * 4 + 1] = C.G;
                    pal[i * 4 + 2] = C.R;
                    pal[i * 4 + 3] = C.A;
                }

                LockBitmapInfo lbi = LockBitmap(B, PixelFormat.Format8bppIndexed, 1);
                try
                {
                    for (int j = 0; j < H; j++)
                    {
                        for (int i = 0; i < W; i++)
                        {
                            int c = lbi.data[lbi.linewidth * j + i];
                            byte b = pal[c * 4];
                            byte g = pal[c * 4 + 1];
                            byte r = pal[c * 4 + 2];
                            res[i, j] = new ColorBytePixel() { b = b, g = g, r = r, a = 255 };
                        }
                    }
                }
                finally
                {
                    UnlockBitmap(lbi);
                }
            }
            else
            {
                LockBitmapInfo lbi = LockBitmap(B);
                try
                {
                    for (int j = 0; j < H; j++)
                    {
                        for (int i = 0; i < W; i++)
                        {
                            byte b = lbi.data[lbi.linewidth * j + i * 4];
                            byte g = lbi.data[lbi.linewidth * j + i * 4 + 1];
                            byte r = lbi.data[lbi.linewidth * j + i * 4 + 2];
                            res[i, j] = res[i, j] = new ColorBytePixel() { b = b, g = g, r = r, a = 255 };
                        }
                    }
                }
                finally
                {
                    res.bitmap = B;
                    UnlockBitmap(lbi);
                }
            }

            return res;
        }

        public static ByteImage BitmapToColorByteImage(Bitmap B)
        {
            int W = B.Width, H = B.Height;
            ByteImage res = new ByteImage(W, H);

            if (B.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                Color[] pi = B.Palette.Entries;
                byte[] pal = new byte[1024];
                for (int i = 0; i < pi.Length; i++)
                {
                    Color C = pi[i];
                    pal[i * 4] = C.B;
                    pal[i * 4 + 1] = C.G;
                    pal[i * 4 + 2] = C.R;
                    pal[i * 4 + 3] = C.A;
                }

                LockBitmapInfo lbi = LockBitmap(B, PixelFormat.Format8bppIndexed, 1);
                try
                {
                    for (int j = 0; j < H; j++)
                    {
                        for (int i = 0; i < W; i++)
                        {
                            int c = lbi.data[lbi.linewidth * j + i];
                            byte b = pal[c * 4];
                            byte g = pal[c * 4 + 1];
                            byte r = pal[c * 4 + 2];
                            res[i, j] = new ColorBytePixel() { b = b, g = g, r = r, a = 255 };
                        }
                    }
                }
                finally
                {
                    UnlockBitmap(lbi);
                }
            }
            else
            {
                LockBitmapInfo lbi = LockBitmap(B);
                try
                {
                    for (int j = 0; j < H; j++)
                    {
                        for (int i = 0; i < W; i++)
                        {
                            byte b = lbi.data[lbi.linewidth * j + i * 4];
                            byte g = lbi.data[lbi.linewidth * j + i * 4 + 1];
                            byte r = lbi.data[lbi.linewidth * j + i * 4 + 2];
                            res[i, j] = new ColorBytePixel() { b = b, g = g, r = r, a = 255 };
                        }
                    }
                }
                finally
                {
                    UnlockBitmap(lbi);
                }
            }

            return res;
        }

        internal static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            BitmapData bmpdata = null;

            try
            {
                bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                int numbytes = bmpdata.Stride * bitmap.Height;
                byte[] bytedata = new byte[numbytes];
                IntPtr ptr = bmpdata.Scan0;

                Marshal.Copy(ptr, bytedata, 0, numbytes);

                return bytedata;
            }
            finally
            {
                if (bmpdata != null)
                    bitmap.UnlockBits(bmpdata);
            }
        }
    }
    
    public unsafe struct LockBitmapInfo
    {
        public Bitmap B;
        public int linewidth;
        public BitmapData bitmapData;
        public byte* data;
        public int Width, Height;
    }
}
