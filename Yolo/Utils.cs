using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace YoloV7WebCamInference.Yolo
{
    public static class Utils
    {
        public static float[] Xywh2xyxy(float[] source)
        {
            return new float[4]
            {
                source[0] - source[2] / 2f,
                source[1] - source[3] / 2f,
                source[0] + source[2] / 2f,
                source[1] + source[3] / 2f
            };
        }

        public static Bitmap ResizeImage(Image image, int target_width, int target_height)
        {
            PixelFormat pixelFormat = image.PixelFormat;
            Bitmap bitmap = new Bitmap(target_width, target_height, pixelFormat);
            int width = image.Width;
            int height = image.Height;
            int num = width;
            int num2 = height;
            float num3 = (float)target_width / (float)num;
            float num4 = (float)target_height / (float)num2;
            float val = num3;
            float val2 = num4;
            float num5 = Math.Min(val, val2);
            width = (int)((float)num * num5);
            int num6 = (int)((float)num2 * num5);
            int num7 = width;
            int num8 = num6;
            width = target_width / 2 - num7 / 2;
            int num9 = target_height / 2 - num8 / 2;
            int x = width;
            int y = num9;
            Rectangle rect = new Rectangle(x, y, num7, num8);
            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.FromArgb(0, 0, 0, 0));
            graphics.SmoothingMode = SmoothingMode.None;
            graphics.InterpolationMode = InterpolationMode.Bilinear;
            graphics.PixelOffsetMode = PixelOffsetMode.Half;
            graphics.DrawImage(image, rect);
            return bitmap;
        }

        public unsafe static Tensor<float> ExtractPixels(Bitmap image)
        {
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
            BitmapData bitmapData = image.LockBits(rect, ImageLockMode.ReadOnly, image.PixelFormat);
            int bytesPerPixel = Image.GetPixelFormatSize(image.PixelFormat) / 8;
            DenseTensor<float> tensor = new DenseTensor<float>(new int[4] { 1, 3, image.Height, image.Width });
            Parallel.For(0, bitmapData.Height, delegate (int y)
            {
                byte* row = (byte*)(void*)bitmapData.Scan0 + y * bitmapData.Stride;
                Parallel.For(0, bitmapData.Width, delegate (int x)
                {
                    tensor[new int[4] { 0, 0, y, x }] = (float)(int)row[x * bytesPerPixel + 2] / 255f;
                    tensor[new int[4] { 0, 1, y, x }] = (float)(int)row[x * bytesPerPixel + 1] / 255f;
                    tensor[new int[4] { 0, 2, y, x }] = (float)(int)row[x * bytesPerPixel] / 255f;
                });
            });
            image.UnlockBits(bitmapData);
            return tensor;
        }

        public static float Clamp(float value, float min, float max)
        {
            if (!(value < min))
            {
                if (!(value > max))
                {
                    return value;
                }

                return max;
            }

            return min;
        }
    }
}