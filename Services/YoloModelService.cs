using System;
using System.Drawing;
using System.Windows.Media.Imaging;
using YoloV7WebCamInference.Interfaces;
using YoloV7WebCamInference.Yolo;

namespace YoloV7WebCamInference.Services
{
    public class YoloModelService : IYoloModelService
    {
        private YoloV7 _yolov7;

        public bool LoadYoloModel(string path, bool useCUDA = false)
        {
            _yolov7 = new YoloV7(path, useCUDA);
            return _yolov7 != null;
        }

        public void LoadLabels(string pathToLabelsFile = "")
        {
            if (pathToLabelsFile == "")
            {
                _yolov7.SetupYoloDefaultLabels();
            }
        }

        public WriteableBitmap PredictAndDraw(Image image)
        {
            using var graphics = Graphics.FromImage(image);
            foreach (var prediction in _yolov7.Predict(image))
            {
                double score = Math.Round(prediction.Score, 2);
                graphics.DrawRectangles(new Pen(prediction.Label.Color, 1), new[] { prediction.Rectangle });
                var (x, y) = (prediction.Rectangle.X - 3, prediction.Rectangle.Y - 23);
                graphics.DrawString($"{prediction.Label.Name} ({score})",
                                new Font("Consolas", 16, GraphicsUnit.Pixel), new SolidBrush(prediction.Label.Color),
                                new PointF(x, y));
            }

            return Helpers.BitmapHelper.Convert(image);
        }
    }
}
