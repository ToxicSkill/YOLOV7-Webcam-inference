using OpenCvSharp.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using YoloV7WebCamInference.Interfaces;
using YoloV7WebCamInference.Models;

namespace YoloV7WebCamInference.ViewModels
{
    public partial class CameraWindowViewModel : ViewModelBase
    {
        private CancellationToken _cancellationToken;

        private Camera _selectedCamera;

        public ObservableCollection<Camera> AvailableCameras { get; set; }

        public Camera SelectedCamera
        {
            get => _selectedCamera;
            set
            {
                if (_selectedCamera != value && value != null)
                {
                    _selectedCamera = value;
                    OnPropertyChanged(nameof(SelectedCamera));
                    HandleCameraChange().ContinueWith(OnAsyncFailed, TaskContinuationOptions.OnlyOnFaulted);
                }
            }
        }

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
            AvailableCameras = new(_cameraService.GetAllCameras());

            if (!InitializeYolo() || !InitializeCamera())
            {
                CameraName = "Failed to initialize yolo model or camera";
                App.Current.Shutdown();
            }

            OnPropertyChanged(nameof(AvailableCameras));
        }

        private static void OnAsyncFailed(Task task)
        {
            if (task != null)
            {
                var ex = task.Exception;
                Console.Write(ex?.Message);
            }
        }

        private async Task HandleCameraChange()
        {
            _cancellationToken.ThrowIfCancellationRequested();
            _cameraService.SetCurrentCamera(_selectedCamera);
            CameraName = _selectedCamera.Name;
            OnPropertyChanged(nameof(CameraName));
            OnPropertyChanged(nameof(SelectedCamera));
            await PlayCamera(_cancellationToken);
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

        private bool InitializeCamera()
        {
            if (_cameraService.IsCameraOpen())
            {
                _cameraService.SetBufferSize(0);
                SelectedCamera = _cameraService.GetCurrentCamera();
                OnPropertyChanged(nameof(CameraName));
                return true;
            }

            return false;
        }

        private void SetFps()
        {
            Fps = SelectedCamera.Fps.ToString();
            OnPropertyChanged(nameof(Fps));
        }

        private async Task PlayCamera(CancellationToken cancelToken)
        {
            SetFps();
            while (!_cameraService.IsCaptureDisposed() && !cancelToken.IsCancellationRequested)
            {
                var frame = _cameraService.GetFrame();
                if (frame.Empty())
                {
                    break;
                }

                await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
                {
                    Thread.CurrentThread.Priority = ThreadPriority.Highest;
                    SourceImage = _yoloModelService.PredictAndDraw(frame.ToBitmap());
                    OnPropertyChanged(nameof(SourceImage));
                }));
            }
            await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() =>
            {
                _cameraService.GetCurrentCamera().VideoCapture.Release();
            }));
        }
    }
}
