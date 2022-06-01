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

        /*public ByteImage ToByteImage()
        {
            ByteImage res = new ByteImage(Width, Height);
            for (int i = 0; i < res.rawdata.Length; i++)
            {
                byte c = rawdata[i] < 0.0f ? (byte)0 : rawdata[i] > 255.0f ? (byte)255 : (byte)rawdata[i];
                res.rawdata[i] = new ColorBytePixel() { b = c, g = c, r = c, a = 0 };
            }
            return res;
        }*/
    static string ReadString(Stream stream)
        {
            StringBuilder sb = new StringBuilder();
            int c1 = stream.ReadByte();
            if (c1 == -1)
                return null;

            while (true)
            {
                if (c1 == 13 || c1 == 10 || c1 == -1)
                    return sb.ToString();
                else
                    sb.Append((char)c1);

                c1 = stream.ReadByte();
            }
        }

        public static Bitmap BitmapSourceToBitmap(BitmapSource bitmapSource)
        {
            var width = bitmapSource.PixelWidth;
            var height = bitmapSource.PixelHeight;
            var stride = width * ((bitmapSource.Format.BitsPerPixel + 7) / 8);
            var memoryBlockPointer = Marshal.AllocHGlobal(height * stride);
            bitmapSource.CopyPixels(new Int32Rect(0, 0, width, height), memoryBlockPointer, height * stride, stride);
            var bitmap = new Bitmap(width, height, stride, PixelFormat.Format32bppPArgb, memoryBlockPointer);
            return bitmap;
        }
        /*static ByteImage ReadPGM(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            try
            {
                while (true)
                {
                    string str = ReadString(fs).Trim();
                    if (str == null)
                        return null;
                    else if (str == "" || str.StartsWith("#"))
                        continue;
                    else if (str != "P5")
                        return null;
                    else
                        break;
                }

                int Width = -1, Height = -1, MaxL = -1;

                while (true)
                {
                    string str = ReadString(fs).Trim();
                    if (str == null)
                        return null;
                    else if (str == "" || str.StartsWith("#"))
                        continue;
                    string[] arr = str.Split(' ', '\t');
                    Width = int.Parse(arr[0]);
                    Height = int.Parse(arr[1]);
                    break;
                }

                while (true)
                {
                    string str = ReadString(fs).Trim();
                    if (str == null)
                        return null;
                    else if (str == "" || str.StartsWith("#"))
                        continue;
                    MaxL = int.Parse(str);
                    break;
                }

                ByteImage res = new ByteImage(Width, Height);

                if (MaxL <= 255)
                {
                    byte[] raw = new byte[Width * Height];
                    fs.Read(raw, 0, raw.Length);
                    for (int j = 0; j < Height; j++)
                        for (int i = 0; i < Width; i++)
                            res[i, j] = raw[j * Width + i];
                }
                else
                {
                    byte[] raw = new byte[Width * Height * 2];
                    fs.Read(raw, 0, raw.Length * 2);
                    for (int j = 0; j < Height; j++)
                        for (int i = 0; i < Width; i++)
                            res[i, j] = raw[(j * Width + i) * 2] + raw[(j * Width + i) * 2 + 1] * 255;
                }

                return res;
            }
            finally
            {
                fs.Close();
            }
        }*/

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
                        byte c = image[i, j] < 0.0f ? (byte)0 : image[i, j] > 255.0f ? (byte)255 : (byte)image[i, j];
                        lbi.data[lbi.linewidth * j + i * 4] = c;
                        lbi.data[lbi.linewidth * j + i * 4 + 1] = c;
                        lbi.data[lbi.linewidth * j + i * 4 + 2] = c;
                    }
            }
            finally
            {
                UnlockBitmap(lbi);
            }

            return B;
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
                        for (int i = 0; i < W; i++)
                        {
                            int c = lbi.data[lbi.linewidth * j + i];
                            int b = pal[c * 4];
                            int g = pal[c * 4 + 1];
                            int r = pal[c * 4 + 2];
                            res[i, j] = (byte)(0.114f * b + 0.587f * g + 0.299f * r);
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
                        for (int i = 0; i < W; i++)
                        {
                            int b = lbi.data[lbi.linewidth * j + i * 4];
                            int g = lbi.data[lbi.linewidth * j + i * 4 + 1];
                            int r = lbi.data[lbi.linewidth * j + i * 4 + 2];
                            res[i, j] = (byte)(0.114f * b + 0.587f * g + 0.299f * r);
                        }
                }
                finally
                {
                    UnlockBitmap(lbi);
                }
            }

            return res;
        }

        /*public static ByteImage BitmapToColorByteImage(Bitmap B)
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
                        for (int i = 0; i < W; i++)
                        {
                            int c = lbi.data[lbi.linewidth * j + i];
                            byte b = pal[c * 4];
                            byte g = pal[c * 4 + 1];
                            byte r = pal[c * 4 + 2];
                            res[i, j] = new ColorBytePixel() { b = b, g = g, r = r, a = 255 };
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
                        for (int i = 0; i < W; i++)
                        {
                            byte b = lbi.data[lbi.linewidth * j + i * 4];
                            byte g = lbi.data[lbi.linewidth * j + i * 4 + 1];
                            byte r = lbi.data[lbi.linewidth * j + i * 4 + 2];
                            res[i, j] = new ColorBytePixel() { b = b, g = g, r = r, a = 255 };
                        }
                }
                finally
                {
                    UnlockBitmap(lbi);
                }
            }

            return res;
        }*/

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
