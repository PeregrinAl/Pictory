using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pictory
{
    public partial class Base
    {
        public static ByteImage FlipByteImage(ByteImage image)
        {
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width / 2; x++)
                {
                    ColorBytePixel p = image[x, y];
                    image[x, y] = image[image.Width - 1 - x, y];
                    image[image.Width - 1 - x, y] = p;
                }
            }
            return image;
        }
    }
}
