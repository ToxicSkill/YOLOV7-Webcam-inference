using CommunityToolkit.Mvvm.ComponentModel;
using OpenCvSharp;
using System.Windows.Media.Imaging;

namespace YoloV7WebCamInference.Models
{
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

        public VideoCapture VideoCapture { get; set; } = videoCapture;
    }
}
