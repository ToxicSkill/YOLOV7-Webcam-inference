using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.WpfExtensions;
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

        public WriteableBitmap PredictAndDraw(Camera camera, Mat mat)
        {
            using var image = mat.ToBitmap(System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var predictions = _yolov7.Predict(image);
            if (predictions != null)
            {
                foreach (var item in predictions)
                {
                    if (camera.CameraDetectionsQueue.Count() > MaxSizeOfDetectionQueue)
                    {
                        _ = camera.CameraDetectionsQueue.Dequeue();
                    }
                    camera.CameraDetectionsQueue.Enqueue(new Models.CameraDetection(item.Label.Name.ToString(), item.Score.ToString("N2"), Scalar.Black));
                }
                //var lastDetections = camera.CameraDetectionsQueue.TakeLast(5);
                //for (var i = 0; i < lastDetections.Count(); i++)
                //{
                //    switch (i)
                //    {
                //        case 0:
                //            camera.DetectioMinusFour = lastDetections.ElementAt(i);
                //            break;
                //        case 1:
                //            camera.DetectioMinusThree = lastDetections.ElementAt(i);
                //            break;
                //        case 2:
                //            camera.DetectioMinusTwo = lastDetections.ElementAt(i);
                //            break;
                //        case 3:
                //            camera.DetectioMinusOne = lastDetections.ElementAt(i);
                //            break;
                //        case 4:
                //            camera.DetectioZero = lastDetections.ElementAt(i);
                //            break;
                //    }
                //}
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
            }
            return mat.ToWriteableBitmap();
        }
    }
}
