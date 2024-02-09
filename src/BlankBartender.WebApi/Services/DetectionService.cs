using BlankBartender.WebApi.External.Models;
using BlankBartender.WebApi.External;
using BlankBartender.WebApi.Services.Interfaces;
using System.Runtime.InteropServices;
using OpenCvSharp;
using System.Collections.Generic;

namespace BlankBartender.WebApi.Services
{
    public class DetectionService : IDetectionService
    {
        private string modelPath = "best3.rknn";
        private string labelsPath = "coco_80_labels_list.txt";
        private readonly byte[] modelData;
        private readonly List<string> labels = new List<string>();
        private readonly RknnInputOutputNum io_num = new RknnInputOutputNum();
        private readonly RknnTensorAttr[] inputAttrs;
        private readonly RknnTensorAttr[] outputAttrs;

        private int ret;
        private UIntPtr ctx = default; // readonly ?
        private RknnInput[] inputs = new RknnInput[1];
        private Mat padding = new Mat();
        private Mat rawImage = new Mat();
        private Size size = new OpenCvSharp.Size(640, 480);
        private bool first =true;


        private static string pipeline = "v4l2src device=/dev/video0 ! video/x-raw, format=(string)NV12, width=(int)2592, height=(int)1944 ! videoconvert ! videoscale ! appsink";
        private VideoCapture capture = new VideoCapture(pipeline, VideoCaptureAPIs.GSTREAMER);
        public DetectionService()
        {
            modelData = System.IO.File.ReadAllBytes(modelPath);
            labels = File.ReadAllLines(labelsPath).ToList();

            int ret = RknnApi.rknn_init(ref ctx, modelData, modelData.Length, 0);
            if (ret < 0)
            {
                Console.WriteLine($"rknn_init error ret={ret}");
                return;
            }

            ret = RknnApi.rknn_query(ctx, RknnQueryCmd.InOutNum, ref io_num, Marshal.SizeOf(typeof(RknnInputOutputNum)));
            if (ret < 0)
            {
                Console.WriteLine($"rknn_query error ret={ret} InOutNum");
                return;
            }

            inputAttrs = new RknnTensorAttr[io_num.input];
            for (int i = 0; i < io_num.input; i++)
            {
                inputAttrs[i] = new RknnTensorAttr
                {
                    Index = (uint)i,
                    Dims = new uint[16]
                };
                ret = RknnApi.rknn_query(ctx, RknnQueryCmd.InputAttr, ref inputAttrs[i], Marshal.SizeOf(typeof(RknnTensorAttr)));
                if (ret < 0)
                {
                    Console.WriteLine($"rknn_query error ret={ret} InputAttr");
                }
            }

            outputAttrs = new RknnTensorAttr[io_num.output];
            for (int i = 0; i < io_num.output; i++)
            {
                outputAttrs[i] = new RknnTensorAttr();
                outputAttrs[i].Index = (uint)i;

                ret = RknnApi.rknn_query(ctx, RknnQueryCmd.OutputAttr, ref outputAttrs[i], Marshal.SizeOf(typeof(RknnTensorAttr)));
                if (ret < 0)
                {
                    Console.WriteLine($"rknn_query error ret={ret} OutputAttr");
                    return;
                }
            }

            uint channel = 3;
            uint width = 0;
            uint height = 0;
            if (inputAttrs[0].Fmt == RknnTensorFormat.RKNN_TENSOR_NCHW)
            {
                channel = inputAttrs[0].Dims[1];
                height = inputAttrs[0].Dims[2];
                width = inputAttrs[0].Dims[3];
            }
            else
            {
                height = inputAttrs[0].Dims[1];
                width = inputAttrs[0].Dims[2];
                channel = inputAttrs[0].Dims[3];
            }
            //Console.WriteLine($"{ret}{ctx} ");
            inputs[0] = new RknnInput
            {
                Index = 0,
                Type = RknnTensorType.RKNN_TENSOR_UINT8,
                Size = (uint)(width * height * channel),
                Fmt = RknnTensorFormat.RKNN_TENSOR_NHWC,
                PassThrough = 0
            };

            var isCaptured = capture.Read(rawImage);

                capture.Read(rawImage);
                capture.Read(rawImage);


            string filePath = "capturedImage.jpg";


            if (!capture.IsOpened())
            capture.Release();
            Cv2.Resize(rawImage, padding, size);


            inputs[0].Buf = padding.Data;
            var isSaved = padding.SaveImage(filePath);

            bool success = Recognize(inputs, ctx, ret, io_num, outputAttrs);
        }

