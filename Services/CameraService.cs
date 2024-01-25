﻿using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
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
        private int _currentCameraIndex;

        private VideoCapture _videoCapture;

        private List<Camera> _cameras;

        private readonly Mat _image = new();

        private readonly List<string> _connectionStrings;

        public CameraService()
        {
            _connectionStrings = GetConnectionStrings();
            _cameras = [];
            GetAllConnectedCameras();
            _videoCapture = new VideoCapture();
            SetCurrentCamera(_cameras?.Count > 0 ? _cameras.First() : null);
        }

        private List<string> GetConnectionStrings()
        {
            return [.. Properties.Settings.Default.ConnectionStrings.Split("\n", StringSplitOptions.None)];
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
