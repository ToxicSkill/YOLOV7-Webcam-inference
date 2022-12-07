using OpenCvSharp;

namespace YoloV7WebCamInference.Models
{
    public class Camera
    {
        public string Name { get; set; }

        public VideoCapture VideoCapture { get; set; }

        public double Fps { get; set; }

        public Camera(string name, VideoCapture videoCapture)
        {
            Name = name;
            VideoCapture = videoCapture;
            Fps = videoCapture.Fps;
        }
    }
}
