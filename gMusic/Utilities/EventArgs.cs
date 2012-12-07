using System;

namespace GoogleMusic
{
	public class EventArgs<T> : EventArgs
	{
		public EventArgs (T value)
		{
			m_value = value;
		}
		
		private T m_value;
		
		public T Value {
			get { return m_value; }
		}
	}
	
	public class EventArgs<T, U> : EventArgs<T>
	{
		
		public EventArgs (T value, U value2)
			: base(value)
		{
			m_value2 = value2;
		}
		
		private U m_value2;
		
		public U Value2 {
			get { return m_value2; }
		}
	}
}

