namespace BlankBartender.WebApi.External.Models
{
    public enum RknnTensorFormat
    {
        RKNN_TENSOR_NCHW = 0,                               /* data format is NCHW. */
        RKNN_TENSOR_NHWC,                                   /* data format is NHWC. */
        RKNN_TENSOR_NC1HWC2,                                /* data format is NC1HWC2. */
        RKNN_TENSOR_UNDEFINED,

        RKNN_TENSOR_FORMAT_MAX
    }
}
