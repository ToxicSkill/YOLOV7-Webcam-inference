using CommunityToolkit.Mvvm.ComponentModel;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Wpf.Ui.Mvvm.Contracts;
using YoloV7WebCamInference.Interfaces;
using Camera = YoloV7WebCamInference.Models.Camera;

namespace YoloV7WebCamInference.ViewModels
{
    [ObservableObject]
    public partial class CameraViewModel
    {
        private const string ModelPath = "Yolo\\yolov7-tiny.onnx";

        private const int MaxFpsQueueCount = 10;

        private const int MilisecondsInSecond = 1000;

        private const int DefaultFps = 5;

        private static readonly DispatcherPriority DispatcherPriority = DispatcherPriority.Render;

        private readonly ISnackbarService _snackbarService;

        private readonly IYoloModelService _yoloModelService;

        private readonly ICameraService _cameraService;

        private CancellationTokenSource _cancellationToken;

        private readonly Queue<int> _fpsQueue;

        public ObservableCollection<Camera> AvailableCameras { get; set; }

        [ObservableProperty]
        public Camera selectedCamera;

        [ObservableProperty]
        private bool runCamera;

        [ObservableProperty]
        private bool runDetection;

        public CameraViewModel(IYoloModelService yoloModelService, ICameraService cameraService, ISnackbarService snackbarService)
        {
            _snackbarService = snackbarService;
            _yoloModelService = yoloModelService;
            _cameraService = cameraService;
            _cancellationToken = new CancellationTokenSource();
            _fpsQueue = new Queue<int>();

            AvailableCameras = new(_cameraService.GetAllCameras());

            var isYoloInitialized = InitializeYolo();
            var isCameraInitialized = InitializeCamera();
            if (!isYoloInitialized || !isCameraInitialized)
            {
                App.Current.Shutdown();
            }
        }

        partial void OnRunCameraChanged(bool value)
        {
            if (value & CheckRunCameraConditions())
            {
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
            else
            {
                RunDetection = false;
                _cancellationToken.Cancel();
            }
        }

        private bool CheckRunCameraConditions()
        {
            if (SelectedCamera != null)
            {
                return true;
            }
            else
            {
                _snackbarService.Show("Error", "No selected camera", Wpf.Ui.Common.SymbolRegular.ErrorCircle20, Wpf.Ui.Common.ControlAppearance.Danger);
                runCamera = false;
                return false;
            }
        }

        partial void OnSelectedCameraChanged(Camera value)
        {
            _cameraService.SetCurrentCamera(SelectedCamera);
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
            var fpsMs = MilisecondsInSecond / DefaultFps;
            var firstImage = true;
            try
            {
                while (true)
                {
                    _cancellationToken.Token.ThrowIfCancellationRequested();
                    var timestamp = Stopwatch.GetTimestamp();
                    await Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        if (RunDetection && RunCamera)
                        {
                            SelectedCamera.ImageSource = _yoloModelService.PredictAndDraw(SelectedCamera, _cameraService.GetFrame());
                        }
                        else if (RunCamera)
                        {
                            SelectedCamera.ImageSource = _cameraService.GetFrame().ToWriteableBitmap();
                        }
                    }, DispatcherPriority);
                    if (firstImage)
                    {
                        if (SelectedCamera.ImageSource != null)
                        {
                            firstImage = false;
                            await Application.Current.Dispatcher.BeginInvoke(() =>
                            {
                                SelectedCamera.ImageSourceSize = $"{SelectedCamera.ImageSource.Width} x {SelectedCamera.ImageSource.Height}";
                            }, DispatcherPriority);
                        }
                    }
                    var loopTimeMiliseconds = Stopwatch.GetElapsedTime(timestamp, Stopwatch.GetTimestamp()).Milliseconds;
                    if (_fpsQueue.Count > MaxFpsQueueCount / 2)
                    {
                        fpsMs = MilisecondsInSecond / (int)Math.Clamp(MilisecondsInSecond / (_fpsQueue.Average() + 1), 1, SelectedCamera.Fps);
                    }
                    var delayTime = Math.Clamp(fpsMs - loopTimeMiliseconds, 0, fpsMs);
                    SelectedCamera.CurrentFps = Math.Clamp(MilisecondsInSecond / loopTimeMiliseconds, 0, SelectedCamera.Fps);
                    AddLoopTimeToQueue(loopTimeMiliseconds);
                    await Task.Delay(delayTime);
                }
            }
            catch (OperationCanceledException)
            {
                RestartCancelToken();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void AddLoopTimeToQueue(int loopTimeMiliseconds)
        {
            if (_fpsQueue.Count > MaxFpsQueueCount)
            {
                _ = _fpsQueue.Dequeue();
            }
            _fpsQueue.Enqueue(loopTimeMiliseconds);
        }

        private void RestartCancelToken()
        {
            _cancellationToken = new CancellationTokenSource();
        }
    }
}
