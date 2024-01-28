using Wpf.Ui.Common.Interfaces;
using YoloV7WebCamInference.ViewModels;

namespace YoloV7WebCamInference.Views
{
    public partial class CameraView : INavigableView<CameraViewModel>
    {

        public CameraViewModel ViewModel
        {
            get;
        }

        public CameraView(CameraViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
            DataContext = ViewModel;
        }

        private void UiPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.OnLoaded();
        }
    }
}
