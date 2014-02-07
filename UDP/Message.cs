using System;
using System.Net;

namespace UDP
{
	[Serializable]
	public abstract class Message
	{
		public IPEndPoint Sender { get; internal set; }
	}
}