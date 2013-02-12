namespace Un4seen.Bass.AddOn.Fx
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
    public sealed class BASS_BFX_COMPRESSOR2
    {
        public float fGain;
        public float fThreshold;
        public float fRatio;
        public float fAttack;
        public float fRelease;
        public BASSFXChan lChannel;
        public BASS_BFX_COMPRESSOR2()
        {
            this.fGain = 5f;
            this.fThreshold = -15f;
            this.fRatio = 3f;
            this.fAttack = 20f;
            this.fRelease = 200f;
            this.lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
        }

        public BASS_BFX_COMPRESSOR2(float Gain, float Threshold, float Ratio, float Attack, float Release, BASSFXChan chans)
        {
            this.fGain = 5f;
            this.fThreshold = -15f;
            this.fRatio = 3f;
            this.fAttack = 20f;
            this.fRelease = 200f;
            this.lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
            this.fGain = Gain;
            this.fThreshold = Threshold;
            this.fRatio = Ratio;
            this.fAttack = Attack;
            this.fRelease = Release;
            this.lChannel = chans;
        }

        public void Calculate0dBGain()
        {
            this.fGain = (this.fThreshold / 2f) * ((1f / this.fRatio) - 1f);
        }

        public void Preset_Default()
        {
            this.fThreshold = -15f;
            this.fRatio = 3f;
            this.fGain = 5f;
            this.fAttack = 20f;
            this.fRelease = 200f;
        }

        public void Preset_Soft()
        {
            this.fThreshold = -15f;
            this.fRatio = 2f;
            this.fGain = 3.7f;
            this.fAttack = 24f;
            this.fRelease = 800f;
        }

        public void Preset_Soft2()
        {
            this.fThreshold = -18f;
            this.fRatio = 3f;
            this.fGain = 6f;
            this.fAttack = 24f;
            this.fRelease = 800f;
        }

        public void Preset_Medium()
        {
            this.fThreshold = -20f;
            this.fRatio = 4f;
            this.fGain = 7.5f;
            this.fAttack = 16f;
            this.fRelease = 500f;
        }

        public void Preset_Hard()
        {
            this.fThreshold = -23f;
            this.fRatio = 8f;
            this.fGain = 10f;
            this.fAttack = 12f;
            this.fRelease = 400f;
        }

        public void Preset_Hard2()
        {
            this.fThreshold = -18f;
            this.fRatio = 9f;
            this.fGain = 8f;
            this.fAttack = 12f;
            this.fRelease = 200f;
        }

        public void Preset_HardCommercial()
        {
            this.fThreshold = -20f;
            this.fRatio = 10f;
            this.fGain = 9f;
            this.fAttack = 8f;
            this.fRelease = 250f;
        }
    }
}

