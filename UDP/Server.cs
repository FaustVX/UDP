using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace UDP
{
	public class Server : IDisposable
	{
		private readonly UdpClient _server;
		private readonly int _port;

		public Server(int port)
		{
			_port = port;
			_server = new UdpClient(_port);
		}

		public void Send<T>(T message, string ip, int port)
			where T : class
		{
			byte[] datas = message.Serialize();
			_server.Send(datas, datas.Length, ip, port);
		}

		public void Send(string message, string ip, int port)
		{
			byte[] datas = message.EncodeString();
			_server.Send(datas, datas.Length, ip, port);
		}

		public void Send<T>(T message, IPEndPoint client)
			where T : class
		{
			byte[] datas = message.Serialize();
			_server.Send(datas, datas.Length, client);
		}

		public void Send(string message, IPEndPoint client)
		{
			byte[] datas = message.EncodeString();
			_server.Send(datas, datas.Length, client);
		}

		public T Receive<T>(IPEndPoint client = null)
			where T : class, new()
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
			where T : class
		{
			byte[] datas = _server.Receive(ref client);
			Client des = datas.Deserialize<Client>();
			if (des != null)
				Send(des, client);
			else if (callBack != null)
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