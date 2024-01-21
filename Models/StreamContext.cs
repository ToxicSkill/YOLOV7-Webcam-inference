using CommunityToolkit.Mvvm.ComponentModel;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using YoloV7WebCamInference.Enums;
using YoloV7WebCamInference.Yolo;

namespace Horus.Models
{
    public partial class StreamContext : ObservableObject
    {
        [ObservableProperty]
        private WriteableBitmap displayFrame;

        public static int MaxDetectedObjects => 3;

        public static int MaxMoveRectanglesHistory => 200;

        public Mat CurrentFrame { get; set; }

        public ConcurrentQueue<Mat> FramesQue { get; set; }

        public Mat Heatmap { get; set; }

        public List<Rect> MoveRectangles { get; set; }

        public Queue<List<Rect>> MoveRectanglesHistory { get; set; }

        public Queue<List<Point>> MovePointsHistory { get; set; }

        public List<Point> HeatmapPoints { get; set; }

        public List<Point> ClusteringCentroidsPoints { get; set; }

        public Queue<List<Point>> ClusteringCentroidsPointsHistory { get; set; }

        public Queue<List<YoloPrediction>> AIDetectionsHistory { get; set; }

        public Mat DifferenceFrame { get; set; }

        public System.Drawing.SizeF StreamSize { get; set; }

        public int MeanOfDetectedObjects { get; set; }

        [ObservableProperty]
        public EPreviewMode previewMode;

        public StreamContext()
        {
            DisplayFrame = new Mat().ToWriteableBitmap();
            CurrentFrame = new();
            FramesQue = new();
            Heatmap = new();
            MoveRectangles = [];
            MoveRectanglesHistory = new();
            DifferenceFrame = new();
            MovePointsHistory = new();
            HeatmapPoints = [];
            ClusteringCentroidsPoints = [];
            ClusteringCentroidsPointsHistory = new();
            AIDetectionsHistory = new();
        }
    }
}
