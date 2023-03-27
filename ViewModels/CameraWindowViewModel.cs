using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using YoloV7WebCamInference.Interfaces;
using YoloV7WebCamInference.Models;

namespace YoloV7WebCamInference.ViewModels
{
    public partial class CameraWindowViewModel : ViewModelBase
    {
        private System.Windows.Media.Imaging.WriteableBitmap _sourceImage;

        private CancellationTokenSource _cancellationToken;

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

        public System.Windows.Media.Imaging.WriteableBitmap SourceImage
        {
            get => _sourceImage;
            private set
            {
                _sourceImage = value;
                OnPropertyChanged(nameof(SourceImage));
            }
        }

        private readonly IYoloModelService _yoloModelService;

        private readonly ICameraService _cameraService;

        private readonly string _modelPath;

        public CameraWindowViewModel(IYoloModelService yoloModelService, ICameraService cameraService, string modelPath)
        {
            _yoloModelService = yoloModelService;
            _cameraService = cameraService;
            _cancellationToken = new CancellationTokenSource();
            _modelPath = modelPath;

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
            _cameraService.SetCurrentCamera(_selectedCamera);
            CameraName = _selectedCamera.Name;
            OnPropertyChanged(nameof(CameraName));
            OnPropertyChanged(nameof(SelectedCamera));
            await PlayCamera();
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

        private async Task PlayCamera()
        {
            try
            {
                while (true)
                {
                    _cancellationToken.Token.ThrowIfCancellationRequested();
                    await Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        SourceImage = _yoloModelService.PredictAndDraw(_cameraService.GetFrame());
                    }, DispatcherPriority.Render);
                    _cancellationToken.Token.ThrowIfCancellationRequested();
                    await Task.Delay(20);
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
