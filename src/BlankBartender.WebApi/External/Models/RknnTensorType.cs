namespace BlankBartender.WebApi.External.Models
{
    public enum RknnTensorType
    {
        RKNN_TENSOR_FLOAT32 = 0,                            /* data type is float32. */
        RKNN_TENSOR_FLOAT16,                                /* data type is float16. */
        RKNN_TENSOR_INT8,                                   /* data type is int8. */
        RKNN_TENSOR_UINT8,                                  /* data type is uint8. */
        RKNN_TENSOR_INT16,                                  /* data type is int16. */
        RKNN_TENSOR_UINT16,                                 /* data type is uint16. */
        RKNN_TENSOR_INT32,                                  /* data type is int32. */
        RKNN_TENSOR_UINT32,                                 /* data type is uint32. */
        RKNN_TENSOR_INT64,                                  /* data type is int64. */
        RKNN_TENSOR_BOOL,

        RKNN_TENSOR_TYPE_MAX
    }
}
