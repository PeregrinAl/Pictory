using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pictory
{
    public static class Convolution
    {
        public static Bitmap Apply(Bitmap input, double[,] kernel)
        {
            //Получаем байты изображения
            
            byte[] inputBytes = ImageDispatcher.BitmapToByteArray(input);
            PixelFormat pixelFormat = input.PixelFormat;
            byte[] outputBytes = new byte[inputBytes.Length];

            int width = input.Width;
            int height = input.Height;

            int kernelWidth = kernel.GetLength(0);
            int kernelHeight = kernel.GetLength(1);

            //Производим вычисления
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    double rSum = 0, gSum = 0, bSum = 0, kSum = 0, aSum = 0;

                    for (int i = 0; i < kernelWidth; i++)
                    {
                        for (int j = 0; j < kernelHeight; j++)
                        {
                            int pixelPosX = x + (i - (kernelWidth / 2));
                            int pixelPosY = y + (j - (kernelHeight / 2));
                            if ((pixelPosX < 0) ||
                              (pixelPosX >= width) ||
                              (pixelPosY < 0) ||
                              (pixelPosY >= height)) continue;

                            byte r = inputBytes[4 * (width * pixelPosY + pixelPosX) + 0];
                            byte g = inputBytes[4 * (width * pixelPosY + pixelPosX) + 1];
                            byte b = inputBytes[4 * (width * pixelPosY + pixelPosX) + 2];
                            byte a = inputBytes[4 * (width * pixelPosY + pixelPosX) + 3];

                            double kernelVal = kernel[i, j];

                            rSum += r * kernelVal;
                            gSum += g * kernelVal;
                            bSum += b * kernelVal;
                            aSum += a * kernelVal;

                            kSum += kernelVal;
                        }
                    }

                    if (kSum <= 0) kSum = 1;

                    //Контролируем переполнения переменных
                    rSum /= kSum;
                    if (rSum < 0) rSum = 0;
                    if (rSum > 255) rSum = 255;

                    gSum /= kSum;
                    if (gSum < 0) gSum = 0;
                    if (gSum > 255) gSum = 255;

                    bSum /= kSum;
                    if (bSum < 0) bSum = 0;
                    if (bSum > 255) bSum = 255;

                    aSum /= kSum;
                    if (aSum < 0) aSum = 0;
                    if (aSum > 255) aSum = 255;

                    //Записываем значения в результирующее изображение
                    outputBytes[4 * (width * y + x) + 0] = (byte)rSum;
                    outputBytes[4 * (width * y + x) + 1] = (byte)gSum;
                    outputBytes[4 * (width * y + x) + 2] = (byte)bSum;
                    outputBytes[4 * (width * y + x) + 3] = 255;
                }
            }
            //Возвращаем отфильтрованное изображение
            return ImageDispatcher.GetBitmap(outputBytes, width, height);
        }
    }
}