        public async Task<bool> DetectGlass()
        {
            //this.capture = new VideoCapture(0, VideoCaptureAPIs.V4L); // 0 indicates the default camera
            

            var isCaptured = capture.Read(rawImage);
            if (first)
            {
                capture.Read(rawImage);
                capture.Read(rawImage);

                first = false;
            }

            string filePath = "capturedImage.jpg";


           // if (!capture.IsOpened())
           //     return false;
           // capture.Release();
            Cv2.Resize(rawImage, padding, size);
          

            inputs[0].Buf = padding.Data;
            var isSaved = padding.SaveImage(filePath);

            bool success = Recognize(inputs, ctx, ret, io_num, outputAttrs);
            return success;
        }
        private bool Recognize(RknnInput[] inputs, UIntPtr ctx, int ret, RknnInputOutputNum io_num, RknnTensorAttr[] outputAttrs)
        {
            // Initialize the outputs array...
            ret = RknnApi.rknn_inputs_set(ctx, io_num.input, inputs);
            if (ret < 0)
            {
                Console.WriteLine($"rknn_inputs_set error ret={ret}");
                return false;
            }
            RknnOutput[] outputs = new RknnOutput[io_num.output];
            GCHandle handle = GCHandle.Alloc(outputs, GCHandleType.Pinned);
            IntPtr pointer = handle.AddrOfPinnedObject();

            ret = RknnApi.rknn_run(ctx, IntPtr.Zero);
            if (ret < 0)
            {
                Console.WriteLine($"rknn_run error ret={ret}");
                return false;
            }

            ret = RknnApi.rknn_outputs_get(ctx, io_num.output, pointer, IntPtr.Zero);
            if (ret < 0)
            {
                Console.WriteLine($"rknn_outputs_get error ret={ret}");
                return false;
            }

            DetectResultGroup detect_result_group = new DetectResultGroup();
            List<float> out_scales = new List<float>();
            List<int> out_zps = new List<int>();
            for (int i = 0; i < io_num.output; ++i)
            {
                out_scales.Add(outputAttrs[i].Scale);
                out_zps.Add(outputAttrs[i].Zp);
            }
            byte[] buf0 = new byte[outputs[0].Size];
            Marshal.Copy(outputs[0].Buf, buf0, 0, (int)outputs[0].Size);
            sbyte[] buf00 = new sbyte[outputs[0].Size];
            Buffer.BlockCopy(buf0, 0, buf00, 0, (int)outputs[0].Size);
            byte[] buf1 = new byte[outputs[1].Size];
            Marshal.Copy(outputs[1].Buf, buf1, 0, (int)outputs[1].Size);
            sbyte[] buf11 = new sbyte[outputs[1].Size];
            Buffer.BlockCopy(buf1, 0, buf11, 0, (int)outputs[1].Size);
            byte[] buf2 = new byte[outputs[2].Size];
            Marshal.Copy(outputs[2].Buf, buf2, 0, (int)outputs[2].Size);
            sbyte[] buf22 = new sbyte[outputs[2].Size];
            Buffer.BlockCopy(buf2, 0, buf22, 0, (int)outputs[2].Size);
            return PostProcess(buf00, buf11, buf22,
                640, 640, 0.25f, 0.45f, out_zps, out_scales, detect_result_group);

        }


