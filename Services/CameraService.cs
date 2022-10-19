using OpenCvSharp;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using YoloV7WebCamInference.Interfaces;

namespace YoloV7WebCamInference.Services
{
    public class CameraService : ICameraService
    {
        private VideoCapture _videoCapture;

        private readonly Mat _image = new ();

        private bool _status = false;

        public CameraService()
        {
            _videoCapture = new VideoCapture();
        }

        public bool InitializeCamera()
        {
            _videoCapture = new VideoCapture(0);
            _status = _videoCapture != null;
            return _status;
        }

        public string GetCameraName()
        {
            return _status ? GetConnectedCamera() : string.Empty;
        }

        public string GetFps()
        {
            return _videoCapture.Fps.ToString();
        }

        public Mat GetFrame()
        {
            if (_status)
            {
                _videoCapture.Read(_image);
            }
            return _image;
        }

        public void SetFps(int fps)
        {
            if (fps > 0)
            {
                _videoCapture.Set(VideoCaptureProperties.Fps, fps);
            }
        }
        public void SetBufferSize(int size)
        {
            if (size > 0)
            {
                _videoCapture.Set(VideoCaptureProperties.BufferSize, size);
            }
        }

        public bool IsCameraOpen()
        {
            return _videoCapture.IsOpened();
        }

        public bool IsCaptureDisposed()
        {
            return _videoCapture.IsDisposed;
        }

        private static string GetConnectedCamera()
        {
            var cameraNames = new List<string>();
            using (ManagementObjectSearcher searcher = new ("SELECT * FROM Win32_PnPEntity WHERE (PNPClass = 'Image' OR PNPClass = 'Camera')"))
            {
                foreach (var device in searcher.Get())
                {
                    cameraNames.Add(device["Caption"].ToString());
                }
            }

            return cameraNames.First();
        }
    }
}
