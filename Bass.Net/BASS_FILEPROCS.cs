namespace Un4seen.Bass
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
    public sealed class BASS_FILEPROCS
    {
        public FILECLOSEPROC close;
        public FILELENPROC length;
        public FILEREADPROC read;
        public FILESEEKPROC seek;
        public BASS_FILEPROCS(FILECLOSEPROC closeCallback, FILELENPROC lengthCallback, FILEREADPROC readCallback, FILESEEKPROC seekCallback)
        {
            this.close = closeCallback;
            this.length = lengthCallback;
            this.read = readCallback;
            this.seek = seekCallback;
        }
    }
}