      public int Process(sbyte[] input, int[] anchor, int gridH, int gridW, int height, int width, int stride,
                         List<float> filterBoxes, List<float> objProbs, List<int> classId, float threshold,
                          int zp, float scale)
    {
        int validCount = 0;
        int gridLen = gridH * gridW;
        float thres = unsigmoid(threshold);
        int thresI8 = QntF32ToAffine(thres, zp, scale);

        for (int a = 0; a < 3; a++)
        {
            for (int i = 0; i < gridH; i++)
            {
                for (int j = 0; j < gridW; j++)
                {
                    sbyte boxConfidence = input[(15 * a + 4) * gridLen + i * gridW + j];

                    if (boxConfidence >= thresI8)
                    {
                        int offset = (15 * a) * gridLen + i * gridW + j;
                        float box_x = (float)(Sigmoid((float)(DeqntAffineToF32(input[offset], zp, scale))) * 2.0 - 0.5);
                        float box_y = (float)(Sigmoid((float)(DeqntAffineToF32(input[offset + gridLen], zp, scale))) * 2.0 - 0.5);
                        float box_w = (float)(Sigmoid((float)(DeqntAffineToF32(input[offset + 2 * gridLen], zp, scale))) * 2.0);
                        float box_h = (float)(Sigmoid((float)(DeqntAffineToF32(input[offset + 3 * gridLen], zp, scale))) * 2.0);
                        box_x = (box_x + j) * stride;
                        box_y = (box_y + i) * stride;
                        box_w = box_w * box_w * anchor[a * 2];
                        box_h = box_h * box_h * anchor[a * 2 + 1];
                        box_x -= (float)(box_w / 2.0);
                        box_y -= (float)(box_h / 2.0);


                        sbyte maxClassProbs = input[offset + 5 * gridLen];
                        int maxClassId = 0;
                        for (int k = 1; k < labels.Count; ++k)
                        {
                            sbyte prob = input[offset + (5 + k) * gridLen];
                            if (prob > maxClassProbs)
                            {
                                maxClassId = k;
                                maxClassProbs = prob;
                            }
                        }
                        float deqnt_cls_conf = Sigmoid((float)DeqntAffineToF32(maxClassProbs, zp, scale));
                        float deqnt_box_conf = Sigmoid((float)DeqntAffineToF32(boxConfidence, zp, scale));
                        float score = deqnt_cls_conf * deqnt_box_conf;
                        if (score > thres)
                        {
                            objProbs.Add(score);

                            classId.Add(maxClassId);
                            validCount++;
                            filterBoxes.Add(box_x);
                            filterBoxes.Add(box_y);
                            filterBoxes.Add(box_w);
                            filterBoxes.Add(box_h);
                        }
                    }
                }
            }
        }
        return validCount;
    }
        private bool PostProcess(
        sbyte[] input0,
        sbyte[] input1,
        sbyte[] input2,
        int model_in_h,
        int model_in_w,
        float conf_threshold,
        float nms_threshold,
        List<int> qnt_zps,
        List<float> qnt_scales,
        DetectResultGroup group)
        {
            int init = -1;
            if (init == -1)
            {
                init = 0;
            }
            group = new DetectResultGroup
            {
                Count = 0,
                Results = new List<DetectResult>() // Initialize the Results array with a new array of DetectResult objects
            };


            List<float> objProbs = new List<float>();
            List<float> filterBoxes = new List<float>();
            List<int> classId = new List<int>();
            int[] anchor0 = { 10, 13, 16, 30, 33, 23 };
            int[] anchor1 = { 30, 61, 62, 45, 59, 119 };
            int[] anchor2 = { 116, 90, 156, 198, 373, 326 };

            // stride 8
            int stride0 = 8;
            int grid_h0 = model_in_h / stride0;
            int grid_w0 = model_in_w / stride0;
            int validCount0 = 0;
            validCount0 = Process(input0, anchor0, grid_h0, grid_w0, model_in_h, model_in_w, stride0, filterBoxes, objProbs, classId, conf_threshold, qnt_zps[0], qnt_scales[0]);
            // stride 16
            int stride1 = 16;
            int grid_h1 = model_in_h / stride1;
            int grid_w1 = model_in_w / stride1;
            int validCount1 = Process(input1, anchor1, grid_h1, grid_w1, model_in_h, model_in_w, stride1, filterBoxes, objProbs, classId, conf_threshold, qnt_zps[1], qnt_scales[1]);
            // stride 32
            int stride2 = 32;
            int grid_h2 = model_in_h / stride2;
            int grid_w2 = model_in_w / stride2;
            int validCount2 = Process(input2, anchor2, grid_h2, grid_w2, model_in_h, model_in_w, stride2, filterBoxes, objProbs, classId, conf_threshold, qnt_zps[2], qnt_scales[2]);
            int validCount = validCount0 + validCount1 + validCount2;
            if (validCount <= 0)
            {
                Console.WriteLine("validCount = 0");
                return false;
            }

            List<int> indexArray = new List<int>();
            for (int i = 0; i < validCount; ++i)
            {
                indexArray.Add(i);
            }

            QuickSortIndiceInverse(objProbs, 0, validCount - 1, indexArray);

            HashSet<int> class_set = new HashSet<int>(classId);
            foreach (var c in class_set)
            {
                Nms(validCount, filterBoxes, classId, indexArray, c, nms_threshold);
            }

            int last_count = 0;
            group.Count = 0;
            for (int i = 0; i < validCount; ++i)
            {
                if (indexArray[i] == -1 || last_count >= 64)
                {
                    continue;
                }
                int n = indexArray[i];

                int id = classId[n];
                float obj_conf = objProbs[i];
                if(obj_conf > conf_threshold)
                {
                    string label = labels[id];
                    group.Results.Add(new DetectResult { Name = label.Substring(0, Math.Min(label.Length, 50)), Prop = obj_conf });

                    group.Results[last_count].Name = label.Substring(0, Math.Min(label.Length, 50));
                    Console.WriteLine($"{label.Substring(0, Math.Min(label.Length, 50))}: {obj_conf}");
                    last_count++;
                }
            }
            group.Count = last_count;
            if (group.Count == 0) System.Console.WriteLine($"{DateTime.Now.ToLongTimeString()} no glass");

            return group.Count > 0;
        }


