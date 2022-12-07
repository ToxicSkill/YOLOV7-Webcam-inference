using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace YoloV7WebCamInference.Helpers
{
    public static class BitmapHelper
    {
        public static WriteableBitmap Convert(Image img)
        {
            using var memory = new MemoryStream();
            img.Save(memory, ImageFormat.Bmp);
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            memory.Seek(0, SeekOrigin.Begin);
            bitmapImage.StreamSource = memory;
            bitmapImage.EndInit();
            return new(bitmapImage);
        }
    }
}
