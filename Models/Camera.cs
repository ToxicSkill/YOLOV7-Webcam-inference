using CommunityToolkit.Mvvm.ComponentModel;
using OpenCvSharp;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using YoloV7WebCamInference.Services;

namespace YoloV7WebCamInference.Models
{
    public class CameraDetection(string label, string score)
    {
        public string Label { get; set; } = label;

        public string Score { get; set; } = score;

        public SolidColorBrush Color { get; set; } = StringToColor(label);

        private static SolidColorBrush StringToColor(string input)
        {
            var hash = input.GetHashCode();
            var red = (hash & 0xFF0000) >> 16;
            var green = (hash & 0x00FF00) >> 8;
            var blue = hash & 0x0000FF;

            return new(System.Windows.Media.Color.FromRgb((byte)red, (byte)green, (byte)blue));
        }
    }

    public partial class Camera(string name, VideoCapture videoCapture) : ObservableObject
    {
        [ObservableProperty]
        public ECameraType cameraType;

        [ObservableProperty]
        public string imageSourceSize;

        [ObservableProperty]
        public WriteableBitmap imageSource;

        [ObservableProperty]
        public string name = name;

        [ObservableProperty]
        public double fps = videoCapture.Fps;

        [ObservableProperty]
        public double currentFps;

        [ObservableProperty]
        public CameraDetection detectioZero;

        [ObservableProperty]
        public ObservableQueue<CameraDetection> cameraDetectionsQueue = [];

        public VideoCapture VideoCapture { get; init; } = videoCapture;
    }
}
