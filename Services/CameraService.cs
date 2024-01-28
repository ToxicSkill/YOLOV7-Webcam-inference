using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Windows.Media.Imaging;
using YoloV7WebCamInference.Interfaces;
using YoloV7WebCamInference.Models;

namespace YoloV7WebCamInference.Services
{
    public enum ECameraType
    {
        USB,
        IP
    }

    public class CameraService : ICameraService
    {
        private readonly WriteableBitmap _defaultWriteableBitmap = new Mat(new Size(1, 1), MatType.CV_8UC1).ToWriteableBitmap();

        private readonly List<string> _connectionStrings;

        private int _currentCameraIndex;

        private VideoCapture _videoCapture;

        private List<Camera> _cameras;

        private Mat _image = new();

        public CameraService()
        {
            _connectionStrings = GetConnectionStrings();
            _cameras = [];
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

        public void GrabCameraFrame()
        {
            if (_videoCapture.Grab())
            {
                _image = _videoCapture.RetrieveMat();
            }
        }

        public Mat GetLastCameraFrame()
        {
            return _image;
        }

        public WriteableBitmap GetLastCameraFrameAsWriteableBitmap()
        {
            if (_image != null)
            {
                if (!_image.Empty())
                {
                    return _image.ToWriteableBitmap();
                }
            }
            return _defaultWriteableBitmap;
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

        public void UpdateCameraInfo(Camera camera)
        {
            camera.ImageSourceSize = $"{camera.VideoCapture.FrameWidth}x{camera.VideoCapture.FrameHeight}";
        }

        private void GetAllConnectedCameras()
        {
            _cameras = [];
            var cameraIndex = 0;
            foreach (var cameraName in GetConnectedPnPCameras())
            {
                var videoCapture = new VideoCapture(cameraIndex);
                if (videoCapture.IsOpened())
                {
                    _cameras.Add(new Camera(cameraName, videoCapture));
                    cameraIndex++;
                }
            }
            foreach (var connectionString in GetConnectionStrings())
            {
                var videoCapture = new VideoCapture(connectionString);
                if (videoCapture.IsOpened())
                {
                    _cameras.Add(new Camera(videoCapture.GetBackendName(), videoCapture));
                }
            }
        }

        private static List<string> GetConnectionStrings()
        {
            var splittedConnectionStrings = new List<string>(Properties.Settings.Default.ConnectionStrings.Split("\n", StringSplitOptions.None));
            return splittedConnectionStrings.Where(x => x != string.Empty).ToList();
        }

        private static List<string> GetConnectedPnPCameras()
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
