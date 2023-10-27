using BlankBartender.WebApi.External.Models;
using System.Runtime.InteropServices;

namespace BlankBartender.WebApi.External
{
    public static class RknnApi
    {
        const string LibraryPath = "librknn_api.so";

        [DllImport(LibraryPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int rknn_init(ref UIntPtr ctx, byte[] model_data, int model_data_size, int flag);

        // Add other RKNN API functions here...
        [DllImport(LibraryPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int rknn_query(UIntPtr ctx, RknnQueryCmd query, ref RknnInputOutputNum ioNum, int size);

        [DllImport(LibraryPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int rknn_query(UIntPtr ctx, RknnQueryCmd query, ref RknnTensorAttr info, int size);

        [DllImport(LibraryPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int rknn_destroy(UIntPtr ctx);

        [DllImport(LibraryPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int rknn_inputs_set(UIntPtr ctx, int length, RknnInput[] inputs);

        [DllImport(LibraryPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int rknn_run(UIntPtr ctx, IntPtr intPtr);

        [DllImport(LibraryPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int rknn_outputs_get(UIntPtr ctx, IntPtr length, IntPtr pointer, IntPtr pointer2);
    }
}
