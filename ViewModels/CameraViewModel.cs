using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using YoloV7WebCamInference.Interfaces;
using YoloV7WebCamInference.Models;

namespace YoloV7WebCamInference.ViewModels
{
    [ObservableObject]
    public partial class CameraViewModel
    {
        private const string ModelPath = "Yolo\\yolov7-tiny.onnx";

        private static readonly DispatcherPriority DispatcherPriority = DispatcherPriority.Send;

        private readonly IYoloModelService _yoloModelService;

        private readonly ICameraService _cameraService;

        private CancellationTokenSource _cancellationToken;

        public ObservableCollection<Camera> AvailableCameras { get; set; }

        [ObservableProperty]
        public Camera selectedCamera;

        public CameraViewModel(IYoloModelService yoloModelService, ICameraService cameraService)
        {
            _yoloModelService = yoloModelService;
            _cameraService = cameraService;
            _cancellationToken = new CancellationTokenSource();

            AvailableCameras = new(_cameraService.GetAllCameras());

            var isYoloInitialized = InitializeYolo();
            var isCameraInitialized = InitializeCamera();
            if (!isYoloInitialized && !isCameraInitialized)
            {
                MessageBox.Show("Failed to initialize yolo model and camera");
            }
            else if (!isYoloInitialized)
            {
                MessageBox.Show("Failed to initialize yolo model");
            }
            else if (!isCameraInitialized)
            {
                MessageBox.Show("Failed to initialize camera(s)");
            }


            if (!isYoloInitialized || !isCameraInitialized)
            {
                App.Current.Shutdown();
            }
        }

        private static void OnAsyncFailed(Task task)
        {
            if (task != null)
            {
                Console.Write(task.Exception?.Message);
            }
        }

        partial void OnSelectedCameraChanged(Camera value)
        {
            _cameraService.SetCurrentCamera(SelectedCamera);
            Task.Run(async () =>
            {
                try
                {
                    await PlayCamera();
                }
                catch (Exception)
                {
                    throw;
                }
            });
        }



        private bool InitializeYolo()
        {
            var status = _yoloModelService.LoadYoloModel(ModelPath);

            if (status)
            {
                _yoloModelService.LoadLabels();
            }

            return status;
        }

        private bool InitializeCamera()
        {
            if (_cameraService.IsCameraOpen())
            {
                _cameraService.SetBufferSize(0);
                SelectedCamera = _cameraService.GetCurrentCamera();
                return true;
            }

            return false;
        }

        private async Task PlayCamera()
        {
            var fpsMs = (int)(1000 / 6);// SelectedCamera.Fps);
            long timestamp = 0;
            try
            {
                while (!_cancellationToken.Token.IsCancellationRequested)
                {
                    _cancellationToken.Token.ThrowIfCancellationRequested();
                    timestamp = Stopwatch.GetTimestamp();
                    await Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        SelectedCamera.ImageSource = _yoloModelService.PredictAndDraw(_cameraService.GetFrame());
                        //SelectedCamera.ImageSource = _cameraService.GetFrame().ToWriteableBitmap();
                    }, DispatcherPriority);
                    _cancellationToken.Token.ThrowIfCancellationRequested();
                    var timeMs = Stopwatch.GetElapsedTime(timestamp, Stopwatch.GetTimestamp()).Milliseconds;
                    SelectedCamera.Fps = 1000 / timeMs;
                    await Task.Delay(Math.Clamp(fpsMs - timeMs, 0, fpsMs));
                }
            }
            catch (OperationCanceledException)
            {
                RestartCancelToken();
            }
        }

        private void RestartCancelToken()
        {
            _cancellationToken.Dispose();
            _cancellationToken = new CancellationTokenSource();
        }
    }
}
