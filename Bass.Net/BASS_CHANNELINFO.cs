namespace Un4seen.Bass
{
    using System;

    [Serializable]
    public sealed class BASS_CHANNELINFO
    {
        internal c a;
        public int chans;
        public BASSChannelType ctype;
        public string filename = string.Empty;
        public BASSFlag flags;
        public int freq;
        public int origres;
        public int plugin;
        public int sample;

//        public override string ToString()
//        {
//            return string.Format("{0}, {1}Hz, {2}, {3}bit", new object[] { Utils.BASSChannelTypeToString(this.ctype), this.freq, Utils.ChannelNumberToString(this.chans), (this.origres == 0) ? (this.Is32bit ? 0x20 : (this.Is8bit ? 8 : 0x10)) : this.origres });
//        }

        public bool Is32bit
        {
            get
            {
                return ((this.flags & BASSFlag.BASS_MUSIC_FLOAT) != BASSFlag.BASS_DEFAULT);
            }
        }

        public bool Is8bit
        {
            get
            {
                return ((this.flags & BASSFlag.BASS_FX_BPM_BKGRND) != BASSFlag.BASS_DEFAULT);
            }
        }

        public bool IsDecodingChannel
        {
            get
            {
                return ((this.flags & BASSFlag.BASS_MUSIC_DECODE) != BASSFlag.BASS_DEFAULT);
            }
        }
    }
}

