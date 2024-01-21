using OpenCvSharp;
using System.Windows.Media.Imaging;
using YoloV7WebCamInference.Models;

namespace YoloV7WebCamInference.Interfaces
{
    public interface IYoloModelService
    {
        bool LoadYoloModel(string path, bool useCUDA = false);

        void LoadLabels(string pathToLabelsFile = "");

        public WriteableBitmap PredictAndDraw(Camera camera, Mat mat, int scoreThreshold);
    }
}
