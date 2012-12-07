namespace Un4seen.Bass
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
    internal struct c
    {
        public int a;
        public int b;
        public BASSFlag flag;
        public BASSChannelType d;
        public int e;
        public int f;
        public int g;
        public IntPtr h;
    }
}

