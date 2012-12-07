using System;

namespace Un4seen.Bass
{
	public static class BassUtil
	{
		public static int LowWord(long qWord)
		{
			return (int) (((ulong) qWord) & 0xffffffffL);
		}

		public static int LowWord32(int dWord)
		{
			return (dWord & 0xffff);
		}
		public static short HighWord(int dWord)
		{
			return (short) ((dWord >> 0x10) & 0xffff);
		}

		public static int HighWord(long qWord)
		{
			return (int) (((ulong) (qWord >> 0x20)) & 0xffffffffL);
		}

		public static int HighWord32(int dWord)
		{
			return ((dWord >> 0x10) & 0xffff);
		}
	}
}

