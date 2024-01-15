using CommunityToolkit.Mvvm.ComponentModel;
using OpenCvSharp;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace YoloV7WebCamInference.Models
{
    public partial class CameraDetection(string label, string score, Scalar color) : ObservableObject
    {
        [ObservableProperty]
        public string label = label;

        [ObservableProperty]
        public string score = score;

        [ObservableProperty]
        public Scalar color = color;
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
        public Queue<CameraDetection> cameraDetectionsQueue = new();

        public VideoCapture VideoCapture { get; set; } = videoCapture;
    }
}
