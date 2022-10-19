using System.Drawing;
using System.Windows.Media.Imaging;

namespace YoloV7WebCamInference.Interfaces
{
    public interface IYoloModelService
    {
        bool LoadYoloModel(string path, bool useCUDA = false);

        void LoadLabels(string pathToLabelsFile = "");

        WriteableBitmap PredictAndDraw(Image image);
    }
}
