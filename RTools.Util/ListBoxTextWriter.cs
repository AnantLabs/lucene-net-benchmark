using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections;
using System.Windows.Forms;

namespace NUnit.UiKit
{
	/// <summary>
	/// This is a TextWriter that writes to a ListBox.  This buffers Write calls
	/// until the next WriteLine.
	/// </summary>
	public class ListBoxTextWriter : TextWriter
	{
		#region Fields

		StringBuilder writeBuffer;
		Queue writeQueue;
		ListBox listBox;
		WaitCallback flushWaitCallback;

		#endregion

		#region Properties

		int maxCount; 
		public int MaxCount 
		{ 
			get { return(maxCount); } 
			set { maxCount = value; } 
		}

		/// <summary>
		/// Required override.
		/// </summary>
		public override Encoding Encoding { get { return(Encoding.ASCII); } }

		#endregion

		#region Constructor

		public ListBoxTextWriter(ListBox listBox)
		{
			maxCount = 1000;
			this.listBox = listBox;
			writeBuffer = new StringBuilder();
			flushWaitCallback = new WaitCallback(Flush);
			writeQueue = Queue.Synchronized(new Queue());
		}

		#endregion

		#region Main ListBoxAdd method

		private void Flush(object obj)
		{
			if (listBox == null) return;
			if (writeQueue.Count > 0)
			{
				listBox.BeginUpdate();
				while (writeQueue.Count > 0)
				{
					string s = (string)writeQueue.Dequeue();
					if (s != null)
					{
						listBox.Items.Add(s);
					}
				}

				// drop items if we have too many
				int origCount = listBox.Items.Count;
				for (int i = 0; i < origCount - maxCount; i++) listBox.Items.RemoveAt(0);

				// scroll down so we show the latest lines
				int nVisibleLines = listBox.Height / listBox.GetItemHeight(0);
				listBox.TopIndex = listBox.Items.Count - nVisibleLines;
				listBox.EndUpdate();
			}
		}

		#endregion

		#region Write/WriteLine

		public override void WriteLine(string s)
		{
			lock(writeBuffer)
			{
				writeQueue.Enqueue(writeBuffer.ToString() + s);
				writeBuffer.Length = 0;
			}
			ThreadPool.QueueUserWorkItem(flushWaitCallback);
		}

		public override void Write(string s)
		{
			lock(writeBuffer)
			{
				writeBuffer.Append(s);
			}
		}

		public override void Write(string s, params object[] p)
		{
			lock(writeBuffer)
			{
				writeBuffer.AppendFormat(s, p);
			}
		}

		public override void WriteLine()
		{
			WriteLine("");
		}

		public override void WriteLine(string s, params object[] p)
		{
			WriteLine(String.Format(s, p));
		}

		#endregion
	}
}
