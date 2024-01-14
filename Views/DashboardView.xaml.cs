using Wpf.Ui.Common.Interfaces;
using YoloV7WebCamInference.ViewModels;

namespace YoloV7WebCamInference.Views
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class DashboardView : INavigableView<DashboardViewModel>
    {
        public DashboardViewModel ViewModel
        {
            get;
        }

        public DashboardView(DashboardViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
            DataContext = ViewModel;
        }
    }
}
