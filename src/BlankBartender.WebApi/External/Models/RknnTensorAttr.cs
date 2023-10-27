using System.Runtime.InteropServices;

namespace BlankBartender.WebApi.External.Models
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct RknnTensorAttr
    {
        public uint Index;
        public uint NDims;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] // Assuming RKNN_MAX_DIMS is 4, adjust as necessary
        public uint[] Dims;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] // Assuming RKNN_MAX_NAME_LEN is 256, adjust as necessary
        public string Name;
        public uint NElems;
        public uint Size;
        public RknnTensorFormat Fmt;
        public RknnTensorType Type;
        public RknnTensorQntType QntType;
        public sbyte Fl;
        public int Zp;
        public float Scale;
        public uint WStride;
        public uint SizeWithStride;
        public byte PassThrough;
        public uint HStride;
    }
}
