namespace Un4seen.Bass.AddOn.Fx
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
    public sealed class BASS_BFX_PEAKEQ
    {
        public int lBand;
        public float fBandwidth;
        public float fQ;
        public float fCenter;
        public float fGain;
        public BASSFXChan lChannel;
        public BASS_BFX_PEAKEQ()
        {
            this.fBandwidth = 1f;
            this.fCenter = 1000f;
            this.lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
        }

        public BASS_BFX_PEAKEQ(int Band, float Bandwidth, float Q, float Center, float Gain)
        {
            this.fBandwidth = 1f;
            this.fCenter = 1000f;
            this.lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
            this.lBand = Band;
            this.fBandwidth = Bandwidth;
            this.fQ = Q;
            this.fCenter = Center;
            this.fGain = Gain;
        }

        public BASS_BFX_PEAKEQ(int Band, float Bandwidth, float Q, float Center, float Gain, BASSFXChan chans)
        {
            this.fBandwidth = 1f;
            this.fCenter = 1000f;
            this.lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
            this.lBand = Band;
            this.fBandwidth = Bandwidth;
            this.fQ = Q;
            this.fCenter = Center;
            this.fGain = Gain;
            this.lChannel = chans;
        }
    }
}

