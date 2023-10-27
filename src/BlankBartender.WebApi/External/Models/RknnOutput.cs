using System.Runtime.InteropServices;

namespace BlankBartender.WebApi.External.Models
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RknnOutput
    {
        public byte Want_Float; // uint8_t want_float
        public byte IsPreAlloc; // uint8_t is_prealloc
        public uint Index; // uint32_t index
        public IntPtr Buf; // void* buf
        public uint Size; // uint32_t size
    }
}
