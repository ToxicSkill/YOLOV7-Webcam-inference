using YoloV7WebCamInference.Models;

namespace YoloV7WebCamInference.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ViewModelBase ViewModel { get; set; }

        public MainWindowViewModel(ViewModelBase viewModel)
        {
            ViewModel = viewModel;
            OnPropertyChanged(nameof(ViewModel));
        }
    }
}
