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
        private int _currentCameraIndex;

        private VideoCapture _videoCapture;

        private List<Camera> _cameras;

        private readonly Mat _image = new();

        public CameraService()
        {
            _cameras = new List<Camera>();
            GetAllConnectedCameras();
            _videoCapture = new VideoCapture();
            SetCurrentCamera(_cameras?.Count > 0 ? _cameras.First() : null);
        }

        public string GetCurrentCameraName()
        {
            return _cameras[_currentCameraIndex].Name;
        }

        public List<Camera> GetAllCameras()
        {
            return _cameras;
        }

        public Camera GetCurrentCamera()
        {
            return _cameras[_currentCameraIndex];
        }

        public void SetCurrentCamera(Camera? camera)
        {
            if (camera == null)
            {
                return;
            }

            _currentCameraIndex = _cameras.IndexOf(camera);
            _videoCapture = _cameras[_currentCameraIndex].VideoCapture;
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
            _cameras = new();
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
            using (ManagementObjectSearcher searcher = new("SELECT * FROM Win32_PnPEntity WHERE (PNPClass = 'Image' OR PNPClass = 'Camera')"))
            {
                foreach (var device in searcher.Get())
                {
                    var deviceName = device["Caption"].ToString() ?? string.Empty;
                    cameraNames.Add(deviceName);
                }
            }

            return cameraNames;
        }
    }
}
