using System;
using System.Net;
using System.Net.Sockets;

namespace UDP
{
	[Serializable]
	public class Client : IDisposable
	{
		private readonly string _ipServer;
		private readonly int _portServer;
		[NonSerialized]
		private IPEndPoint _server;
		[NonSerialized]
		private readonly UdpClient _client;

		public Client(string ipServer, int portServer)
		{
			_ipServer = ipServer;
			_portServer = portServer;
			_client = new UdpClient();
		}

		
		public void Send<T>(T message) where T : class
		{
			Send(message, null);
		}

		public void Send<T>(T message, Action<T> receive, Action<Exception> error = null)
			where T : class
		{
			try
			{
				byte[] datas = message.Serialize();
				if (_server != null)
					_client.Send(datas, datas.Length, _server);
				else
					_client.Send(datas, datas.Length, _ipServer, _portServer);

				Receive(receive);
			}
			catch (Exception e)
			{
				if (error != null)
					error(e);
			}
		}

		public void Send(string message)
		{
			Send(message, null);
		}

		public void Send(string message, Action<string> receive, Action<Exception> error = null)
		{
			try
			{
				byte[] datas = message.EncodeString();
				if (_server != null)
					_client.Send(datas, datas.Length, _server);
				else
					_client.Send(datas, datas.Length, _ipServer, _portServer);

				Receive(receive);
			}
			catch (Exception e)
			{
				if (error != null)
					error(e);
			}
		}

		public T Receive<T>()
			where T : class
		{
			T result = null;
			Receive<T>(t => result = t);
			return result;
		}

		public string Receive()
		{
			string result = null;
			Receive(s => result = s);
			return result;
		}

		public void Receive<T>(Action<T> callback)
			where T : class
		{
			byte[] datas = _client.Receive(ref _server);
			if (callback != null)
				callback(datas.Deserialize<T>());
		}

		public void Receive(Action<string> callBack)
		{
			byte[] datas = _client.Receive(ref _server);
			if (callBack != null)
				callBack(datas.DecodeBytes());
		}

		public void Close()
		{
			_client.Close();
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