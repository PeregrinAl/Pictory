using FirstFloor.ModernUI.Presentation;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Pictory
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Uri fileName;
        public ImageSourceConverter imageSourceConverter = new ImageSourceConverter();
        public System.Windows.Point cropStartPoint;
        public System.Windows.Point cropEndPoint;
        public bool enabled = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image File (*.bmp, *.jpg, *.png) | *.bmp; *.jpg; *.png";
            if (openFileDialog.ShowDialog() == true)
            {
                mainImage.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                fileName = new Uri(openFileDialog.FileName);
                BitmapImage bitmapImage = new BitmapImage();
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) {
                this.DragMove();
            }
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            ImageFileService fileService = new ImageFileService();
            DefaultDialogService dialogService = new DefaultDialogService();
            try
            {
                if (dialogService.SaveFileDialog() == true)
                {
                    switch (TypeOfImage.SelectedIndex) {
                        case 0:
                            fileService.SaveToJpeg(mainImage, dialogService.FilePath + ".jpg");
                            dialogService.ShowMessage("Файл сохранен");
                            break;
                        case 1:
                            fileService.SaveToPng(mainImage, dialogService.FilePath+ ".png");
                            dialogService.ShowMessage("Файл сохранен");
                            break;
                        case 2:
                            fileService.SaveToBmp(mainImage, dialogService.FilePath + ".bmp");
                            dialogService.ShowMessage("Файл сохранен");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                dialogService.ShowMessage(ex.Message);
            }
        }

        private void Rotation_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage bitmapImage = mainImage.Source as BitmapImage;
            TransformedBitmap transformBmp = new TransformedBitmap();

            /*Bitmap newBitmapImage = ImageDispatcher.ImageToBitmap(Base.FlipByteImage(ImageDispatcher.BitmapToByteImage(ImageDispatcher.BitmapSourceToBitmap(bitmapImage))));

            mainImage.Source = ImageDispatcher.BitmapToImageSource(newBitmapImage);*/
            transformBmp.BeginInit();

            transformBmp.Source = (BitmapSource)mainImage.Source;

            RotateTransform transform = new RotateTransform(90);

            transformBmp.Transform = transform;

            transformBmp.EndInit();



            // Set Image.Source to TransformedBitmap

            mainImage.Source = transformBmp;

        }

        private void Image_Click(object sender, MouseButtonEventArgs e)
        {
            cropStartPoint = e.GetPosition(mainImage);
        }

        private void Image_UnClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                cropEndPoint = e.GetPosition(mainImage);
                BitmapSource tempSource = (BitmapSource)mainImage.Source;
                CroppedBitmap cb = new CroppedBitmap(tempSource,
                            new Int32Rect((int)cropStartPoint.X, (int)cropStartPoint.Y,
                                 tempSource.PixelWidth - (int)cropStartPoint.X - (tempSource.PixelWidth - (int)cropEndPoint.X),
                               tempSource.PixelHeight - (int)cropStartPoint.Y - (tempSource.PixelHeight - (int)cropEndPoint.Y)));
                mainImage.Source = cb;
            }
            catch (Exception) { 
            
            }
        }

        private void NoiseRemover_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource image = (BitmapSource)mainImage.Source;

            int[,] arrR = new int[image.PixelWidth, image.PixelHeight];
            int[,] arrG = new int[image.PixelWidth, image.PixelHeight];
            int[,] arrB = new int[image.PixelWidth, image.PixelHeight];
            Bitmap image2 = ImageDispatcher.BitmapSourceToBitmap(image);

            for (int i = 0; i < image2.Width; i++)
            {
                for (int j = 0; j < image2.Height; j++)
                {
                    arrR[i, j] = image2.GetPixel(i, j).R;
                    arrG[i, j] = image2.GetPixel(i, j).G;
                    arrB[i, j] = image2.GetPixel(i, j).B;
                }
            }

            for (int i = 1; i < image2.Width - 1; i++)
            {
                for (int j = 1; j < image2.Height - 1; j++)
                {
                    int arrRSum = 0, arrGSum = 0, arrBSum = 0;
                    int arrsrR = 0, arrsrG = 0, arrsrB = 0;
                    for (int x = -1; x < 2; x++)
                    {
                        for (int y = -1; y < 2; y++)
                        {
                            arrRSum = arrRSum + arrR[i + x, j + y];
                            arrGSum = arrGSum + arrG[i + x, j + y];
                            arrBSum = arrBSum + arrB[i + x, j + y];

                        }
                    }
                    arrsrR = arrRSum / 9;
                    arrsrG = arrGSum / 9;
                    arrsrB = arrBSum / 9;
                    image2.SetPixel(i, j, System.Drawing.Color.FromArgb(arrsrR, arrsrG, arrsrB));

                }
            }
            mainImage.Source = ImageDispatcher.BitmapToImageSource(image2);
        }
        private void ApplyEffect(Window win)
        {
            System.Windows.Media.Effects.BlurEffect objBlur = new System.Windows.Media.Effects.BlurEffect();
            objBlur.Radius = 20;
            win.Effect = objBlur;
        }
        private void ClearEffect(Window win)
        {
            win.Effect = null;
        }

        private void brightnessPercentage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BitmapSource image = (BitmapSource)mainImage.Source;

            int[,] arrR = new int[image.PixelWidth, image.PixelHeight];
            int[,] arrG = new int[image.PixelWidth, image.PixelHeight];
            int[,] arrB = new int[image.PixelWidth, image.PixelHeight];
            Bitmap image2 = ImageDispatcher.BitmapSourceToBitmap(image);

            for (int i = 0; i < image2.Width; i++)
            {
                for (int j = 0; j < image2.Height; j++)
                {
                    arrR[i, j] = image2.GetPixel(i, j).R;
                    arrG[i, j] = image2.GetPixel(i, j).G;
                    arrB[i, j] = image2.GetPixel(i, j).B;
                }
            }

            for (int i = 1; i < image2.Width - 1; i++)
            {
                for (int j = 1; j < image2.Height - 1; j++)
                {
                    int red = arrR[i, j] + Convert.ToInt32(((ComboBoxItem)brightnessPercentage.SelectedItem).Tag) * 128 / 100;
                    int green = arrG[i, j] + Convert.ToInt32(((ComboBoxItem)brightnessPercentage.SelectedItem).Tag) * 128 / 100;
                    int blue = arrB[i, j] + Convert.ToInt32(((ComboBoxItem)brightnessPercentage.SelectedItem).Tag) * 128 / 100;
                    image2.SetPixel(i, j, System.Drawing.Color.FromArgb(red < 0.0 ? 0 : red > 255.0 ? 255 : red,
                        green < 0.0 ? 0 : green > 255.0 ? 255 : green,
                        blue < 0.0 ? 0 : blue > 255.0 ? 255 : blue));
                }
            }
            mainImage.Source = ImageDispatcher.BitmapToImageSource(image2);
        }

        private void contrastPercentage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int percent = Convert.ToInt32(((ComboBoxItem)contrastPercentage.SelectedItem).Tag);
            BitmapSource image = (BitmapSource)mainImage.Source;

            int[,] arrR = new int[image.PixelWidth, image.PixelHeight];
            int[,] arrG = new int[image.PixelWidth, image.PixelHeight];
            int[,] arrB = new int[image.PixelWidth, image.PixelHeight];
            Bitmap image2 = ImageDispatcher.BitmapSourceToBitmap(image);

            for (int i = 0; i < image2.Width; i++)
            {
                for (int j = 0; j < image2.Height; j++)
                {
                    arrR[i, j] = image2.GetPixel(i, j).R;
                    arrG[i, j] = image2.GetPixel(i, j).G;
                    arrB[i, j] = image2.GetPixel(i, j).B;
                }
            }

            for (int i = 1; i < image2.Width - 1; i++)
            {
                for (int j = 1; j < image2.Height - 1; j++)
                {
                    if (percent < 0)
                    {
                        int red = (arrR[i, j] * (100 + percent) - 128 * percent) / 100;
                        int green = (arrG[i, j] * (100 + percent) - 128 * percent) / 100;
                        int blue = (arrB[i, j] * (100 + percent) - 128 * percent) / 100;
                        image2.SetPixel(i, j, System.Drawing.Color.FromArgb(red < 0.0 ? 0 : red > 255.0 ? 255 : red,
                            green < 0.0 ? 0 : green > 255.0 ? 255 : green,
                            blue < 0.0 ? 0 : blue > 255.0 ? 255 : blue));
                    }
                    else
                    {
                        int red = (arrR[i, j] * 100 - 128 * percent) / (100 - percent);
                        int green = (arrG[i, j] * 100 - 128 * percent) / (100 - percent);
                        int blue = (arrB[i, j] * 100 - 128 * percent) / (100 - percent);
                        image2.SetPixel(i, j, System.Drawing.Color.FromArgb(red < 0.0 ? 0 : red > 255.0 ? 255 : red,
                            green < 0.0 ? 0 : green > 255.0 ? 255 : green,
                            blue < 0.0 ? 0 : blue > 255.0 ? 255 : blue));
                    }
                }
            }
            mainImage.Source = ImageDispatcher.BitmapToImageSource(image2);
        }
    }
}
