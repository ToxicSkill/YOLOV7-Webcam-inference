using OpenCvSharp;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media.Imaging;
using YoloV7WebCamInference.Yolo;

namespace YoloV7WebCamInference.Interfaces
{
    public interface IYoloModelService
    {
        bool LoadYoloModel(string path, bool useCUDA = false);

        void LoadLabels(string pathToLabelsFile = "");

        List<YoloPrediction>? Predict(Image image);

        WriteableBitmap Draw(Mat mat, List<YoloPrediction>? predictions);
    }
}
