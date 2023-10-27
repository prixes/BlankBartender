namespace BlankBartender.WebApi.External.Models
{
    public enum RknnTensorQntType
    {
        None = 0,         // RKNN_TENSOR_QNT_NONE
        Dfp,              // RKNN_TENSOR_QNT_DFP
        AffineAsymmetric, // RKNN_TENSOR_QNT_AFFINE_ASYMMETRIC
        Max               // RKNN_TENSOR_QNT_MAX
    }
}
