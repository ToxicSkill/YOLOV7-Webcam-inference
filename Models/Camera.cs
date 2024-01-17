using CommunityToolkit.Mvvm.ComponentModel;
using OpenCvSharp;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace YoloV7WebCamInference.Models
{
    public class CameraDetection(string label, string score, SolidColorBrush color)
    {
        public string Label { get; set; } = label;

        public string Score { get; set; } = score;

        public SolidColorBrush Color { get; set; } = color;
    }

    public partial class Camera(string name, VideoCapture videoCapture) : ObservableObject
    {
        [ObservableProperty]
        public WriteableBitmap imageSource;

        [ObservableProperty]
        public string name = name;

        [ObservableProperty]
        public double fps;

        [ObservableProperty]
        public double currentFps = videoCapture.Fps;

        [ObservableProperty]
        public CameraDetection detectioZero;

        [ObservableProperty]
        public ObservableQueue<CameraDetection> cameraDetectionsQueue = [];

        public VideoCapture VideoCapture { get; set; } = videoCapture;
    }
}
