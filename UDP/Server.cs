using System;
using System.Net;
using System.Net.Sockets;

namespace UDP
{
	public class Server : IDisposable
	{
		private readonly UdpClient _server;
		private readonly int _port;
		public IPEndPoint Address { get; private set; }

		public Server(int port)
		{
			_port = port;
			_server = new UdpClient(_port);

			var m = "Test Local Address".EncodeString();
			_server.Send(m, m.Length, new IPEndPoint(IPAddress.Broadcast, 0));
			Address = (IPEndPoint)_server.Client.LocalEndPoint;
		}

		public void Send<T>(T message, string ip, int port)
			where T : Message
		{
			message.Sender = Address;
			byte[] datas = message.Serialize();
			_server.Send(datas, datas.Length, ip, port);
		}

		public void Send(string message, string ip, int port)
		{
			byte[] datas = message.EncodeString();
			_server.Send(datas, datas.Length, ip, port);
		}

		public void Send<T>(T message, IPEndPoint client)
			where T : Message
		{
			message.Sender = Address;
			byte[] datas = message.Serialize();
			_server.Send(datas, datas.Length, client);
		}

		public void Send(string message, IPEndPoint client)
		{
			byte[] datas = message.EncodeString();
			_server.Send(datas, datas.Length, client);
		}

		public T Receive<T>(IPEndPoint client = null)
			where T : Message, new()
		{
			T result = null;
			Receive<T>((t, c) => result = t);
			return result;
		}

		public string Receive(IPEndPoint client = null)
		{
			string result = null;
			Receive(((s, c) => result = s), client);
			return result;
		}

		public void Receive<T>(Action<T, IPEndPoint> callBack, IPEndPoint client = null)
			where T : Message
		{
			byte[] datas = _server.Receive(ref client);
			if (callBack != null)
				callBack(datas.Deserialize<T>(), client);
		}

		public void Receive(Action<string, IPEndPoint> callBack, IPEndPoint client = null)
		{
			byte[] datas = _server.Receive(ref client);
			if (callBack != null)
				callBack(datas.DecodeBytes(), client);
		}

		public void Close()
		{
			_server.Close();
		}

		/// <summary>
		/// Exécute les tâches définies par l'application associées à la libération ou à la redéfinition des ressources non managées.
		/// </summary>
		void IDisposable.Dispose()
		{
			Close();
		}
	}
}