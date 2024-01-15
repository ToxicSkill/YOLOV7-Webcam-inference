using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace YoloV7WebCamInference.Yolo
{
    public class YoloV7 : IDisposable
    {
        private readonly InferenceSession _inferenceSession;
        private readonly YoloModel _model = new YoloModel();
        private readonly MD5 _md5;

        public YoloV7(string modelPath, bool useCuda = false)
        {
            _md5 = MD5.Create();
            if (useCuda)
            {
                SessionOptions opts = SessionOptions.MakeSessionOptionWithCudaProvider();
                _inferenceSession = new InferenceSession(modelPath, opts);
            }
            else
            {
                SessionOptions opts = new();
                _inferenceSession = new InferenceSession(modelPath, opts);
            }

            // Get model info
            get_input_details();
            get_output_details();
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
            var s = new string[] { "person", "bicycle", "car", "motorcycle", "airplane", "bus", "train", "truck", "boat", "traffic light", "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep", "cow", "elephant", "bear", "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie", "suitcase", "frisbee", "skis", "snowboard", "sports ball", "kite", "baseball bat", "baseball glove", "skateboard", "surfboard", "tennis racket", "bottle", "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple", "sandwich", "orange", "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair", "couch", "potted plant", "bed", "dining table", "toilet", "tv", "laptop", "mouse", "remote", "keyboard", "cell phone", "microwave", "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase", "scissors", "teddy bear", "hair drier", "toothbrush" };
            SetupLabels(s);
        }

        public List<YoloPrediction> Predict(Image image)
        {
            return ParseDetect(Inference(image)[0], image);
        }

        private List<YoloPrediction> ParseDetect(DenseTensor<float> output, Image image)
        {
            var result = new ConcurrentBag<YoloPrediction>();

            var (w, h) = (image.Width, image.Height); // image w and h
            var (xGain, yGain) = (_model.Width / (float)w, _model.Height / (float)h); // x, y gains
            var gain = Math.Min(xGain, yGain); // gain = resized / original

            var (xPad, yPad) = ((_model.Width - w * gain) / 2, (_model.Height - h * gain) / 2); // left, right pads

            Parallel.For(0, output.Dimensions[0], i =>
            {
                var span = output.Buffer.Span.Slice(i * output.Strides[0]);
                var label = _model.Labels[(int)span[5]];
                var prediction = new YoloPrediction(label, span[6]);

                var xMin = (span[1] - xPad) / gain;
                var yMin = (span[2] - yPad) / gain;
                var xMax = (span[3] - xPad) / gain;
                var yMax = (span[4] - yPad) / gain;

                //install package TensorFlow.Net,SciSharp.TensorFlow.Redist 安装这两个包可以用numpy 进行计算
                //var box = np.array(item.GetValue(1), item.GetValue(2), item.GetValue(3), item.GetValue(4));
                //var tmp =  np.array(xPad, yPad,xPad, yPad) ;
                //box -= tmp;
                //box /= gain;

                prediction.Rectangle = new RectangleF(xMin, yMin, xMax - xMin, yMax - yMin);
                result.Add(prediction);
            });

            return result.ToList();
        }

        private DenseTensor<float>[] Inference(Image img)
        {
            Bitmap resized;

            if (img.Width != _model.Width || img.Height != _model.Height)
            {
                resized = Utils.ResizeImage(img, _model.Width, _model.Height); // fit image size to specified input size
            }
            else
            {
                resized = new Bitmap(img);
            }

            var inputs = new List<NamedOnnxValue> // add image as onnx input
            {
                NamedOnnxValue.CreateFromTensor("images", Utils.ExtractPixels2(resized))
            };

            var result = _inferenceSession.Run(inputs); // run inference

            var output = new List<DenseTensor<float>>();

            foreach (var item in _model.Outputs) // add outputs for processing
            {
                output.Add(result.First(x =>
                {
                    return x.Name == item;
                }).Value as DenseTensor<float>);
            };

            return [.. output];
        }

        private void get_input_details()
        {
            _model.Height = _inferenceSession.InputMetadata["images"].Dimensions[2];
            _model.Width = _inferenceSession.InputMetadata["images"].Dimensions[3];
        }

        private void get_output_details()
        {
            _model.Outputs = _inferenceSession.OutputMetadata.Keys.ToArray();
            _model.Dimensions = _inferenceSession.OutputMetadata[_model.Outputs[0]].Dimensions[1];
            _model.UseDetect = !(_model.Outputs.Any(x => x == "score"));
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