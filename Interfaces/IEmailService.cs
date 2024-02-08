using YoloV7WebCamInference.Models;

namespace YoloV7WebCamInference.Interfaces
{
    public interface IEmailService
    {
        void SendMail(EMail email);
    }
}