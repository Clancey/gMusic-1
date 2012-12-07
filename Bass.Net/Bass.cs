using System;
//using System.Runtime.InteropServices;
using System.Runtime.InteropServices;

namespace Un4seen.Bass
{
	public delegate void DOWNLOADPROC (IntPtr buffer, int length, IntPtr user);
	
	public delegate void NotifyProc (BassIosNotify status);

	public class Bass
	{
		static object a = new object ();

		public static bool BASS_Init (int device, int freq, BASSInit flags, IntPtr win)
		{
			return  BASS_Init (device, freq, flags, win, IntPtr.Zero);
		}

		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern bool BASS_ChannelPlay (int handle, [MarshalAs(UnmanagedType.Bool)] bool restart);

		public static bool BASS_Init (int device, int freq, BASSInit flags, IntPtr win, Guid clsid)
		{
			//return true;
			if (clsid == Guid.Empty) {
				return BASS_Init (device, freq, flags, win, IntPtr.Zero);
			}
			return BASS_InitGuid (device, freq, flags, win, clsid);
		}

		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern BASSError BASS_ErrorGetCode ();

		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern int BASS_ChannelSetFX (int handle, BASSFXType type, int priority);

		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		private static extern bool BASS_Init (int A_0, int A_1, BASSInit A_2, IntPtr A_3, IntPtr A_4);

		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		private static extern bool BASS_InitGuid (int A_0, int A_1, BASSInit A_2, IntPtr A_3, [MarshalAs(UnmanagedType.LPStruct)] Guid A_4);

		public static int BASS_StreamCreateFile(IntPtr memory, long offset, long length, BASSFlag flags)
		{
			return BASS_StreamCreateFileMemory(true, memory, offset, length, flags);
		}

