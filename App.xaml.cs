using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using YoloV7WebCamInference.Interfaces;
using YoloV7WebCamInference.Services;
using YoloV7WebCamInference.ViewModels;

namespace YoloV7WebCamInference
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        private const string ModelPath = "E:\\Code\\C#\\YOLOV7-Webcam-inference\\yolov7-tiny.onnx";

        public App()
        {
            IServiceCollection services = new ServiceCollection();

            _ = services
                .AddSingleton(CreateYoloModelService)
                .AddSingleton(CreateCameraService)
                .AddSingleton(CreateCameraViewModel)
                .AddSingleton(CreateMainWindowViewModel)
                .AddSingleton(s => new MainWindow()
                {
                    DataContext = s.GetRequiredService<MainWindowViewModel>()
                });

            _serviceProvider = services.BuildServiceProvider();
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            MainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            MainWindow.Show();

            base.OnStartup(e);
        }

        public static CameraWindowViewModel CreateCameraViewModel(IServiceProvider provider)
        {
            var yoloModelService = provider.GetRequiredService<IYoloModelService>();
            var cameraService = provider.GetRequiredService<ICameraService>();
            return new CameraWindowViewModel(yoloModelService, cameraService, ModelPath);
        }

        public static IYoloModelService CreateYoloModelService(IServiceProvider provider)
        {
            return new YoloModelService();
        }
        public static ICameraService CreateCameraService(IServiceProvider provider)
        {
            return new CameraService();
        }

        private MainWindowViewModel CreateMainWindowViewModel(IServiceProvider provider)
        {
            var cameraViewModel = provider.GetRequiredService<CameraWindowViewModel>();
            return new MainWindowViewModel(cameraViewModel);
        }
    }
}
