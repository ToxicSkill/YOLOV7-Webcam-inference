using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Wpf.Ui.Common.Interfaces;
using YoloV7WebCamInference.Interfaces;

namespace YoloV7WebCamInference.ViewModels
{
    public partial class SettingsViewModel : ObservableObject, INavigationAware
    {
        private readonly IYoloModelService _yoloModelService;

        [ObservableProperty]
        private string connectionStrings;

        [ObservableProperty]
        public string senderString;

        [ObservableProperty]
        public string recipientString;

        [ObservableProperty]
        public string senderPasswordString;

        [ObservableProperty]
        public int senderSmtpPort;

        [ObservableProperty]
        public string smptServerString;

        [ObservableProperty]
        public ObservableCollection<string> aiModelLabelsItemsSource;

        [ObservableProperty]
        public ObservableCollection<string> selectedAiModelLabels;

        [ObservableProperty]
        public string selectedAiModelLabel;

        public SettingsViewModel(IYoloModelService yoloModelService)
        {
            _yoloModelService = yoloModelService;

            SenderString = Properties.Settings.Default.Sender;
            RecipientString = Properties.Settings.Default.Recipient;
            SenderPasswordString = Properties.Settings.Default.SenderPassword;
            SenderSmtpPort = Properties.Settings.Default.SenderSmtpPort;
            SmptServerString = Properties.Settings.Default.SmptServer;
            ConnectionStrings = Properties.Settings.Default.ConnectionStrings;

            selectedAiModelLabels = new();
            //AiModelLabelsItemsSource = new(_yoloModelService.GetLabels().Select(x => x.Name).ToList());
            //SelectedAiModelLabel = AiModelLabelsItemsSource.First();
        }

        public void OnNavigatedFrom()
        {
            Properties.Settings.Default.Save();
            //if (SelectedAiModelLabels.Any())
            //{
            //    _yoloModelService.SetSelectedLabel(SelectedAiModelLabels.ToList());
            //}
        }

        public void OnNavigatedTo()
        {
        }

        [RelayCommand]
        private void OkConnectionStrings()
        {
            Properties.Settings.Default.ConnectionStrings = ConnectionStrings;
        }

        [RelayCommand]
        private void OkSenderString()
        {
            Properties.Settings.Default.Sender = SenderString;
        }

        [RelayCommand]
        private void OkRecipientString()
        {
            Properties.Settings.Default.Recipient = RecipientString;
        }

        [RelayCommand]
        private void OkSenderPasswordString()
        {
            Properties.Settings.Default.SenderPassword = SenderPasswordString;
        }

        [RelayCommand]
        private void OkSenderSmtpPort()
        {
            Properties.Settings.Default.SenderSmtpPort = SenderSmtpPort;
        }

        [RelayCommand]
        private void OkSmptServerString()
        {
            Properties.Settings.Default.SmptServer = SmptServerString;
        }

        [RelayCommand]
        private void AddLabel()
        {
            SelectedAiModelLabels.Add(SelectedAiModelLabel);
        }

        [RelayCommand]
        private void RemoveLabel()
        {
            SelectedAiModelLabels.Remove(SelectedAiModelLabel);
        }
    }
}
