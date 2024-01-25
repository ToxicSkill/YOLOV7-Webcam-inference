using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Reflection;
using System.Windows;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;
using YoloV7WebCamInference.Interfaces;
using YoloV7WebCamInference.Services;
using YoloV7WebCamInference.ViewModels;
using YoloV7WebCamInference.Views;

namespace YoloV7WebCamInference
{
    public partial class App : Application
    {
        private static readonly IHost _host = Host
        .CreateDefaultBuilder()
        .ConfigureAppConfiguration(c => { c.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)); })
        .ConfigureServices((context, services) =>
        {
            services.AddHostedService<ApplicationHostService>();
            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<ISnackbarService, SnackbarService>();
            services.AddSingleton<IYoloModelService, YoloModelService>();
            services.AddSingleton<ICameraService, CameraService>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<SettingsView>();
            services.AddSingleton<CameraViewModel>();
            services.AddSingleton<CameraView>();
            services.AddSingleton<DashboardViewModel>();
            services.AddSingleton<DashboardView>();
            services.AddScoped<MainWindowViewModel>();
            services.AddScoped<INavigationWindow, MainWindow>();
        }).Build();

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();
        }
    }
}
