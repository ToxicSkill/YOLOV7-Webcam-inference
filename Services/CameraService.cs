using OpenCvSharp;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using YoloV7WebCamInference.Interfaces;
using YoloV7WebCamInference.Models;

namespace YoloV7WebCamInference.Services
{
    public class CameraService : ICameraService
    {

        private VideoCapture _videoCapture;

        private Camera _currentCamera;

        private List<Camera> _cameras;

        private readonly Mat _image = new ();

        public CameraService()
        {
            GetAllConnectedCameras();
            _videoCapture = new VideoCapture();
            SetCurrentCamera(_cameras.Count > 0 ? _cameras.First() : null);
        }

        public string GetCurrentCameraName()
        {
            return _currentCamera.Name;
        }

        public List<Camera> GetAllCameras()
        {
            return _cameras;
        }

        public Camera GetCurrentCamera()
        {
            return _currentCamera;
        }

        public void SetCurrentCamera(Camera? camera)
        {
            if (camera == null)
            {
                return;
            }
            _currentCamera = camera;
            _videoCapture = _currentCamera.VideoCapture;
        }

        public Mat GetFrame()
        {
            _videoCapture.Read(_image);
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

        private void GetAllConnectedCameras()
        {
            _cameras = new ();
            var cameraIndex = 0;
            foreach (var cameraName in GetConnectedCameras())
            {
                _cameras.Add(new Camera(cameraName, new VideoCapture(cameraIndex)));
                cameraIndex++;
            }
        }

        private static List<string> GetConnectedCameras()
        {
            var cameraNames = new List<string>();
            using (ManagementObjectSearcher searcher = new ("SELECT * FROM Win32_PnPEntity WHERE (PNPClass = 'Image' OR PNPClass = 'Camera')"))
            {
                foreach (var device in searcher.Get())
                {
                    cameraNames.Add(device["Caption"].ToString());
                }
            }

            return cameraNames;
        }
    }
}
