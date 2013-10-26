using System;
using System.Net;
using System.Net.Sockets;

namespace UDP
{
	public class Client : IDisposable
	{
		private readonly string _ipServer;
		private readonly int _portServer;
		private IPEndPoint _server;
		private readonly UdpClient _client;

		private Action<byte[]> _sendSync;
		private Action<byte[], Action> _sendAsync;
		private Action _setSendDelegate;

		//public delegate void ReceivedMessage<in T>(T message);

		public Client(string ipServer, int portServer)
		{
			_ipServer = ipServer;
			_portServer = portServer;
			_client = new UdpClient();

			_sendSync = datas => _client.Send(datas, datas.Length, _ipServer, _portServer);
			_sendAsync = (datas, action) => _client.BeginSend(datas, datas.Length, _ipServer, _portServer, iasync =>
				{
					_client.EndSend(iasync);
					action();
				}, null);
			_setSendDelegate = () =>
				{
					_sendSync = data => _client.Send(data, data.Length, _server);
					_sendAsync = (data, action) => _client.BeginSend(data, data.Length, _server, async =>
						{
							_client.EndSend(async);
							action();
						}, null);
					_setSendDelegate = () => { };
				};
		}

		public void Send<T>(T message, Action<Exception> error = null)
		{
			try
			{
				byte[] datas = message.Serialize();
				_sendSync(datas);
				//if (_server != null)
				//	_client.Send(datas, datas.Length, _server);
				//else
				//	_client.Send(datas, datas.Length, _ipServer, _portServer);
			}
			catch (Exception e)
			{
				if (error != null)
					error(e);
			}
		}

		public void SendAsync<T>(T message, Action<Exception> error = null)
		{
			try
			{
				byte[] datas = message.Serialize();
				_sendAsync(datas, delegate { });
				//if (_server != null)
				//	_client.BeginSend(datas, datas.Length, _server, iasync => _client.EndSend(iasync), null);
				//else
				//	_client.BeginSend(datas, datas.Length, _ipServer, _portServer, iasync => _client.EndSend(iasync), null);
			}
			catch (Exception e)
			{
				if (error != null)
					error(e);
			}
		}

		public T Receive<T>(Action<Exception> error = null)
			where T : class
		{
			try
			{
				byte[] datas = _client.Receive(ref _server);
				_setSendDelegate();

				return datas.Deserialize<T>();
			}
			catch (Exception e)
			{
				if (error != null)
					error(e);
			}
			return null;
		}

		public void ReceiveAsync<T>(Action<T> receive, Action<Exception> error = null)
			where T : class
		{
			try
			{
				_client.BeginReceive(iasync =>
					{
						if (!iasync.IsCompleted)
							return;
						byte[] datas = _client.EndReceive(iasync, ref _server);
						_setSendDelegate();

						if (receive != null)
							receive(datas.Deserialize<T>());
					}, null);
			}
			catch (Exception e)
			{
				if (error != null)
					error(e);
			}
		}

		public T SendAndReceive<T>(T message, Action<Exception> error = null)
			where T : class
		{
			try
			{
				byte[] datas = message.Serialize();
				_sendSync(datas);
				//if (_server != null)
				//	_client.Send(datas, datas.Length, _server);
				//else
				//	_client.Send(datas, datas.Length, _ipServer, _portServer);

				// Receive
				return Receive<T>(error);
			}
			catch (Exception e)
			{
				if (error != null)
					error(e);
			}
			return null;
		}

		public void SendAndReceiveAsync<T>(T message, Action<T> receive, Action<Exception> error = null)
			where T : class
		{
			byte[] datas = message.Serialize();
			_sendAsync(datas, () =>
				{
					if (receive != null)
						ReceiveAsync(receive, error);
				});

			//if (_server != null)
			//	_client.BeginSend(datas, datas.Length, _server, iasync =>
			//	{
			//		_client.EndSend(iasync);

			//		if (receive != null)
			//			ReceiveAsync(receive, error);
			//	}, null);
			//else
			//	_client.BeginSend(datas, datas.Length, _ipServer, _portServer, iasync =>
			//	{
			//		_client.EndSend(iasync);

			//		if (receive != null)
			//			ReceiveAsync(receive, error);
			//	}, null);
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