using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.Collections.Generic;
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

        public List<YoloPrediction>? Predict(Image image)
        {
            return _yolov7.Predict(image);
        }

        public WriteableBitmap Draw(Mat mat, List<YoloPrediction>? predictions)
        {
            if (predictions == null)
            {
                return mat.ToWriteableBitmap();
            }
            foreach (var prediction in predictions)
            {
                var color = new Scalar(
                    prediction.Label.Color.R,
                    prediction.Label.Color.G,
                    prediction.Label.Color.B);
                var rect = new Rect(
                    (int)prediction.Rectangle.X,
                    (int)prediction.Rectangle.Y,
                    (int)prediction.Rectangle.Width,
                    (int)prediction.Rectangle.Height);
                Cv2.Rectangle(mat, rect, color);
                Cv2.PutText(mat, prediction.Label.Name, new OpenCvSharp.Point(
                    prediction.Rectangle.X - 7,
                    prediction.Rectangle.Y - 23), HersheyFonts.HersheyPlain, 1, color, 2);
            }
            return mat.ToWriteableBitmap();
        }
    }
}
