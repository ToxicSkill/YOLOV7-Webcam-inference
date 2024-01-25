using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Controls.Navigation;
using YoloV7WebCamInference.Views;

namespace YoloV7WebCamInference.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        public ICollection<INavigationControl> menuItems;

        [ObservableProperty]
        public ICollection<INavigationControl> footerItems;

        [ObservableProperty]
        public CameraView cameraView;

        public MainWindowViewModel(CameraView cameraView)
        {
            MenuItems = new ObservableCollection<INavigationControl>();
            FooterItems = new ObservableCollection<INavigationControl>();
            InitializeMenu();
            CameraView = cameraView;
        }

        private void InitializeMenu()
        {
            MenuItems.Add(new NavigationItem()
            {
                Icon = SymbolRegular.Home20,
                PageTag = "home",
                Cache = true,
                Content = "Main",
                PageType = typeof(DashboardView)
            });
            MenuItems.Add(new NavigationSeparator());
            MenuItems.Add(new NavigationItem()
            {
                Icon = SymbolRegular.Camera20,
                PageTag = "camera",
                Cache = true,
                Content = "Camera",
                PageType = typeof(CameraView)
            });
            MenuItems.Add(new NavigationItem()
            {
                Icon = SymbolRegular.Settings20,
                PageTag = "settings",
                Cache = true,
                Content = "Settings",
                PageType = typeof(SettingsView)
            });
        }
    }
}
