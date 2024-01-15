﻿using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui.Mvvm.Contracts;
using YoloV7WebCamInference.Views;

namespace YoloV7WebCamInference.Services;

/// <summary>
/// Managed host of the application.
/// </summary>
public class ApplicationHostService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INavigationService _navigationService;
    private readonly IPageService _pageService;
    private readonly IThemeService _themeService;

    private INavigationWindow _navigationWindow;

    public ApplicationHostService(IServiceProvider serviceProvider, INavigationService navigationService,
        IPageService pageService, IThemeService themeService)
    {
        // If you want, you can do something with these services at the beginning of loading the application.
        _serviceProvider = serviceProvider;
        _navigationService = navigationService;
        _pageService = pageService;
        _themeService = themeService;
    }

    /// <summary>
    /// Triggered when the application host is ready to start the service.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        PrepareNavigation();

        await HandleActivationAsync();
    }

    /// <summary>
    /// Triggered when the application host is performing a graceful shutdown.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }

    /// <summary>
    /// Creates main window during activation.
    /// </summary>
    private async Task HandleActivationAsync()
    {
        await Task.CompletedTask;

        if (!Application.Current.Windows.OfType<MainWindow>().Any())
        {
            _navigationWindow = _serviceProvider.GetService(typeof(INavigationWindow)) as INavigationWindow;
            _navigationWindow!.ShowWindow();

            _navigationWindow.Navigate(typeof(DashboardView));
        }

        await Task.CompletedTask;
    }

    private void PrepareNavigation()
    {
        _navigationService.SetPageService(_pageService);
    }
}