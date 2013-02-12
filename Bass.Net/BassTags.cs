namespace Un4seen.Bass.AddOn.Tags
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using Un4seen.Bass;

    [Serializable]
    public sealed class BassTags
    {
        public static bool EvalNativeTAGs = true;
        public static bool EvalNativeTAGsBEXT = true;
        public static bool EvalNativeTAGsCART = true;
        public static readonly string[] ID3v1Genre = new string[] { 
            "Blues", "Classic Rock", "Country", "Dance", "Disco", "Funk", "Grunge", "Hip-Hop", "Jazz", "Metal", "New Age", "Oldies", "Other", "Pop", "R&B", "Rap", 
            "Reggae", "Rock", "Techno", "Industrial", "Alternative", "Ska", "Death Metal", "Pranks", "Soundtrack", "Euro-Techno", "Ambient", "Trip-Hop", "Vocal", "Jazz+Funk", "Fusion", "Trance", 
            "Classical", "Instrumental", "Acid", "House", "Game", "Sound Clip", "Gospel", "Noise", "Alternative Rock", "Bass", "Soul", "Punk", "Space", "Meditative", "Instrumental Pop", "Instrumental Rock", 
            "Ethnic", "Gothic", "Darkwave", "Techno-Industrial", "Electronic", "Pop-Folk", "Eurodance", "Dream", "Southern Rock", "Comedy", "Cult", "Gangsta", "Top 40", "Christian Rap", "Pop/Funk", "Jungle", 
            "Native American", "Cabaret", "New Wave", "Psychedelic", "Rave", "Showtunes", "Trailer", "Lo-Fi", "Tribal", "Acid Punk", "Acid Jazz", "Polka", "Retro", "Musical", "Rock & Roll", "Hard Rock", 
            "Folk", "Folk/Rock", "National Folk", "Swing", "Fusion", "Bebob", "Latin", "Revival", "Celtic", "Bluegrass", "Avantgarde", "Gothic Rock", "Progressive Rock", "Psychedelic Rock", "Symphonic Rock", "Slow Rock", 
            "Big Band", "Chorus", "Easy Listening", "Acoustic", "Humour", "Speech", "Chanson", "Opera", "Chamber Music", "Sonata", "Symphony", "Booty Bass", "Primus", "Porn Groove", "Satire", "Slow Jam", 
            "Club", "Tango", "Samba", "Folklore", "Ballad", "Power Ballad", "Rhythmic Soul", "Freestyle", "Duet", "Punk Rock", "Drum Solo", "A Cappella", "Euro-House", "Dance Hall", "Goa", "Drum & Bass", 
            "Club-House", "Hardcore", "Terror", "Indie", "BritPop", "Negerpunk", "Polsk Punk", "Beat", "Christian Gangsta Rap", "Heavy Metal", "Black Metal", "Crossover", "Contemporary Christian", "Christian Rock", "Merengue", "Salsa", 
            "Thrash Metal", "Anime", "Jpop", "Synthpop"
         };
        public static bool ReadPictureTAGs = true;

        private BassTags()
        {
        }

        private static IntPtr a(int A_0, BASS_CHANNELINFO A_1, out BASSTag A_2)
        {
            IntPtr zero = IntPtr.Zero;
            A_2 = BASSTag.BASS_TAG_UNKNOWN;
            if ((A_0 == 0) || (A_1 == null))
            {
                return zero;
            }
            BASSChannelType ctype = A_1.ctype;
            if ((ctype & BASSChannelType.BASS_CTYPE_STREAM_WAV) > BASSChannelType.BASS_CTYPE_UNKNOWN)
            {
                ctype = BASSChannelType.BASS_CTYPE_STREAM_WAV;
            }
            switch (ctype)
            {
                case BASSChannelType.BASS_CTYPE_STREAM_WMA:
                    zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(A_0, BASSTag.BASS_TAG_WMA);
                    A_2 = BASSTag.BASS_TAG_WMA;
                    return zero;

                case BASSChannelType.BASS_CTYPE_STREAM_WMA_MP3:
                case BASSChannelType.BASS_CTYPE_STREAM_MP1:
                case BASSChannelType.BASS_CTYPE_STREAM_MP2:
                case BASSChannelType.BASS_CTYPE_STREAM_MP3:
                    zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(A_0, BASSTag.BASS_TAG_ID3V2);
                    if (zero == IntPtr.Zero)
                    {
                        zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(A_0, BASSTag.BASS_TAG_ID3);
                        if (zero == IntPtr.Zero)
                        {
                            zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(A_0, BASSTag.BASS_TAG_APE);
                            if (zero == IntPtr.Zero)
                            {
                                zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(A_0, BASSTag.BASS_TAG_RIFF_BEXT);
                                if (zero != IntPtr.Zero)
                                {
                                    A_2 = BASSTag.BASS_TAG_RIFF_BEXT;
                                    return zero;
                                }
                                A_2 = BASSTag.BASS_TAG_ID3V2;
                                return zero;
                            }
                            A_2 = BASSTag.BASS_TAG_APE;
                            return zero;
                        }
                        A_2 = BASSTag.BASS_TAG_ID3;
                        return zero;
                    }
                    A_2 = BASSTag.BASS_TAG_ID3V2;
                    return zero;

                case BASSChannelType.BASS_CTYPE_STREAM_WINAMP:
                    zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(A_0, BASSTag.BASS_TAG_ID3V2);
                    if (!(zero == IntPtr.Zero))
                    {
                        A_2 = BASSTag.BASS_TAG_ID3V2;
                        return zero;
                    }
                    zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(A_0, BASSTag.BASS_TAG_APE);
                    if (!(zero == IntPtr.Zero))
                    {
                        A_2 = BASSTag.BASS_TAG_APE;
                        return zero;
                    }
                    zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(A_0, BASSTag.BASS_TAG_OGG);
                    if (!(zero == IntPtr.Zero))
                    {
                        A_2 = BASSTag.BASS_TAG_OGG;
                        return zero;
                    }
                    zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(A_0, BASSTag.BASS_TAG_ID3);
                    if (zero != IntPtr.Zero)
                    {
                        A_2 = BASSTag.BASS_TAG_ID3;
                        return zero;
                    }
                    A_2 = BASSTag.BASS_TAG_ID3V2;
                    return zero;

                case BASSChannelType.BASS_CTYPE_STREAM_OGG:
                case BASSChannelType.BASS_CTYPE_STREAM_FLAC:
                case BASSChannelType.BASS_CTYPE_STREAM_FLAC_OGG:
                case BASSChannelType.BASS_CTYPE_STREAM_OPUS:
                    zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(A_0, BASSTag.BASS_TAG_OGG);
                    if (zero == IntPtr.Zero)
                    {
                        zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(A_0, BASSTag.BASS_TAG_APE);
                        if (zero != IntPtr.Zero)
                        {
                            A_2 = BASSTag.BASS_TAG_APE;
                            return zero;
                        }
                        A_2 = BASSTag.BASS_TAG_OGG;
                        return zero;
                    }
                    A_2 = BASSTag.BASS_TAG_OGG;
                    return zero;

                case BASSChannelType.BASS_CTYPE_STREAM_AIFF:
                case BASSChannelType.BASS_CTYPE_STREAM_WAV:
                    zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(A_0, BASSTag.BASS_TAG_RIFF_INFO);
                    if (zero == IntPtr.Zero)
                    {
                        zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(A_0, BASSTag.BASS_TAG_RIFF_BEXT);
                        if (zero == IntPtr.Zero)
                        {
                            zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(A_0, BASSTag.BASS_TAG_ID3V2);
                            if (zero != IntPtr.Zero)
                            {
                                A_2 = BASSTag.BASS_TAG_ID3V2;
                                return zero;
                            }
                            A_2 = BASSTag.BASS_TAG_RIFF_INFO;
                            return zero;
                        }
                        A_2 = BASSTag.BASS_TAG_RIFF_BEXT;
                        return zero;
                    }
                    A_2 = BASSTag.BASS_TAG_RIFF_INFO;
                    return zero;

                case BASSChannelType.BASS_CTYPE_STREAM_MF:
                    zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(A_0, BASSTag.BASS_TAG_MF);
                    if (!(zero == IntPtr.Zero))
                    {
                        A_2 = BASSTag.BASS_TAG_MF;
                        return zero;
                    }
                    zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(A_0, BASSTag.BASS_TAG_MP4);
                    if (!(zero == IntPtr.Zero))
                    {
                        A_2 = BASSTag.BASS_TAG_MP4;
                        return zero;
                    }
                    zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(A_0, BASSTag.BASS_TAG_ID3V2);
                    if (!(zero == IntPtr.Zero))
                    {
                        A_2 = BASSTag.BASS_TAG_ID3V2;
                        return zero;
                    }
                    zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(A_0, BASSTag.BASS_TAG_APE);
                    if (!(zero != IntPtr.Zero))
                    {
                        A_2 = BASSTag.BASS_TAG_MF;
                        return zero;
                    }
                    A_2 = BASSTag.BASS_TAG_APE;
                    return zero;

                case BASSChannelType.BASS_CTYPE_MUSIC_MO3:
                case BASSChannelType.BASS_CTYPE_MUSIC_MOD:
                case BASSChannelType.BASS_CTYPE_MUSIC_MTM:
                case BASSChannelType.BASS_CTYPE_MUSIC_S3M:
                case BASSChannelType.BASS_CTYPE_MUSIC_XM:
                case BASSChannelType.BASS_CTYPE_MUSIC_IT:
                    zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(A_0, BASSTag.BASS_TAG_MUSIC_NAME);
                    A_2 = BASSTag.BASS_TAG_MUSIC_NAME;
                    return zero;

                case BASSChannelType.BASS_CTYPE_STREAM_WV:
                case BASSChannelType.BASS_CTYPE_STREAM_WV_H:
                case BASSChannelType.BASS_CTYPE_STREAM_WV_L:
                case BASSChannelType.BASS_CTYPE_STREAM_WV_LH:
                case BASSChannelType.BASS_CTYPE_STREAM_OFR:
                case BASSChannelType.BASS_CTYPE_STREAM_APE:
                case BASSChannelType.BASS_CTYPE_STREAM_SPX:
                case BASSChannelType.BASS_CTYPE_STREAM_MPC:
                case BASSChannelType.BASS_CTYPE_STREAM_TTA:
                    zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(A_0, BASSTag.BASS_TAG_APE);
                    if (zero == IntPtr.Zero)
                    {
                        zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(A_0, BASSTag.BASS_TAG_OGG);
                        if (zero == IntPtr.Zero)
                        {
                            zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(A_0, BASSTag.BASS_TAG_ID3V2);
                            if (zero == IntPtr.Zero)
                            {
                                zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(A_0, BASSTag.BASS_TAG_ID3);
                                if (zero != IntPtr.Zero)
                                {
                                    A_2 = BASSTag.BASS_TAG_ID3;
                                    return zero;
                                }
                                A_2 = BASSTag.BASS_TAG_APE;
                                return zero;
                            }
                            A_2 = BASSTag.BASS_TAG_ID3V2;
                            return zero;
                        }
                        A_2 = BASSTag.BASS_TAG_OGG;
                        return zero;
                    }
                    A_2 = BASSTag.BASS_TAG_APE;
                    return zero;

                case BASSChannelType.BASS_CTYPE_STREAM_MIDI:
                    zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(A_0, BASSTag.BASS_TAG_MIDI_TRACK);
                    if (!(zero == IntPtr.Zero))
                    {
                        A_2 = BASSTag.BASS_TAG_MIDI_TRACK;
                        return zero;
                    }
                    zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(A_0, BASSTag.BASS_TAG_RIFF_INFO);
                    if (zero != IntPtr.Zero)
                    {
                        A_2 = BASSTag.BASS_TAG_RIFF_INFO;
                        return zero;
                    }
                    A_2 = BASSTag.BASS_TAG_MIDI_TRACK;
                    return zero;

                case BASSChannelType.BASS_CTYPE_STREAM_AAC:
                case BASSChannelType.BASS_CTYPE_STREAM_MP4:
                case BASSChannelType.BASS_CTYPE_STREAM_ALAC:
                    zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(A_0, BASSTag.BASS_TAG_MP4);
                    if (zero == IntPtr.Zero)
                    {
                        zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(A_0, BASSTag.BASS_TAG_ID3V2);
                        if (zero == IntPtr.Zero)
                        {
                            zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(A_0, BASSTag.BASS_TAG_APE);
                            if (zero == IntPtr.Zero)
                            {
                                zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(A_0, BASSTag.BASS_TAG_OGG);
                                if (zero != IntPtr.Zero)
                                {
                                    A_2 = BASSTag.BASS_TAG_OGG;
                                    return zero;
                                }
                                A_2 = BASSTag.BASS_TAG_MP4;
                                return zero;
                            }
                            A_2 = BASSTag.BASS_TAG_APE;
                            return zero;
                        }
                        A_2 = BASSTag.BASS_TAG_ID3V2;
                        return zero;
                    }
                    A_2 = BASSTag.BASS_TAG_MP4;
                    return zero;
            }
            return IntPtr.Zero;
        }

        private static bool b(IntPtr A_0, TAG_INFO A_1)
        {
            if ((A_0 == IntPtr.Zero) || (A_1 == null))
            {
                return false;
            }
            try
            {
                A_1.b();
                int num = 0;
                int num2 = 0;
                Un4seen.Bass.AddOn.Tags.c c = new Un4seen.Bass.AddOn.Tags.c(A_0);
                while (c.q())
                {
                    string str = c.s();
                    short num3 = c.o();
                    object obj2 = c.p();
                    if ((str.Length > 0) && (obj2 is string))
                    {
                        A_1.c(string.Format("{0}={1}", str, obj2));
                    }
                    else
                    {
                        if (((str == "POPM") || (str == "POP")) && (obj2 is byte))
                        {
                            if (num2 == 0)
                            {
                                A_1.c(string.Format("POPM={0}", obj2));
                            }
                            num2++;
                            continue;
                        }
                        if ((ReadPictureTAGs && ((str == "APIC") || (str == "PIC"))) && (obj2 is byte[]))
                        {
                            num++;
                            A_1.AddPicture(c.w(obj2 as byte[], num3, A_1.PictureCount, str == "PIC"));
                        }
                    }
                }
                c.r();
                if (ReadPictureTAGs && EvalNativeTAGs)
                {
                    A_1.AddNativeTag("APIC", num);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static TAG_INFO BASS_TAG_GetFromFile(string file)
        {
            return BASS_TAG_GetFromFile(file, true, true);
        }

        public static bool BASS_TAG_GetFromFile(int stream, TAG_INFO tags)
        {
            if ((stream == 0) || (tags == null))
            {
                return false;
            }
            bool flag = false;
            BASS_CHANNELINFO info = new BASS_CHANNELINFO();
            if (Un4seen.Bass.Bass.BASS_ChannelGetInfo(stream, info))
            {
                tags.channelinfo = info;
                BASSTag tag = BASSTag.BASS_TAG_UNKNOWN;
                IntPtr data = a(stream, info, out tag);
                tags.tagType = tag;
                if (data != IntPtr.Zero)
                {
                    switch (tag)
                    {
                        case BASSTag.BASS_TAG_RIFF_INFO:
                        {
                            flag = tags.UpdateFromMETA(data, BassNet.UseRiffInfoUTF8, false);
                            IntPtr ptr10 = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_RIFF_BEXT);
                            if (ptr10 != IntPtr.Zero)
                            {
                                e(ptr10, tags);
                            }
                            IntPtr ptr11 = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_RIFF_CART);
                            if (ptr11 != IntPtr.Zero)
                            {
                                d(ptr11, tags);
                            }
                            IntPtr ptr12 = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_ID3V2);
                            if (ptr12 != IntPtr.Zero)
                            {
                                b(ptr12, tags);
                            }
                            break;
                        }
                        case BASSTag.BASS_TAG_RIFF_BEXT:
                        case BASSTag.BASS_TAG_RIFF_CART:
                        {
                            IntPtr ptr13 = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_RIFF_INFO);
                            if (ptr13 != IntPtr.Zero)
                            {
                                tags.UpdateFromMETA(ptr13, BassNet.UseRiffInfoUTF8, false);
                            }
                            IntPtr ptr14 = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_RIFF_CART);
                            if (ptr14 != IntPtr.Zero)
                            {
                                d(ptr14, tags);
                            }
                            IntPtr ptr15 = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_ID3V2);
                            if (ptr15 != IntPtr.Zero)
                            {
                                b(ptr15, tags);
                                tags.a();
                            }
                            flag = e(data, tags);
                            break;
                        }
                        case BASSTag.BASS_TAG_MUSIC_NAME:
                            tags.title = Un4seen.Bass.Bass.BASS_ChannelGetMusicName(stream);
                            tags.artist = Un4seen.Bass.Bass.BASS_ChannelGetMusicMessage(stream);
                            flag = true;
                            break;

                        case BASSTag.BASS_TAG_MIDI_TRACK:
                        {
                            int num3 = 0;
                            while (true)
                            {
                                IntPtr ptr16 = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, (BASSTag) (0x11000 + num3));
                                if (!(ptr16 != IntPtr.Zero))
                                {
                                    if (!flag && (tags.NativeTags.Length > 0))
                                    {
                                        flag = true;
                                        if (tags.NativeTags.Length > 0)
                                        {
                                            tags.title = tags.NativeTags[0].Trim();
                                        }
                                        if (tags.NativeTags.Length > 1)
                                        {
                                            tags.artist = tags.NativeTags[1].Trim();
                                        }
                                    }
                                    break;
                                }
                                flag |= tags.UpdateFromMETA(ptr16, false, false);
                                num3++;
                            }
                        }
                        case BASSTag.BASS_TAG_ID3:
                        {
                            IntPtr ptr4 = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_APE);
                            if (ptr4 != IntPtr.Zero)
                            {
                                tags.UpdateFromMETA(ptr4, true, false);
                                if (ReadPictureTAGs)
                                {
                                    f(stream, tags);
                                }
                                tags.a();
                            }
                            flag = c(data, tags);
                            break;
                        }
                        case BASSTag.BASS_TAG_ID3V2:
                        {
                            IntPtr ptr2 = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_ID3);
                            if (ptr2 != IntPtr.Zero)
                            {
                                c(ptr2, tags);
                                tags.a();
                            }
                            IntPtr ptr3 = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_APE);
                            if (ptr3 != IntPtr.Zero)
                            {
                                tags.UpdateFromMETA(ptr3, true, false);
                                if (ReadPictureTAGs)
                                {
                                    f(stream, tags);
                                }
                                tags.a();
                            }
                            flag = b(data, tags);
                            break;
                        }
                        case BASSTag.BASS_TAG_OGG:
                        {
                            IntPtr ptr5 = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_APE);
                            if (ptr5 != IntPtr.Zero)
                            {
                                tags.UpdateFromMETA(ptr5, true, false);
                                if (ReadPictureTAGs)
                                {
                                    f(stream, tags);
                                }
                                tags.a();
                            }
                            if (ReadPictureTAGs && ((info.ctype == BASSChannelType.BASS_CTYPE_STREAM_FLAC) || (info.ctype == BASSChannelType.BASS_CTYPE_STREAM_FLAC_OGG)))
                            {
                                BASS_TAG_FLAC_PICTURE bass_tag_flac_picture;
                                for (int i = 0; (bass_tag_flac_picture = BASS_TAG_FLAC_PICTURE.GetTag(stream, i)) != null; i++)
                                {
                                    tags.AddPicture(new TagPicture(i, bass_tag_flac_picture.Mime, TagPicture.PICTURE_TYPE.FrontAlbumCover, bass_tag_flac_picture.Desc, bass_tag_flac_picture.Data));
                                }
                            }
                            flag = tags.UpdateFromMETA(data, true, false);
                            break;
                        }
                        case BASSTag.BASS_TAG_APE:
                        {
                            IntPtr ptr6 = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_ID3);
                            if (ptr6 != IntPtr.Zero)
                            {
                                c(ptr6, tags);
                                tags.a();
                            }
                            IntPtr ptr7 = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_ID3V2);
                            if (ptr7 != IntPtr.Zero)
                            {
                                b(ptr7, tags);
                                tags.a();
                            }
                            if (ReadPictureTAGs && ((info.ctype == BASSChannelType.BASS_CTYPE_STREAM_FLAC) || (info.ctype == BASSChannelType.BASS_CTYPE_STREAM_FLAC_OGG)))
                            {
                                BASS_TAG_FLAC_PICTURE bass_tag_flac_picture2;
                                for (int j = 0; (bass_tag_flac_picture2 = BASS_TAG_FLAC_PICTURE.GetTag(stream, j)) != null; j++)
                                {
                                    tags.AddPicture(new TagPicture(j, bass_tag_flac_picture2.Mime, TagPicture.PICTURE_TYPE.FrontAlbumCover, bass_tag_flac_picture2.Desc, bass_tag_flac_picture2.Data));
                                }
                            }
                            flag = tags.UpdateFromMETA(data, true, false);
                            if (ReadPictureTAGs)
                            {
                                f(stream, tags);
                            }
                            break;
                        }
                        case BASSTag.BASS_TAG_MP4:
                        {
                            IntPtr ptr8 = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_APE);
                            if (ptr8 != IntPtr.Zero)
                            {
                                tags.UpdateFromMETA(ptr8, true, false);
                                if (ReadPictureTAGs)
                                {
                                    f(stream, tags);
                                }
                                tags.a();
                            }
                            IntPtr ptr9 = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_ID3V2);
                            if (ptr9 != IntPtr.Zero)
                            {
                                b(ptr9, tags);
                                tags.a();
                            }
                            flag = tags.UpdateFromMETA(data, true, false);
                            break;
                        }
                        case BASSTag.BASS_TAG_WMA:
                            flag = tags.UpdateFromMETA(data, true, false);
                            break;

                        case BASSTag.BASS_TAG_MF:
                            flag = tags.UpdateFromMETA(data, true, false);
                            break;
                    }
                }
                tags.duration = Un4seen.Bass.Bass.BASS_ChannelBytes2Seconds(stream, Un4seen.Bass.Bass.BASS_ChannelGetLength(stream));
                if (tags.duration < 0.0)
                {
                    tags.duration = 0.0;
                }
                if (tags.bitrate == 0)
                {
                    long num4 = Un4seen.Bass.Bass.BASS_StreamGetFilePosition(stream, BASSStreamFilePosition.BASS_FILEPOS_END);
                    tags.bitrate = (int) ((((double) num4) / (125.0 * tags.duration)) + 0.5);
                }
            }
            return flag;
        }

        public static TAG_INFO BASS_TAG_GetFromFile(string file, bool setDefaultTitle, bool prescan)
        {
            if (!string.IsNullOrEmpty(file))
            {
                TAG_INFO tags = new TAG_INFO(file, setDefaultTitle);
                if (BASS_TAG_GetFromFile(file, prescan, tags))
                {
                    return tags;
                }
            }
            return null;
        }

        public static bool BASS_TAG_GetFromFile(string file, bool prescan, TAG_INFO tags)
        {
            if (tags != null)
            {
                int stream = Un4seen.Bass.Bass.BASS_StreamCreateFile(file, 0L, 0L, BASSFlag.BASS_MUSIC_DECODE | (prescan ? BASSFlag.BASS_DSHOW_STREAM_VIDEOPROC : BASSFlag.BASS_DEFAULT));
                if (stream != 0)
                {
                    BASS_TAG_GetFromFile(stream, tags);
                    Un4seen.Bass.Bass.BASS_StreamFree(stream);
                    return true;
                }
            }
            return false;
        }

        public static bool BASS_TAG_GetFromURL(int stream, TAG_INFO tags)
        {
            if ((stream == 0) || (tags == null))
            {
                return false;
            }
            bool flag = false;
            BASS_CHANNELINFO info = new BASS_CHANNELINFO();
            if (Un4seen.Bass.Bass.BASS_ChannelGetInfo(stream, info))
            {
                tags.channelinfo = info;
            }
            IntPtr data = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_ICY);
            if (data != IntPtr.Zero)
            {
                tags.tagType = BASSTag.BASS_TAG_ICY;
            }
            else
            {
                data = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_HTTP);
                if (data != IntPtr.Zero)
                {
                    tags.tagType = BASSTag.BASS_TAG_HTTP;
                }
            }
            if (data != IntPtr.Zero)
            {
                flag = tags.UpdateFromMETA(data, false, false);
            }
            data = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_META);
            if (data != IntPtr.Zero)
            {
                tags.tagType = BASSTag.BASS_TAG_META;
                flag = tags.UpdateFromMETA(data, false, true);
            }
            else
            {
                data = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_OGG);
                if (data == IntPtr.Zero)
                {
                    data = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_APE);
                    if (data != IntPtr.Zero)
                    {
                        tags.tagType = BASSTag.BASS_TAG_APE;
                    }
                }
                else
                {
                    tags.tagType = BASSTag.BASS_TAG_OGG;
                }
                if (data == IntPtr.Zero)
                {
                    data = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_WMA);
                    if (data != IntPtr.Zero)
                    {
                        tags.tagType = BASSTag.BASS_TAG_WMA;
                    }
                }
                if (data != IntPtr.Zero)
                {
                    flag = tags.UpdateFromMETA(data, true, false);
                }
            }
            tags.duration = Un4seen.Bass.Bass.BASS_ChannelBytes2Seconds(stream, Un4seen.Bass.Bass.BASS_ChannelGetLength(stream));
            return flag;
        }

        private static bool c(IntPtr A_0, TAG_INFO A_1)
        {
            if ((A_0 == IntPtr.Zero) || (A_1 == null))
            {
                return false;
            }
            bool flag = true;
            try
            {
                BASS_TAG_ID3 bass_tag_id = (BASS_TAG_ID3) Marshal.PtrToStructure(A_0, typeof(BASS_TAG_ID3));
                if (!bass_tag_id.ID.Equals("TAG"))
                {
                    return flag;
                }
                A_1.title = bass_tag_id.Title;
                if (EvalNativeTAGs)
                {
                    A_1.AddNativeTag("Title", bass_tag_id.Title);
                }
                A_1.artist = bass_tag_id.Artist;
                if (EvalNativeTAGs)
                {
                    A_1.AddNativeTag("Artist", bass_tag_id.Artist);
                }
                A_1.album = bass_tag_id.Album;
                if (EvalNativeTAGs)
                {
                    A_1.AddNativeTag("Album", bass_tag_id.Album);
                }
                A_1.year = bass_tag_id.Year;
                if (EvalNativeTAGs)
                {
                    A_1.AddNativeTag("Year", bass_tag_id.Year);
                }
                A_1.comment = bass_tag_id.Comment;
                if (EvalNativeTAGs)
                {
                    A_1.AddNativeTag("Comment", bass_tag_id.Comment);
                }
                if (bass_tag_id.g == 0)
                {
                    A_1.track = bass_tag_id.Track.ToString();
                    if (EvalNativeTAGs)
                    {
                        A_1.AddNativeTag("Track", bass_tag_id.Track.ToString());
                    }
                }
                if (EvalNativeTAGs)
                {
                    A_1.AddNativeTag("Genre", bass_tag_id.Genre);
                }
                try
                {
                    A_1.genre = ID3v1Genre[bass_tag_id.Genre];
                }
                catch
                {
                    A_1.genre = "Unknown";
                }
            }
            catch
            {
                flag = false;
            }
            return flag;
        }

        private static bool d(IntPtr A_0, TAG_INFO A_1)
        {
            if ((A_0 == IntPtr.Zero) || (A_1 == null))
            {
                return false;
            }
            bool flag = true;
            try
            {
                BASS_TAG_CART bass_tag_cart = (BASS_TAG_CART) Marshal.PtrToStructure(A_0, typeof(BASS_TAG_CART));
                if (EvalNativeTAGsCART)
                {
                    A_1.AddNativeTag("CARTVersion", bass_tag_cart.Version);
                }
                if (!string.IsNullOrEmpty(bass_tag_cart.Title) && string.IsNullOrEmpty(A_1.title))
                {
                    A_1.title = bass_tag_cart.Title;
                }
                if (EvalNativeTAGsCART)
                {
                    A_1.AddNativeTag("CARTTitle", bass_tag_cart.Title);
                }
                if (!string.IsNullOrEmpty(bass_tag_cart.Artist) && string.IsNullOrEmpty(A_1.artist))
                {
                    A_1.artist = bass_tag_cart.Artist;
                }
                if (EvalNativeTAGsCART)
                {
                    A_1.AddNativeTag("CARTArtist", bass_tag_cart.Artist);
                }
                if (!string.IsNullOrEmpty(bass_tag_cart.CutID) && string.IsNullOrEmpty(A_1.album))
                {
                    A_1.album = bass_tag_cart.CutID;
                }
                if (EvalNativeTAGsCART)
                {
                    A_1.AddNativeTag("CARTCutID", bass_tag_cart.CutID);
                }
                if (!string.IsNullOrEmpty(bass_tag_cart.ClientID) && string.IsNullOrEmpty(A_1.copyright))
                {
                    A_1.copyright = bass_tag_cart.ClientID;
                }
                if (EvalNativeTAGsCART)
                {
                    A_1.AddNativeTag("CARTClientID", bass_tag_cart.ClientID);
                }
                if (!string.IsNullOrEmpty(bass_tag_cart.Category) && string.IsNullOrEmpty(A_1.genre))
                {
                    A_1.genre = bass_tag_cart.Category;
                }
                if (EvalNativeTAGsCART)
                {
                    A_1.AddNativeTag("CARTCategory", bass_tag_cart.Category);
                }
                if (!string.IsNullOrEmpty(bass_tag_cart.Classification) && string.IsNullOrEmpty(A_1.grouping))
                {
                    A_1.grouping = bass_tag_cart.Classification;
                }
                if (EvalNativeTAGsCART)
                {
                    A_1.AddNativeTag("CARTClassification", bass_tag_cart.Classification);
                }
                if (!string.IsNullOrEmpty(bass_tag_cart.ProducerAppID) && string.IsNullOrEmpty(A_1.encodedby))
                {
                    A_1.encodedby = bass_tag_cart.ProducerAppID;
                }
                if (EvalNativeTAGsCART)
                {
                    A_1.AddNativeTag("CARTProducerAppID", bass_tag_cart.ProducerAppID);
                }
                string tagText = bass_tag_cart.GetTagText(A_0);
                if (!string.IsNullOrEmpty(tagText) && string.IsNullOrEmpty(A_1.comment))
                {
                    A_1.comment = tagText;
                }
                if (EvalNativeTAGsCART)
                {
                    A_1.AddNativeTag("CARTTagText", tagText);
                }
                if (EvalNativeTAGsCART)
                {
                    A_1.AddNativeTag("CARTOutCue", bass_tag_cart.OutCue);
                    A_1.AddNativeTag("CARTStartDate", bass_tag_cart.StartDate);
                    A_1.AddNativeTag("CARTStartTime", bass_tag_cart.StartTime);
                    A_1.AddNativeTag("CARTEndDate", bass_tag_cart.EndDate);
                    A_1.AddNativeTag("CARTEndTime", bass_tag_cart.EndTime);
                    A_1.AddNativeTag("CARTProducerAppVersion", bass_tag_cart.ProducerAppVersion);
                    A_1.AddNativeTag("CARTUserDef", bass_tag_cart.UserDef);
                    A_1.AddNativeTag("CARTLevelReference", bass_tag_cart.LevelReference.ToString(CultureInfo.InvariantCulture));
                    A_1.AddNativeTag("CARTTimer1Usage", bass_tag_cart.Timer1Usage);
                    A_1.AddNativeTag("CARTTimer1Value", ((uint) bass_tag_cart.Timer1Value).ToString(CultureInfo.InvariantCulture));
                    A_1.AddNativeTag("CARTTimer2Usage", bass_tag_cart.Timer2Usage);
                    A_1.AddNativeTag("CARTTimer2Value", ((uint) bass_tag_cart.Timer2Value).ToString(CultureInfo.InvariantCulture));
                    A_1.AddNativeTag("CARTTimer3Usage", bass_tag_cart.Timer3Usage);
                    A_1.AddNativeTag("CARTTimer3Value", ((uint) bass_tag_cart.Timer3Value).ToString(CultureInfo.InvariantCulture));
                    A_1.AddNativeTag("CARTTimer4Usage", bass_tag_cart.Timer4Usage);
                    A_1.AddNativeTag("CARTTimer4Value", ((uint) bass_tag_cart.Timer4Value).ToString(CultureInfo.InvariantCulture));
                    A_1.AddNativeTag("CARTTimer5Usage", bass_tag_cart.Timer5Usage);
                    A_1.AddNativeTag("CARTTimer5Value", ((uint) bass_tag_cart.Timer5Value).ToString(CultureInfo.InvariantCulture));
                    A_1.AddNativeTag("CARTTimer6Usage", bass_tag_cart.Timer6Usage);
                    A_1.AddNativeTag("CARTTimer6Value", ((uint) bass_tag_cart.Timer6Value).ToString(CultureInfo.InvariantCulture));
                    A_1.AddNativeTag("CARTTimer7Usage", bass_tag_cart.Timer7Usage);
                    A_1.AddNativeTag("CARTTimer7Value", ((uint) bass_tag_cart.Timer7Value).ToString(CultureInfo.InvariantCulture));
                    A_1.AddNativeTag("CARTTimer8Usage", bass_tag_cart.Timer8Usage);
                    A_1.AddNativeTag("CARTTimer8Value", ((uint) bass_tag_cart.Timer8Value).ToString(CultureInfo.InvariantCulture));
                    A_1.AddNativeTag("CARTURL", bass_tag_cart.URL);
                }
            }
            catch
            {
                flag = false;
            }
            return flag;
        }

        private static bool e(IntPtr A_0, TAG_INFO A_1)
        {
            if ((A_0 == IntPtr.Zero) || (A_1 == null))
            {
                return false;
            }
            bool flag = true;
            try
            {
                BASS_TAG_BEXT bass_tag_bext = (BASS_TAG_BEXT) Marshal.PtrToStructure(A_0, typeof(BASS_TAG_BEXT));
                if (!string.IsNullOrEmpty(bass_tag_bext.Description) && string.IsNullOrEmpty(A_1.title))
                {
                    A_1.title = bass_tag_bext.Description;
                }
                if (EvalNativeTAGsBEXT)
                {
                    A_1.AddNativeTag("BWFDescription", bass_tag_bext.Description);
                }
                if (!string.IsNullOrEmpty(bass_tag_bext.Originator) && string.IsNullOrEmpty(A_1.artist))
                {
                    A_1.artist = bass_tag_bext.Originator;
                }
                if (EvalNativeTAGsBEXT)
                {
                    A_1.AddNativeTag("BWFOriginator", bass_tag_bext.Originator);
                }
                if (!string.IsNullOrEmpty(bass_tag_bext.OriginatorReference) && string.IsNullOrEmpty(A_1.encodedby))
                {
                    A_1.encodedby = bass_tag_bext.OriginatorReference;
                }
                if (EvalNativeTAGsBEXT)
                {
                    A_1.AddNativeTag("BWFOriginatorReference", bass_tag_bext.OriginatorReference);
                }
                if (!string.IsNullOrEmpty(bass_tag_bext.OriginationDate) && string.IsNullOrEmpty(A_1.year))
                {
                    A_1.year = bass_tag_bext.OriginationDate;
                }
                if (EvalNativeTAGsBEXT)
                {
                    A_1.AddNativeTag("BWFOriginationDate", bass_tag_bext.OriginationDate);
                    A_1.AddNativeTag("BWFOriginationTime", bass_tag_bext.OriginationTime);
                    A_1.AddNativeTag("BWFTimeReference", bass_tag_bext.TimeReference.ToString());
                    A_1.AddNativeTag("BWFVersion", bass_tag_bext.Version.ToString());
                    A_1.AddNativeTag("BWFUMID", bass_tag_bext.UMID);
                }
                string codingHistory = bass_tag_bext.GetCodingHistory(A_0);
                if (!string.IsNullOrEmpty(codingHistory) && string.IsNullOrEmpty(A_1.comment))
                {
                    A_1.comment = codingHistory;
                }
                if (EvalNativeTAGsBEXT)
                {
                    A_1.AddNativeTag("BWFCodingHistory", codingHistory);
                }
            }
            catch
            {
                flag = false;
            }
            return flag;
        }

        private static void f(int A_0, TAG_INFO A_1)
        {
            TagPicture[] pictureArray = Un4seen.Bass.Bass.BASS_ChannelGetTagsAPEPictures(A_0);
            if ((pictureArray != null) && (pictureArray.Length > 0))
            {
                foreach (TagPicture picture in pictureArray)
                {
                    A_1.AddPicture(picture);
                }
            }
        }
    }
}

