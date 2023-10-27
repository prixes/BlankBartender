namespace BlankBartender.WebApi.External.Models
{
    public enum RknnQueryCmd
    {
        InOutNum = 0, // RKNN_QUERY_IN_OUT_NUM
        InputAttr = 1, // RKNN_QUERY_INPUT_ATTR
        OutputAttr = 2, // RKNN_QUERY_OUTPUT_ATTR
        PerfDetail = 3, // RKNN_QUERY_PERF_DETAIL
        PerfRun = 4, // RKNN_QUERY_PERF_RUN
        SdkVersion = 5, // RKNN_QUERY_SDK_VERSION
        MemSize = 6, // RKNN_QUERY_MEM_SIZE
        CustomString = 7, // RKNN_QUERY_CUSTOM_STRING
        NativeInputAttr = 8, // RKNN_QUERY_NATIVE_INPUT_ATTR
        NativeOutputAttr = 9, // RKNN_QUERY_NATIVE_OUTPUT_ATTR
        NativeNc1Hwc2InputAttr = 8, // RKNN_QUERY_NATIVE_NC1HWC2_INPUT_ATTR
        NativeNc1Hwc2OutputAttr = 9, // RKNN_QUERY_NATIVE_NC1HWC2_OUTPUT_ATTR
        NativeNhwcInputAttr = 10, // RKNN_QUERY_NATIVE_NHWC_INPUT_ATTR
        NativeNhwcOutputAttr = 11, // RKNN_QUERY_NATIVE_NHWC_OUTPUT_ATTR
        DeviceMemInfo = 12, // RKNN_QUERY_DEVICE_MEM_INFO
        InputDynamicRange = 13, // RKNN_QUERY_INPUT_DYNAMIC_RANGE
        CurrentInputAttr = 14, // RKNN_QUERY_CURRENT_INPUT_ATTR
        CurrentOutputAttr = 15, // RKNN_QUERY_CURRENT_OUTPUT_ATTR
        CurrentNativeInputAttr = 16, // RKNN_QUERY_CURRENT_NATIVE_INPUT_ATTR
        CurrentNativeOutputAttr = 17, // RKNN_QUERY_CURRENT_NATIVE_OUTPUT_ATTR
        CmdMax // RKNN_QUERY_CMD_MAX
    }
}
