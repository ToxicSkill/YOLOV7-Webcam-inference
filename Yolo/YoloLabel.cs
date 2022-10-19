using System.Drawing;

namespace YoloV7WebCamInference.Yolo
{
    public class YoloLabel
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public Color Color { get; set; }
    }
}