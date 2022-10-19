using OpenCvSharp;

namespace YoloV7WebCamInference.Interfaces
{
    public interface ICameraService
    {
        bool InitializeCamera();

        string GetCameraName();

        Mat GetFrame();

        string GetFps();

        void SetFps(int fps);

        void SetBufferSize(int size);

        bool IsCameraOpen();

        bool IsCaptureDisposed();
    }
}