		public static int BASS_StreamCreateFile(string file, long offset, long length, BASSFlag flags)
		{
			flags |= BASSFlag.BASS_DEFAULT | BASSFlag.BASS_UNICODE;
			return BASS_StreamCreateFileUnicode(false, file, offset, length, flags);
		}
		#if iOS
		[DllImport("__Internal",
		#else
		[DllImport("libbass",
		#endif
			EntryPoint="BASS_StreamCreateFile")]
		private static extern int BASS_StreamCreateFileMemory([MarshalAs(UnmanagedType.Bool)] bool A_0, IntPtr A_1, long A_2, long A_3, BASSFlag A_4);
		           #if iOS
		           [DllImport("__Internal",
			#else
		           [DllImport("libbass",
			#endif
		           EntryPoint="BASS_StreamCreateFile")]
		private static extern int BASS_StreamCreateFileUnicode([MarshalAs(UnmanagedType.Bool)] bool A_0, [In, MarshalAs(UnmanagedType.LPWStr)] string A_1, long A_2, long A_3, BASSFlag A_4);
			#if iOS
		           [DllImport("__Internal")]
			#else
		           [DllImport("libbass")]
			#endif
		public static extern int BASS_StreamCreateFileUser(BASSStreamSystem system, BASSFlag flags, BASS_FILEPROCS procs, IntPtr user);
			#if iOS
		           [DllImport("__Internal",
			#else
		           [DllImport("libbass",
			#endif
		           EntryPoint="BASS_StreamCreate")]
		private static extern int BASS_StreamCreatePtr(int A_0, int A_1, BASSFlag A_2, IntPtr A_3, IntPtr A_4);
		public static int BASS_StreamCreatePush(int freq, int chans, BASSFlag flags, IntPtr user)
		{
			return BASS_StreamCreatePtr(freq, chans, flags, new IntPtr(-1), user);
		}


		public static int BASS_StreamCreateURL (string url, int offset, BASSFlag flags, DOWNLOADPROC proc, IntPtr user)
		{
			//return 0;
			flags |= BASSFlag.BASS_DEFAULT | BASSFlag.BASS_UNICODE;
			return BASS_StreamCreateURLUnicode (url, offset, flags, proc, user);
		}

		#if iOS
		[DllImport("__Internal",
		#else
		[DllImport("libbass",
		#endif
			EntryPoint="BASS_StreamCreateURL")]
		private static extern int BASS_StreamCreateURLUnicode ([In, MarshalAs(UnmanagedType.LPWStr)] string A_0, int A_1, BASSFlag A_2, DOWNLOADPROC A_3, IntPtr A_4);

		[return: MarshalAs(UnmanagedType.Bool)]
			#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern bool BASS_StreamFree(int handle);
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern long BASS_StreamGetFilePosition(int handle, BASSStreamFilePosition mode);

		public static bool BASS_FXGetParameters (int handle, BASS_DX8_PARAMEQ par)
		{
			lock (a) {
				return BASS_FXGetParametersExt (handle, par);
			}
		}

		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal",
		#else
		[DllImport("libbass",
		#endif
			EntryPoint="BASS_FXGetParameters")]
		private static extern bool BASS_FXGetParametersExt (int A_0, BASS_DX8_PARAMEQ A_1);
		
		public static bool BASS_FXSetParameters(int handle, BASS_DX8_PARAMEQ par)
		{
			lock (a)
			{
				return BASS_FXSetParametersExt(handle, par);
			}
		}

		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal",
		#else
		[DllImport("libbass",
		#endif
			EntryPoint="BASS_FXSetParameters")]
		private static extern bool BASS_FXSetParametersExt (int A_0, BASS_DX8_PARAMEQ A_1);


		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern int BASS_SampleGetChannel(int handle, [MarshalAs(UnmanagedType.Bool)] bool onlynew);
		public static int BASS_SampleGetChannelCount(int handle)
		{
			return BASS_SampleGetChannels(handle, null);
		}

		public static int[] BASS_SampleGetChannels(int handle)
		{
			if (handle == 0)
				return null;
			var sampleInfo = BASS_SampleGetInfo (handle);
			if (sampleInfo == null) {
				var err = BASS_ErrorGetCode();
				return null;
			}
			int[] channels = new int[sampleInfo.max];
			int length = BASS_SampleGetChannels(handle, channels);
			if (length >= 0)
			{
				int[] destinationArray = new int[length];
				Array.Copy(channels, destinationArray, length);
				return destinationArray;
			}
			return null;
		}

		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern void BASS_Apply3D();
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern double BASS_ChannelBytes2Seconds(int handle, long pos);
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern BASSFlag BASS_ChannelFlags(int handle, BASSFlag flags, BASSFlag mask);
		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern bool BASS_ChannelGet3DAttributes(int handle, ref BASS3DMode mode, ref float min, ref float max, ref int iangle, ref int oangle, ref int outvol);
		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern bool BASS_ChannelGet3DAttributes(int handle, [In, Out, MarshalAs(UnmanagedType.AsAny)] object mode, [In, Out, MarshalAs(UnmanagedType.AsAny)] object min, [In, Out, MarshalAs(UnmanagedType.AsAny)] object max, [In, Out, MarshalAs(UnmanagedType.AsAny)] object iangle, [In, Out, MarshalAs(UnmanagedType.AsAny)] object oangle, [In, Out, MarshalAs(UnmanagedType.AsAny)] object outvol);
		//[return: MarshalAs(UnmanagedType.Bool)]
		//[DllImport("__Internal")]
		//public static extern bool BASS_ChannelGet3DPosition(int handle, [In, Out] BASS_3DVECTOR pos, [In, Out] BASS_3DVECTOR orient, [In, Out] BASS_3DVECTOR vel);
		//[return: MarshalAs(UnmanagedType.Bool)]
		//[DllImport("__Internal")]
		//public static extern bool BASS_ChannelGetAttribute(int handle, BASSAttribute attrib, ref float value);
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern int BASS_ChannelGetData(int handle, IntPtr buffer, int length);
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern int BASS_ChannelGetData(int handle, [In, Out] byte[] buffer, int length);
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern int BASS_ChannelGetData(int handle, [In, Out] short[] buffer, int length);
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern int BASS_ChannelGetData(int handle, [In, Out] int[] buffer, int length);
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern int BASS_ChannelGetData(int handle, [In, Out] float[] buffer, int length);
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern int BASS_ChannelGetDevice(int handle);
		public static BASS_CHANNELINFO BASS_ChannelGetInfo(int handle)
		{
			BASS_CHANNELINFO info = new BASS_CHANNELINFO();
			if (BASS_ChannelGetInfo(handle, info))
			{
				return info;
			}
			return null;
		}

		public static bool BASS_ChannelGetInfo(int handle, BASS_CHANNELINFO info)
		{
			bool flag = BASS_ChannelGetInfoInternal(handle, ref info.a);
			if (flag)
			{
				info.chans = info.a.b;
				info.ctype = info.a.d;
				info.flags = info.a.flag;
				info.freq = info.a.a;
				info.origres = info.a.e;
				info.plugin = info.a.f;
				info.sample = info.a.g;
				info.filename = Marshal.PtrToStringUni(info.a.h);
			}
			return flag;
		}

		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal",
		#else
		[DllImport("libbass",
		#endif
			EntryPoint="BASS_ChannelGetInfo")]
		private static extern bool BASS_ChannelGetInfoInternal(int A_0, [In, Out] ref Un4seen.Bass.c A_1);
		public static long BASS_ChannelGetLength(int handle)
		{
			return BASS_ChannelGetLength(handle, BASSMode.BASS_POS_BYTES);
		}

		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern long BASS_ChannelGetLength(int handle, BASSMode mode);
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern int BASS_ChannelGetLevel(int handle);
		public static bool BASS_ChannelGetLevel(int handle, float[] level)
		{
			if (level.Length > 0)
			{
				Array.Clear(level, 0, level.Length);
			}
			else
			{
				return false;
			}
			int num = (int) BASS_ChannelSeconds2Bytes(handle, 0.02);
			if (num <= 0)
			{
				return false;
			}
			float[] buffer = new float[num / 4];
			num = BASS_ChannelGetData(handle, buffer, num | 0x40000000) / 4;
			int index = 0;
			for (int i = 0; i < num; i++)
			{
				float num3 = Math.Abs(buffer[i]);
				if (num3 > level[index])
				{
					level[index] = num3;
				}
				index++;
				if (index >= level.Length)
				{
					index = 0;
				}
			}
			return true;
		}

//		public static string[] BASS_ChannelGetMidiTrackText(int handle, int track)
//		{
//			if (track >= 0)
//			{
//				return Utils.IntPtrToArrayNullTermAnsi(BASS_ChannelGetTags(handle, (BASSTag) (0x11000 + track)));
//			}
//			List<string> list = new List<string>();
//			track = 0;
//			while (true)
//			{
//				IntPtr pointer = BASS_ChannelGetTags(handle, (BASSTag) (0x11000 + track));
//				if (!(pointer != IntPtr.Zero))
//				{
//					break;
//				}
//				string[] collection = Utils.IntPtrToArrayNullTermAnsi(pointer);
//				if ((collection != null) && (collection.Length > 0))
//				{
//					list.AddRange(collection);
//				}
//				track++;
//			}
//			if (list.Count > 0)
//			{
//				return list.ToArray();
//			}
//			return null;
//		}
//
//		public static string BASS_ChannelGetMusicInstrument(int handle, int instrument)
//		{
//			IntPtr ptr = BASS_ChannelGetTags(handle, (BASSTag) (0x10100 + instrument));
//			if (ptr != IntPtr.Zero)
//			{
//				return Marshal.PtrToStringUni(ptr);
//			}
//			return null;
//		}
//
//		public static string BASS_ChannelGetMusicMessage(int handle)
//		{
//			IntPtr ptr = BASS_ChannelGetTags(handle, BASSTag.BASS_TAG_MUSIC_MESSAGE);
//			if (ptr != IntPtr.Zero)
//			{
//				return Marshal.PtrToStringUni(ptr);
//			}
//			return null;
//		}
//
//		public static string BASS_ChannelGetMusicName(int handle)
//		{
//			IntPtr ptr = BASS_ChannelGetTags(handle, BASSTag.BASS_TAG_MUSIC_NAME);
//			if (ptr != IntPtr.Zero)
//			{
//				return Marshal.PtrToStringUni(ptr);
//			}
//			return null;
//		}
//
//		public static string BASS_ChannelGetMusicSample(int handle, int sample)
//		{
//			IntPtr ptr = BASS_ChannelGetTags(handle, (BASSTag) (0x10300 + sample));
//			if (ptr != IntPtr.Zero)
//			{
//				return Marshal.PtrToStringUni(ptr);
//			}
//			return null;
//		}

		public static long BASS_ChannelGetPosition(int handle)
		{
			return BASS_ChannelGetPosition(handle, BASSMode.BASS_POS_BYTES);
		}

		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern long BASS_ChannelGetPosition(int handle, BASSMode mode);
//		public static string BASS_ChannelGetTagLyrics3v2(int handle)
//		{
//			IntPtr ptr = BASS_ChannelGetTags(handle, BASSTag.BASS_TAG_LYRICS3);
//			if (ptr != IntPtr.Zero)
//			{
//				return Marshal.PtrToStringUni(ptr);
//			}
//			return null;
//		}

//		[DllImport("__Internal")]
//		public static extern IntPtr BASS_ChannelGetTags(int handle, BASSTag tags);
//		public static string[] BASS_ChannelGetTagsAPE(int handle)
//		{
//			return BASS_ChannelGetTagsArrayNullTermUtf8(handle, BASSTag.BASS_TAG_APE);
//		}

		//        public static BASS_TAG_APE_BINARY[] BASS_ChannelGetTagsAPEBinary(int handle)
		//        {
		//            BASS_TAG_APE_BINARY bass_tag_ape_binary;
		//            List<BASS_TAG_APE_BINARY> list = new List<BASS_TAG_APE_BINARY>();
		//            for (int i = 0; (bass_tag_ape_binary = BASS_TAG_APE_BINARY.GetTag(handle, i)) != null; i++)
		//            {
		//                list.Add(bass_tag_ape_binary);
		//            }
		//            if (list.Count > 0)
		//            {
		//                return list.ToArray();
		//            }
		//            return null;
		//        }

		//        public static TagPicture[] BASS_ChannelGetTagsAPEPictures(int handle)
		//        {
		//            BASS_TAG_APE_BINARY bass_tag_ape_binary;
		//            List<TagPicture> list = new List<TagPicture>();
		//            for (int i = 0; (bass_tag_ape_binary = BASS_TAG_APE_BINARY.GetTag(handle, i)) != null; i++)
		//            {
		//                if ((bass_tag_ape_binary.Key != null) && bass_tag_ape_binary.Key.ToLower().StartsWith("cover art"))
		//                {
		//                    list.Add(new TagPicture(bass_tag_ape_binary.Data, 2));
		//                }
		//            }
		//            if (list.Count > 0)
		//            {
		//                return list.ToArray();
		//            }
		//            return null;
		//        }

//		public static string[] BASS_ChannelGetTagsArrayNullTermAnsi(int handle, BASSTag format)
//		{
//			return Utils.IntPtrToArrayNullTermAnsi(BASS_ChannelGetTags(handle, format));
//		}
//
//		public static string[] BASS_ChannelGetTagsArrayNullTermUtf8(int handle, BASSTag format)
//		{
//			return Utils.IntPtrToArrayNullTermUtf8(BASS_ChannelGetTags(handle, format));
//		}

		//        public static string[] BASS_ChannelGetTagsBWF(int handle)
		//        {
		//            IntPtr ptr = BASS_ChannelGetTags(handle, BASSTag.BASS_TAG_RIFF_BEXT);
		//            if (!(ptr != IntPtr.Zero))
		//            {
		//                return null;
		//            }
		//            string[] strArray = new string[9];
		//            try
		//            {
		//                BASS_TAG_BEXT bass_tag_bext = (BASS_TAG_BEXT) Marshal.PtrToStructure(ptr, typeof(BASS_TAG_BEXT));
		//                strArray[0] = bass_tag_bext.Description;
		//                strArray[1] = bass_tag_bext.Originator;
		//                strArray[2] = bass_tag_bext.OriginatorReference;
		//                strArray[3] = bass_tag_bext.OriginationDate;
		//                strArray[4] = bass_tag_bext.OriginationTime;
		//                strArray[5] = bass_tag_bext.TimeReference.ToString();
		//                strArray[6] = bass_tag_bext.Version.ToString();
		//                strArray[7] = bass_tag_bext.UMID;
		//                strArray[8] = bass_tag_bext.GetCodingHistory(ptr);
		//            }
		//            catch
		//            {
		//            }
		//            return strArray;
		//        }
		//
		//        public static BASS_TAG_FLAC_CUE BASS_ChannelGetTagsFLACCuesheet(int handle)
		//        {
		//            return BASS_TAG_FLAC_CUE.GetTag(handle);
		//        }
		//
		//        public static BASS_TAG_FLAC_PICTURE[] BASS_ChannelGetTagsFLACPictures(int handle)
		//        {
		//            BASS_TAG_FLAC_PICTURE bass_tag_flac_picture;
		//            List<BASS_TAG_FLAC_PICTURE> list = new List<BASS_TAG_FLAC_PICTURE>();
		//            for (int i = 0; (bass_tag_flac_picture = BASS_TAG_FLAC_PICTURE.GetTag(handle, i)) != null; i++)
		//            {
		//                list.Add(bass_tag_flac_picture);
		//            }
		//            if (list.Count > 0)
		//            {
		//                return list.ToArray();
		//            }
		//            return null;
		//        }

//		public static string[] BASS_ChannelGetTagsHTTP(int handle)
//		{
//			return BASS_ChannelGetTagsArrayNullTermAnsi(handle, BASSTag.BASS_TAG_HTTP);
//		}
//
//		public static string[] BASS_ChannelGetTagsICY(int handle)
//		{
//			return BASS_ChannelGetTagsArrayNullTermAnsi(handle, BASSTag.BASS_TAG_ICY);
//		}

		//        public static string[] BASS_ChannelGetTagsID3V1(int handle)
		//        {
		//            IntPtr ptr = BASS_ChannelGetTags(handle, BASSTag.BASS_TAG_ID3);
		//            if (!(ptr != IntPtr.Zero))
		//            {
		//                return null;
		//            }
		//            string[] strArray = new string[7];
		//            try
		//            {
		//                BASS_TAG_ID3 bass_tag_id = (BASS_TAG_ID3) Marshal.PtrToStructure(ptr, typeof(BASS_TAG_ID3));
		//                strArray[0] = bass_tag_id.Title;
		//                strArray[1] = bass_tag_id.Artist;
		//                strArray[2] = bass_tag_id.Album;
		//                strArray[3] = bass_tag_id.Year;
		//                strArray[4] = bass_tag_id.Comment;
		//                strArray[5] = bass_tag_id.Genre.ToString();
		//                if (bass_tag_id.g == 0)
		//                {
		//                    strArray[6] = bass_tag_id.Track.ToString();
		//                }
		//            }
		//            catch
		//            {
		//            }
		//            return strArray;
		//        }

		//        public static string[] BASS_ChannelGetTagsID3V2(int handle)
		//        {
		//            IntPtr ptr = BASS_ChannelGetTags(handle, BASSTag.BASS_TAG_ID3V2);
		//            if (ptr != IntPtr.Zero)
		//            {
		//                try
		//                {
		//                    List<string> list = new List<string>();
		//                    Un4seen.Bass.AddOn.Tags.c c = new Un4seen.Bass.AddOn.Tags.c(ptr);
		//                    int num = 0;
		//                    while (c.q())
		//                    {
		//                        string str = c.s();
		//                        object obj2 = c.p();
		//                        if ((str.Length > 0) && (obj2 is string))
		//                        {
		//                            list.Add(string.Format("{0}={1}", str, obj2));
		//                        }
		//                        else if (((str == "POPM") || (str == "POP")) && (obj2 is byte))
		//                        {
		//                            if (num == 0)
		//                            {
		//                                list.Add(string.Format("POPM={0}", obj2));
		//                            }
		//                            num++;
		//                        }
		//                    }
		//                    c.r();
		//                    if (list.Count > 0)
		//                    {
		//                        return list.ToArray();
		//                    }
		//                    return null;
		//                }
		//                catch
		//                {
		//                    return null;
		//                }
		//            }
		//            return null;
		//        }

//		public static string[] BASS_ChannelGetTagsMETA(int handle)
//		{
//			return Utils.IntPtrToArrayNullTermAnsi(BASS_ChannelGetTags(handle, BASSTag.BASS_TAG_META));
//		}
//
//		public static string[] BASS_ChannelGetTagsMF(int handle)
//		{
//			return BASS_ChannelGetTagsArrayNullTermUtf8(handle, BASSTag.BASS_TAG_MF);
//		}
//
//		public static string[] BASS_ChannelGetTagsMP4(int handle)
//		{
//			return BASS_ChannelGetTagsArrayNullTermUtf8(handle, BASSTag.BASS_TAG_MP4);
//		}
//
//		public static string[] BASS_ChannelGetTagsOGG(int handle)
//		{
//			return BASS_ChannelGetTagsArrayNullTermUtf8(handle, BASSTag.BASS_TAG_OGG);
//		}
//
//		public static string[] BASS_ChannelGetTagsRIFF(int handle)
//		{
//			return BASS_ChannelGetTagsArrayNullTermAnsi(handle, BASSTag.BASS_TAG_RIFF_INFO);
//		}
//
//		public static string[] BASS_ChannelGetTagsWMA(int handle)
//		{
//			return BASS_ChannelGetTagsArrayNullTermUtf8(handle, BASSTag.BASS_TAG_WMA);
//		}

//		[DllImport("__Internal")]
//		public static extern BASSActive BASS_ChannelIsActive(int handle);
//		[return: MarshalAs(UnmanagedType.Bool)]
//		[DllImport("__Internal")]
//		public static extern bool BASS_ChannelIsSliding(int handle, BASSAttribute attrib);
		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern bool BASS_ChannelLock(int handle, [MarshalAs(UnmanagedType.Bool)] bool state);
		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern bool BASS_ChannelPause(int handle);
		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern bool BASS_ChannelRemoveDSP(int handle, int dsp);
		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern bool BASS_ChannelRemoveFX(int handle, int fx);
		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern bool BASS_ChannelRemoveLink(int handle, int chan);
		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern bool BASS_ChannelRemoveSync(int handle, int sync);
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern long BASS_ChannelSeconds2Bytes(int handle, double pos);
		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern bool BASS_ChannelSet3DAttributes(int handle, BASS3DMode mode, float min, float max, int iangle, int oangle, int outvol);
//		[return: MarshalAs(UnmanagedType.Bool)]
//		[DllImport("__Internal")]
//		public static extern bool BASS_ChannelSet3DPosition(int handle, [In] BASS_3DVECTOR pos, [In] BASS_3DVECTOR orient, [In] BASS_3DVECTOR vel);
//		[return: MarshalAs(UnmanagedType.Bool)]
//		[DllImport("__Internal")]
//		public static extern bool BASS_ChannelSetAttribute(int handle, BASSAttribute attrib, float value);
		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern bool BASS_ChannelSetDevice(int handle, int device);
//		[DllImport("__Internal")]
//		public static extern int BASS_ChannelSetDSP(int handle, DSPPROC proc, IntPtr user, int priority);
		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern bool BASS_ChannelSetLink(int handle, int chan);
		public static bool BASS_ChannelSetPosition(int handle, double seconds)
		{
			return BASS_ChannelSetPosition(handle, BASS_ChannelSeconds2Bytes(handle, seconds), BASSMode.BASS_POS_BYTES);
		}

		public static bool BASS_ChannelSetPosition(int handle, long pos)
		{
			return BASS_ChannelSetPosition(handle, pos, BASSMode.BASS_POS_BYTES);
		}

//		public static bool BASS_ChannelSetPosition(int handle, int order, int row)
//		{
//			return BASS_ChannelSetPosition(handle, (long) Utils.MakeLong(order, row), BASSMode.BASS_POS_MUSIC_ORDERS);
//		}

		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern bool BASS_ChannelSetPosition(int handle, long pos, BASSMode mode);
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern int BASS_ChannelSetSync(int handle, BASSSync type, long param, SYNCPROC proc, IntPtr user);
		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern bool BASS_ChannelSlideAttribute(int handle, BASSAttribute attrib, float value, int time);
		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern bool BASS_ChannelStop(int handle);
		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern bool BASS_ChannelUpdate(int handle, int length);
		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern bool BASS_Free();

		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern BASSActive BASS_ChannelIsActive(int handle);

		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern int BASS_SampleGetChannels(int handle, int[] channels); 
		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern bool BASS_SampleGetData(int handle, IntPtr buffer);
		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern bool BASS_SampleGetData(int handle, byte[] buffer);
		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern bool BASS_SampleGetData(int handle, short[] buffer);
		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern bool BASS_SampleGetData(int handle, int[] buffer);
		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern bool BASS_SampleGetData(int handle, float[] buffer);
		public static BASS_SAMPLE BASS_SampleGetInfo(int handle)
		{
			BASS_SAMPLE info = new BASS_SAMPLE();
			if (BASS_SampleGetInfo(handle, info))
			{
				return info;
			}
			return null;
		}

		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern bool BASS_SampleGetInfo(int handle, [In, Out] BASS_SAMPLE info);
		public static int BASS_SampleLoad(IntPtr memory, long offset, int length, int max, BASSFlag flags)
		{
			return BASS_SampleLoadMemory(true, memory, offset, length, max, flags);
		}

		public static int BASS_SampleLoad(string file, long offset, int length, int max, BASSFlag flags)
		{
			flags |= BASSFlag.BASS_DEFAULT | BASSFlag.BASS_UNICODE;
			return BASS_SampleLoadUnicode(false, file, offset, length, max, flags);
		}

		public static int BASS_SampleLoad(byte[] memory, long offset, int length, int max, BASSFlag flags)
		{
			return BASS_SampleLoadMemory(true, memory, offset, length, max, flags);
		}

		#if iOS
		[DllImport("__Internal",
		#else
		[DllImport("libbass",
		#endif
			EntryPoint="BASS_SampleLoad")]
		private static extern int BASS_SampleLoadMemory([MarshalAs(UnmanagedType.Bool)] bool A_0, IntPtr A_1, long A_2, int A_3, int A_4, BASSFlag A_5);
		#if iOS
		[DllImport("__Internal",
		#else
		[DllImport("libbass",
		#endif
			EntryPoint="BASS_SampleLoad")]
		private static extern int BASS_SampleLoadMemory([MarshalAs(UnmanagedType.Bool)] bool A_0, byte[] A_1, long A_2, int A_3, int A_4, BASSFlag A_5);
		#if iOS
		[DllImport("__Internal",
		#else
		[DllImport("libbass",
		#endif
			EntryPoint="BASS_SampleLoad")]
		private static extern int BASS_SampleLoadUnicode([MarshalAs(UnmanagedType.Bool)] bool A_0, [In, MarshalAs(UnmanagedType.LPWStr)] string A_1, long A_2, int A_3, int A_4, BASSFlag A_5);


		
		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern bool BASS_Start();
		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern bool BASS_SetConfig(BASSConfig option, [In, MarshalAs(UnmanagedType.Bool)] bool newvalue);
		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern bool BASS_SetConfig(BASSConfig option, int newvalue);
		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal")]
		#else
		[DllImport("libbass")]
		#endif
		public static extern bool BASS_SetConfigPtr(BASSConfig option, IntPtr newvalue);

		
		[return: MarshalAs(UnmanagedType.Bool)]
		#if iOS
		[DllImport("__Internal",
		#else
		[DllImport("libbass",
		#endif
			EntryPoint="BASS_SetConfigPtr")]
		private static extern bool BASS_SetInturuption(BASSConfig option, NotifyProc newvalue);

		public static bool SetInturuption(NotifyProc notifyProc)
		{
			return BASS_SetInturuption (BASSConfig.IOS_NOTIFY, notifyProc);
		}

	}

	[Serializable, StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
	public sealed class BASS_DX8_PARAMEQ
	{
		public float fCenter;
		public float fBandwidth;
		public float fGain;

		public BASS_DX8_PARAMEQ ()
		{
			this.fCenter = 100f;
			this.fBandwidth = 18f;
		}


		public BASS_DX8_PARAMEQ (float Center, float Bandwidth, float Gain)
		{
			this.fCenter = Center;
			this.fBandwidth = Bandwidth;
			this.fGain = Gain;
		}

		public void Preset_Default ()
		{
			this.fCenter = 100f;
			this.fBandwidth = 18f;
			this.fGain = 0f;
		}

		public void Preset_Low ()
		{
			this.fCenter = 125f;
			this.fBandwidth = 18f;
			this.fGain = 0f;
		}

		public void Preset_Mid ()
		{
			this.fCenter = 1000f;
			this.fBandwidth = 18f;
			this.fGain = 0f;
		}

		public void Preset_High ()
		{
			this.fCenter = 8000f;
			this.fBandwidth = 18f;
			this.fGain = 0f;
		}
	}

	[Flags]
	public enum BASSInit
	{
		BASS_DEVICE_3D = 4,
		BASS_DEVICE_8BITS = 1,
		BASS_DEVICE_CPSPEAKERS = 0x400,
		BASS_DEVICE_DEFAULT = 0,
		BASS_DEVICE_FREQ = 0x4000,
		BASS_DEVICE_LATENCY = 0x100,
		BASS_DEVICE_MONO = 2,
		BASS_DEVICE_NOSPEAKER = 0x1000,
		BASS_DEVICE_SPEAKERS = 0x800,
		BASS_DEVIDE_DMIX = 0x2000
	}

	public enum BASSFXType
	{
		BASS_FX_BFX_APF = 0x1000e,
		BASS_FX_BFX_AUTOWAH = 0x10009,
		BASS_FX_BFX_BQF = 0x10013,
		BASS_FX_BFX_CHORUS = 0x1000d,
		BASS_FX_BFX_COMPRESSOR = 0x1000f,
		BASS_FX_BFX_COMPRESSOR2 = 0x10011,
		BASS_FX_BFX_DAMP = 0x10008,
		BASS_FX_BFX_DISTORTION = 0x10010,
		BASS_FX_BFX_ECHO = 0x10001,
		BASS_FX_BFX_ECHO2 = 0x1000a,
		BASS_FX_BFX_ECHO3 = 0x1000c,
		BASS_FX_BFX_FLANGER = 0x10002,
		BASS_FX_BFX_LPF = 0x10006,
		BASS_FX_BFX_MIX = 0x10007,
		BASS_FX_BFX_PEAKEQ = 0x10004,
		BASS_FX_BFX_PHASER = 0x1000b,
		BASS_FX_BFX_REVERB = 0x10005,
		BASS_FX_BFX_ROTATE = 0x10000,
		BASS_FX_BFX_VOLUME = 0x10003,
		BASS_FX_BFX_VOLUME_ENV = 0x10012,
		BASS_FX_DX8_CHORUS = 0,
		BASS_FX_DX8_COMPRESSOR = 1,
		BASS_FX_DX8_DISTORTION = 2,
		BASS_FX_DX8_ECHO = 3,
		BASS_FX_DX8_FLANGER = 4,
		BASS_FX_DX8_GARGLE = 5,
		BASS_FX_DX8_I3DL2REVERB = 6,
		BASS_FX_DX8_PARAMEQ = 7,
		BASS_FX_DX8_REVERB = 8
	}

	public enum BASSError
	{
		BASS_ERROR_ACM_CANCEL = 0x7d0,
		BASS_ERROR_ALREADY = 14,
		BASS_ERROR_BUFLOST = 4,
		BASS_ERROR_BUSY = 0x2e,
		BASS_ERROR_CAST_DENIED = 0x834,
		BASS_ERROR_CDTRACK = 13,
		BASS_ERROR_CODEC = 0x2c,
		BASS_ERROR_CREATE = 0x21,
		BASS_ERROR_DECODE = 0x26,
		BASS_ERROR_DEVICE = 0x17,
		BASS_ERROR_DRIVER = 3,
		BASS_ERROR_DX = 0x27,
		BASS_ERROR_EMPTY = 0x1f,
		BASS_ERROR_ENDED = 0x2d,
		BASS_ERROR_FILEFORM = 0x29,
		BASS_ERROR_FILEOPEN = 2,
		BASS_ERROR_FORMAT = 6,
		BASS_ERROR_FREQ = 0x19,
		BASS_ERROR_HANDLE = 5,
		BASS_ERROR_ILLPARAM = 20,
		BASS_ERROR_ILLTYPE = 0x13,
		BASS_ERROR_INIT = 8,
		BASS_ERROR_MEM = 1,
		BASS_ERROR_NO3D = 0x15,
		BASS_ERROR_NOCD = 12,
		BASS_ERROR_NOCHAN = 0x12,
		BASS_ERROR_NOEAX = 0x16,
		BASS_ERROR_NOFX = 0x22,
		BASS_ERROR_NOHW = 0x1d,
		BASS_ERROR_NONET = 0x20,
		BASS_ERROR_NOPAUSE = 0x10,
		BASS_ERROR_NOPLAY = 0x18,
		BASS_ERROR_NOTAUDIO = 0x11,
		BASS_ERROR_NOTAVAIL = 0x25,
		BASS_ERROR_NOTFILE = 0x1b,
		BASS_ERROR_PLAYING = 0x23,
		BASS_ERROR_POSITION = 7,
		BASS_ERROR_SPEAKER = 0x2a,
		BASS_ERROR_START = 9,
		BASS_ERROR_TIMEOUT = 40,
		BASS_ERROR_UNKNOWN = -1,
		BASS_ERROR_VERSION = 0x2b,
		BASS_ERROR_VIDEO_ABORT = 0x2f,
		BASS_ERROR_VIDEO_CANNOT_CONNECT = 0x30,
		BASS_ERROR_VIDEO_CANNOT_READ = 0x31,
		BASS_ERROR_VIDEO_CANNOT_WRITE = 50,
		BASS_ERROR_VIDEO_FAILURE = 0x33,
		BASS_ERROR_VIDEO_FILTER = 0x34,
		BASS_ERROR_VIDEO_INVALID_CHAN = 0x35,
		BASS_ERROR_VIDEO_INVALID_DLL = 0x36,
		BASS_ERROR_VIDEO_INVALID_FORMAT = 0x37,
		BASS_ERROR_VIDEO_INVALID_HANDLE = 0x38,
		BASS_ERROR_VIDEO_INVALID_PARAMETER = 0x39,
		BASS_ERROR_VIDEO_NO_AUDIO = 0x3a,
		BASS_ERROR_VIDEO_NO_EFFECT = 0x3b,
		BASS_ERROR_VIDEO_NO_INTERFACE = 60,
		BASS_ERROR_VIDEO_NO_RENDERER = 0x3d,
		BASS_ERROR_VIDEO_NO_SUPPORT = 0x3e,
		BASS_ERROR_VIDEO_NO_VIDEO = 0x3f,
		BASS_ERROR_VIDEO_NOT_ALLOWED = 0x40,
		BASS_ERROR_VIDEO_NOT_CONNECTED = 0x41,
		BASS_ERROR_VIDEO_NOT_EXISTS = 0x42,
		BASS_ERROR_VIDEO_NOT_FOUND = 0x43,
		BASS_ERROR_VIDEO_NOT_READY = 0x44,
		BASS_ERROR_VIDEO_NULL_DEVICE = 0x45,
		BASS_ERROR_VIDEO_OPEN = 70,
		BASS_ERROR_VIDEO_OUTOFMEMORY = 0x47,
		BASS_ERROR_VIDEO_PARTIAL_RENDER = 0x48,
		BASS_ERROR_VIDEO_TIME_OUT = 0x49,
		BASS_ERROR_VIDEO_UNKNOWN_FILE_TYPE = 0x4a,
		BASS_ERROR_VIDEO_UNSUPPORT_STREAM = 0x4b,
		BASS_ERROR_VIDEO_VIDEO_FILTER = 0x4c,
		BASS_ERROR_WASAPI = 0x1388,
		BASS_ERROR_WMA_CODEC = 0x3eb,
		BASS_ERROR_WMA_DENIED = 0x3ea,
		BASS_ERROR_WMA_INDIVIDUAL = 0x3ec,
		BASS_ERROR_WMA_LICENSE = 0x3e8,
		BASS_ERROR_WMA_WM9 = 0x3e9,
		BASS_FX_ERROR_BPMINUSE = 0xfa1,
		BASS_FX_ERROR_NODECODE = 0xfa0,
		BASS_OK = 0,
		BASS_VST_ERROR_NOINPUTS = 0xbb8,
		BASS_VST_ERROR_NOOUTPUTS = 0xbb9,
		BASS_VST_ERROR_NOREALTIME = 0xbba
	}
}

