using System.Runtime.InteropServices;

namespace BlankBartender.WebApi.External.Models
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RknnInput
    {
        public uint Index;
        public IntPtr Buf;
        public uint Size;
        public int PassThrough; // pass through mode.
        public RknnTensorType Type;
        public RknnTensorFormat Fmt;
    }
}
