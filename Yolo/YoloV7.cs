using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace YoloV7WebCamInference.Yolo
{
    public class YoloV7 : IDisposable
    {
        private readonly InferenceSession _inferenceSession;

        private readonly YoloModel _model = new ();

        private readonly MD5 _md5;

        public YoloV7(string ModelPath, bool useCuda = false)
        {
            _md5 = MD5.Create();
            if (useCuda)
            {
                SessionOptions options = SessionOptions.MakeSessionOptionWithCudaProvider();
                _inferenceSession = new (ModelPath, options);
            }
            else
            {
                SessionOptions options2 = new ();
                _inferenceSession = new (ModelPath, options2);
            }

            GetInputDetails();
            GetOutputDetails();
        }

        public void SetupLabels(string[] labels)
        {
            labels.Select((string s, int i) => new { i, s }).ToList().ForEach(item =>
            {
                _model.Labels.Add(new YoloLabel
                {
                    Id = item.i,
                    Name = item.s,
                    Color = GetLabelColor(item.s)
                });
            });
        }

        public void SetupYoloDefaultLabels()
        {
            string[] labels = new string[80]
            {
                "person", "bicycle", "car", "motorcycle", "airplane", "bus", "train", "truck", "boat", "traffic light",
                "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep", "cow",
                "elephant", "bear", "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie", "suitcase", "frisbee",
                "skis", "snowboard", "sports ball", "kite", "baseball bat", "baseball glove", "skateboard", "surfboard", "tennis racket", "bottle",
                "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple", "sandwich", "orange",
                "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair", "couch", "potted plant", "bed",
                "dining table", "toilet", "tv", "laptop", "mouse", "remote", "keyboard", "cell phone", "microwave", "oven",
                "toaster", "sink", "refrigerator", "book", "clock", "vase", "scissors", "teddy bear", "hair drier", "toothbrush"
            };
            SetupLabels(labels);
        }

        public List<YoloPrediction> Predict(Image image)
        {
            return ParseDetect(Inference(image)[0], image);
        }

        private List<YoloPrediction> ParseDetect(DenseTensor<float> output, Image image)
        {
            DenseTensor<float> output2 = output;
            ConcurrentBag<YoloPrediction> result = new ConcurrentBag<YoloPrediction>();
            int width = image.Width;
            int height = image.Height;
            int num = width;
            int num2 = height;
            float num3 = (float)_model.Width / (float)num;
            float num4 = (float)_model.Height / (float)num2;
            float val = num3;
            float val2 = num4;
            float gain = Math.Min(val, val2);
            num3 = ((float)_model.Width - (float)num * gain) / 2f;
            float num5 = ((float)_model.Height - (float)num2 * gain) / 2f;
            float xPad = num3;
            float yPad = num5;
            Parallel.For(0, output2.Dimensions[0], delegate (int i)
            {
                YoloPrediction yoloPrediction = new YoloPrediction(_model.Labels[(int)output2[new int[2] { i, 5 }]], output2[new int[2] { 0, 6 }]);
                float num6 = (output2[new int[2] { i, 1 }] - xPad) / gain;
                float num7 = (output2[new int[2] { i, 2 }] - yPad) / gain;
                float num8 = (output2[new int[2] { i, 3 }] - xPad) / gain;
                float num9 = (output2[new int[2] { i, 4 }] - yPad) / gain;
                yoloPrediction.Rectangle = new RectangleF(num6, num7, num8 - num6, num9 - num7);
                result.Add(yoloPrediction);
            });
            return result.ToList();
        }

        private DenseTensor<float>[] Inference(Image img)
        {
            var bitmap = ((img.Width == _model.Width && img.Height == _model.Height) ? new Bitmap(img) : Utils.ResizeImage(img, _model.Width, _model.Height));
            var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("images", Utils.ExtractPixels(bitmap)) };
            var source = _inferenceSession.Run(inputs);
            var list = new List<DenseTensor<float>>();
            string[] outputs = _model.Outputs;
            foreach (string item in outputs)
            {
                list.Add((DenseTensor<float>)source.First((DisposableNamedOnnxValue x) => x.Name == item).Value);
            }

            return list.ToArray();
        }

        private void GetInputDetails()
        {
            _model.Height = _inferenceSession.InputMetadata["images"].Dimensions[2];
            _model.Width = _inferenceSession.InputMetadata["images"].Dimensions[3];
        }

        private void GetOutputDetails()
        {
            _model.Outputs = _inferenceSession.OutputMetadata.Keys.ToArray();
            _model.Dimensions = _inferenceSession.OutputMetadata[_model.Outputs[0]].Dimensions[1];
            _model.UseDetect = !_model.Outputs.Any((string x) => x == "score");
        }

        private Color GetLabelColor(string? name)
        {
            var hash = _md5.ComputeHash(Encoding.UTF8.GetBytes(name));
            return Color.FromArgb(hash[2], hash[0], hash[1]);
        }

        public void Dispose()
        {
            _inferenceSession.Dispose();
        }
    }
}