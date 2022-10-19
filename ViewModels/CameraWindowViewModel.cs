using OpenCvSharp.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using YoloV7WebCamInference.Interfaces;
using YoloV7WebCamInference.Models;

namespace YoloV7WebCamInference.ViewModels
{
    public partial class CameraWindowViewModel : ViewModelBase
    {
        public string CameraName { get; private set; }

        public string Fps { get; private set; }

        public WriteableBitmap SourceImage { get; private set; }

        private readonly IYoloModelService _yoloModelService;

        private readonly ICameraService _cameraService;

        private readonly string _modelPath;

        public CameraWindowViewModel(IYoloModelService yoloModelService, ICameraService cameraService, string modelPath)
        {
            _yoloModelService = yoloModelService;
            _cameraService = cameraService;
            _modelPath = modelPath;
            InitializeCamera();

            if (InitializeYolo())
            {
                Task.Run(() => PlayCamera());
            }
            else
            {
                CameraName = "Failed to initialize yolo model or camera";
                OnPropertyChanged(nameof(CameraName));
            }
        }

        private bool InitializeYolo()
        {
            var status = _yoloModelService.LoadYoloModel(_modelPath);
            
            if (status)
            {
                _yoloModelService.LoadLabels();
            }

            return status;
        }

        private void InitializeCamera()
        {
            _cameraService.InitializeCamera();
            if (_cameraService.IsCameraOpen())
            {
                _cameraService.SetBufferSize(0);
                CameraName = _cameraService.GetCameraName();
                OnPropertyChanged(nameof(CameraName));
            }
        }

        private void SetFps()
        {
            Fps = _cameraService.GetFps();
            OnPropertyChanged(nameof(Fps));
        }

        private async Task PlayCamera()
        {
            SetFps();
            while (!_cameraService.IsCaptureDisposed())
            {
                var frame = _cameraService.GetFrame();
                if (frame.Empty())
                {
                    break;
                }

                await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() =>
                {
                    Thread.CurrentThread.Priority = ThreadPriority.Highest;
                    SourceImage = _yoloModelService.PredictAndDraw(frame.ToBitmap());
                    OnPropertyChanged(nameof(SourceImage));
                }));
            }
        }
    }
}
