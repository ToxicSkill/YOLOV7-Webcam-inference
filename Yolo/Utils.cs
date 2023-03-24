﻿using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace YoloV7WebCamInference.Yolo
{
    public static class Utils
    {
        /// <summary>
        /// xywh to xyxy
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static float[] Xywh2xyxy(float[] source)
        {
            var result = new float[4];

            result[0] = source[0] - source[2] / 2f;
            result[1] = source[1] - source[3] / 2f;
            result[2] = source[0] + source[2] / 2f;
            result[3] = source[1] + source[3] / 2f;

            return result;
        }

        public static Bitmap ResizeImage(Image image, int target_width, int target_height)
        {
            PixelFormat format = image.PixelFormat;

            var output = new Bitmap(target_width, target_height, format);

            var (w, h) = (image.Width, image.Height); // image width and height
            var (xRatio, yRatio) = (target_width / (float)w, target_height / (float)h); // x, y ratios
            var ratio = Math.Min(xRatio, yRatio); // ratio = resized / original
            var (width, height) = ((int)(w * ratio), (int)(h * ratio)); // roi width and height
            var (x, y) = ((target_width / 2) - (width / 2), (target_height / 2) - (height / 2)); // roi x and y coordinates
            var roi = new Rectangle(x, y, width, height); // region of interest

            using (var graphics = Graphics.FromImage(output))
            {
                graphics.Clear(Color.FromArgb(0, 0, 0, 0)); // clear canvas

                graphics.SmoothingMode = SmoothingMode.None; // no smoothing
                graphics.InterpolationMode = InterpolationMode.Bilinear; // bilinear interpolation
                graphics.PixelOffsetMode = PixelOffsetMode.Half; // half pixel offset

                graphics.DrawImage(image, roi); // draw scaled
            }

            return output;
        }

        public static Tensor<float> ExtractPixels(Bitmap image)
        {
            var bitmap = (Bitmap)image;

            var rectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bitmapData = bitmap.LockBits(rectangle, ImageLockMode.ReadOnly, bitmap.PixelFormat);
            int bytesPerPixel = Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;

            var tensor = new DenseTensor<float>(new[] { 1, 3, bitmap.Height, bitmap.Width });

            unsafe // speed up conversion by direct work with memory
            {
                Parallel.For(0, bitmapData.Height, (y) =>
                {
                    byte* row = (byte*)bitmapData.Scan0 + (y * bitmapData.Stride);

                    Parallel.For(0, bitmapData.Width, (x) =>
                    {
                        tensor[0, 0, y, x] = row[x * bytesPerPixel + 2] / 255.0F; // r
                        tensor[0, 1, y, x] = row[x * bytesPerPixel + 1] / 255.0F; // g
                        tensor[0, 2, y, x] = row[x * bytesPerPixel + 0] / 255.0F; // b
                    });
                });

                bitmap.UnlockBits(bitmapData);
            }

            return tensor;
        }

        //https://github.com/ivilson/Yolov7net/issues/17
        public static Tensor<float> ExtractPixels2(Bitmap bitmap)
        {
            int pixelCount = bitmap.Width * bitmap.Height;
            var rectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var tensor = new DenseTensor<float>(new[] { 1, 3, bitmap.Height, bitmap.Width });
            Span<byte> data;

            BitmapData bitmapData;
            if (bitmap.PixelFormat == PixelFormat.Format24bppRgb && bitmap.Width % 4 == 0)
            {
                bitmapData = bitmap.LockBits(rectangle, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                unsafe
                {
                    data = new Span<byte>((void*)bitmapData.Scan0, bitmapData.Height * bitmapData.Stride);
                }

                ExtractPixelsRgb(tensor, data, pixelCount);
            }
            else
            {
                // force convert to 32 bit PArgb
                bitmapData = bitmap.LockBits(rectangle, ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);

                unsafe
                {
                    data = new Span<byte>((void*)bitmapData.Scan0, bitmapData.Height * bitmapData.Stride);
                }

                ExtractPixelsArgb(tensor, data, pixelCount);
            }

            bitmap.UnlockBits(bitmapData);

            return tensor;
        }

        public static void ExtractPixelsArgb(DenseTensor<float> tensor, Span<byte> data, int pixelCount)
        {
            var spanR = tensor.Buffer.Span;
            var spanG = spanR.Slice(pixelCount);
            var spanB = spanG.Slice(pixelCount);

            int sidx = 0;
            int didx = 0;
            for (int i = 0; i < pixelCount; i++)
            {
                spanR[didx] = data[sidx + 2] / 255.0F;
                spanG[didx] = data[sidx + 1] / 255.0F;
                spanB[didx] = data[sidx] / 255.0F;
                didx++;
                sidx += 4;
            }
        }

        public static void ExtractPixelsRgb(DenseTensor<float> tensor, Span<byte> data, int pixelCount)
        {
            var spanR = tensor.Buffer.Span;
            var spanG = spanR.Slice(pixelCount);
            var spanB = spanG.Slice(pixelCount);

            int sidx = 0;
            int didx = 0;
            for (int i = 0; i < pixelCount; i++)
            {
                spanR[didx] = data[sidx + 2] / 255.0F;
                spanG[didx] = data[sidx + 1] / 255.0F;
                spanB[didx] = data[sidx] / 255.0F;
                didx++;
                sidx += 3;
            }
        }

        public static float Clamp(float value, float min, float max)
        {
            return value < min ? min : value > max ? max : value;
        }
    }
}