        private static int Clip(float val, float min, float max)
        {
            float f = val <= min ? min : (val >= max ? max : val);
            return (int)f;
        }
        static int QntF32ToAffine(float f32, int zp, float scale)
        {
            float dst_val = (f32 / scale) + zp;
            int res = Clip(dst_val, -128, 127);
            return res;
        }
        static float DeqntAffineToF32(sbyte qnt, int zp, float scale) { return ((float)qnt - (float)zp) * scale; }

        private static float Sigmoid(float x)
        {
            return 1.0f / (1.0f + (float)Math.Exp(-x));
        }

        private static float unsigmoid(float y)
        {
            return (float)(-1.0 * Math.Log((1.0 / y) - 1.0));

        }

        public static int QuickSortIndiceInverse(List<float> input, int left, int right, List<int> indices)
        {
            float key;
            int keyIndex;
            int low = left;
            int high = right;

            if (left < right)
            {
                keyIndex = indices[left];
                key = input[left];

                while (low < high)
                {
                    while (low < high && input[high] <= key)
                    {
                        high--;
                    }
                    input[low] = input[high];
                    indices[low] = indices[high];

                    while (low < high && input[low] >= key)
                    {
                        low++;
                    }
                    input[high] = input[low];
                    indices[high] = indices[low];
                }

                input[low] = key;
                indices[low] = keyIndex;

                QuickSortIndiceInverse(input, left, low - 1, indices);
                QuickSortIndiceInverse(input, low + 1, right, indices);
            }
            return low;
        }

        public static int Nms(int validCount, List<float> outputLocations, List<int> classIds, List<int> order, int filterId, float threshold)
        {
            for (int i = 0; i < validCount; ++i)
            {
                if (order[i] == -1 || classIds[i] != filterId)
                {
                    continue;
                }
                int n = order[i];
                for (int j = i + 1; j < validCount; ++j)
                {
                    int m = order[j];
                    if (m == -1 || classIds[i] != filterId) // Changed classIds[i] to classIds[j] to match the logic of the original C++ code
                    {
                        continue;
                    }

                    float xmin0 = outputLocations[n * 4 + 0];
                    float ymin0 = outputLocations[n * 4 + 1];
                    float xmax0 = outputLocations[n * 4 + 0] + outputLocations[n * 4 + 2];
                    float ymax0 = outputLocations[n * 4 + 1] + outputLocations[n * 4 + 3];

                    float xmin1 = outputLocations[m * 4 + 0];
                    float ymin1 = outputLocations[m * 4 + 1];
                    float xmax1 = outputLocations[m * 4 + 0] + outputLocations[m * 4 + 2];
                    float ymax1 = outputLocations[m * 4 + 1] + outputLocations[m * 4 + 3];


                    float iou = CalculateOverlap(xmin0, ymin0, xmax0, ymax0, xmin1, ymin1, xmax1, ymax1);

                    if (iou > threshold)
                    {
                        order[j] = -1;
                    }
                }
            }
            return 0;
        }

        public static float CalculateOverlap(float xmin0, float ymin0, float xmax0, float ymax0, float xmin1, float ymin1, float xmax1, float ymax1)
        {
            float area0 = (xmax0 - xmin0) * (ymax0 - ymin0);
            float area1 = (xmax1 - xmin1) * (ymax1 - ymin1);

            float x_overlap = Math.Max(0, Math.Min(xmax0, xmax1) - Math.Max(xmin0, xmin1));
            float y_overlap = Math.Max(0, Math.Min(ymax0, ymax1) - Math.Max(ymin0, ymin1));

            float overlapArea = x_overlap * y_overlap;
            float unionArea = area0 + area1 - overlapArea;

            return unionArea == 0 ? 0 : overlapArea / unionArea;
        }
    }
}
