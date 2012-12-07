namespace Un4seen.Bass
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
    public sealed class BASS_SAMPLE
    {
        public int freq;
        public float volume;
        public float pan;
        public BASSFlag flags;
        public int length;
        public int max;
        public int origres;
        public int chans;
        public int mingap;
        public BASS3DMode mode3d;
        public float mindist;
        public float maxdist;
        public int iangle;
        public int oangle;
        public float outvol;
        public BASSVam vam;
        public int priority;
        public BASS_SAMPLE()
        {
            this.freq = 0xac44;
            this.volume = 1f;
            this.max = 1;
            this.chans = 2;
            this.outvol = 1f;
            this.vam = BASSVam.BASS_VAM_HARDWARE;
        }

        public BASS_SAMPLE(int Freq, float Volume, float Pan, BASSFlag Flags, int Length, int Max, int OrigRes, int Chans, int MinGap, BASS3DMode Flag3D, float MinDist, float MaxDist, int IAngle, int OAngle, float OutVol, BASSVam FlagsVam, int Priority)
        {
            this.freq = 0xac44;
            this.volume = 1f;
            this.max = 1;
            this.chans = 2;
            this.outvol = 1f;
            this.vam = BASSVam.BASS_VAM_HARDWARE;
            this.freq = Freq;
            this.volume = Volume;
            this.pan = Pan;
            this.flags = Flags;
            this.length = Length;
            this.max = Max;
            this.origres = OrigRes;
            this.chans = Chans;
            this.mingap = MinGap;
            this.mode3d = Flag3D;
            this.mindist = MinDist;
            this.maxdist = MaxDist;
            this.iangle = IAngle;
            this.oangle = OAngle;
            this.outvol = OutVol;
            this.vam = FlagsVam;
            this.priority = Priority;
        }

        public override string ToString()
        {
            return string.Format("Frequency={0}, Volume={1}, Pan={2}", this.freq, this.volume, this.pan);
        }
    }
}

