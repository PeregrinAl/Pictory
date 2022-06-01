using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pictory
{
    public struct ColorBytePixel
    {
        public byte b, g, r, a;
    }
    public unsafe class ByteImage
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public readonly ColorBytePixel[] rawdata;
        public byte[] byteArr;
        public Bitmap bitmap;

        public ByteImage(int Width, int Height)
        {
            this.Width = Width;
            this.Height = Height;
            rawdata = new ColorBytePixel[Width * Height];
        }

        public ByteImage(int Width, int Height, Bitmap bitmap)
        {
            this.Width = Width;
            this.Height = Height;
            this.bitmap = bitmap;
            rawdata = new ColorBytePixel[Width * Height];
            byteArr = new byte[Width * Height  * 4];
        }

        public ColorBytePixel this[int x, int y]
        {
            get
            {
#if DEBUG
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                    throw new IndexOutOfRangeException(string.Format("Trying to access pixel ({0}, {1}) in {2}x{3} image", x, y, Width, Height));
#endif
                return rawdata[y * Width + x];
            }
            set
            {
#if DEBUG
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                    throw new IndexOutOfRangeException(string.Format("Trying to access pixel ({0}, {1}) in {2}x{3} image", x, y, Width, Height));
#endif
                rawdata[y * Width + x] = value;
            }
        }
    }
}
