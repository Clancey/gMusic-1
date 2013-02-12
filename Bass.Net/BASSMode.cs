﻿namespace Un4seen.Bass
{
    using System;

    [Flags]
    public enum BASSMode
    {
        BASS_MIDI_DECAYSEEK = 0x4000,
        BASS_MIXER_NORAMPIN = 0x800000,
        BASS_MUSIC_POSRESET = 0x8000,
        BASS_MUSIC_POSRESETEX = 0x400000,
        BASS_POS_BYTES = 0,
        BASS_POS_DECODE = 0x10000000,
        BASS_POS_DECODETO = 0x20000000,
        BASS_POS_MIDI_TICK = 2,
        BASS_POS_MUSIC_ORDERS = 1
    }
}

