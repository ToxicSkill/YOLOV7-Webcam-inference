using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.WpfExtensions;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using YoloV7WebCamInference.Interfaces;
using YoloV7WebCamInference.Models;
using YoloV7WebCamInference.Yolo;

namespace YoloV7WebCamInference.Services
{
    public class YoloModelService : IYoloModelService
    {
        private const int MaxSizeOfDetectionQueue = 10;

        private const string ModelPath = "Yolo\\yolov7-tiny.onnx";

        private YoloV7 _yolov7;

        public YoloModelService()
        {
            _yolov7 = new YoloV7(ModelPath);
            LoadLabels();
        }

        public void LoadLabels(string pathToLabelsFile = "")
        {
            if (pathToLabelsFile == "")
            {
                _yolov7.SetupYoloDefaultLabels();
            }
        }

        public WriteableBitmap PredictAndDraw(Camera camera, Mat mat, int scoreThreshold)
        {
            Draw(camera, mat, _yolov7.Predict(mat.ToBitmap(System.Drawing.Imaging.PixelFormat.Format24bppRgb)), scoreThreshold);
            return mat.ToWriteableBitmap();
        }

        private static void Draw(Camera camera, Mat mat, List<YoloPrediction> predictions, int scoreThreshold)
        {
            if (predictions != null)
            {
                foreach (var prediction in predictions)
                {
                    if (prediction.Label == null)
                    {
                        continue;
                    }
                    if (camera.CameraDetectionsQueue.Count() > MaxSizeOfDetectionQueue)
                    {
                        _ = camera.CameraDetectionsQueue.Dequeue();
                    }
                    var score = prediction.Score * 100;
                    if (score >= scoreThreshold)
                    {
                        camera.CameraDetectionsQueue.Enqueue(new CameraDetection(prediction.Label?.Name?.ToString(), score.ToString("N1")));
                    }
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
                    Cv2.PutText(mat, prediction.Label.Name, new Point(
                        prediction.Rectangle.X - 7,
                        prediction.Rectangle.Y - 23), HersheyFonts.HersheyPlain, 1, color, 2);
                }
            }
        }
    }
}
