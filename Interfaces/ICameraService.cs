using OpenCvSharp;
using System.Collections.Generic;
using YoloV7WebCamInference.Models;

namespace YoloV7WebCamInference.Interfaces
{
    public interface ICameraService
    {
        string GetCurrentCameraName();

        List<Camera> GetAllCameras();

        Camera GetCurrentCamera();

        void SetCurrentCamera(Camera? camera);

        Mat GetFrame();

        void SetFps(int fps);

        void SetBufferSize(int size);

        bool IsCameraOpen();

        bool IsCaptureDisposed();
    }
}
