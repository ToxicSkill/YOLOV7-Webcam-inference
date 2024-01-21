using System;

namespace YoloV7WebCamInference.Enums
{
    [Flags]
    public enum EPreviewMode
    {
        None = 1 << 0,
        MoveDetector = 1 << 1,
        Heatmap = 1 << 2,
        AIDetector = 1 << 3
    }
}
