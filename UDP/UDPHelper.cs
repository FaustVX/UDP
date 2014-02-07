using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;

namespace UDP
{
	public static class UDPHelper
	{
		private class Serializer : SerializationBinder
		{
			public override Type BindToType(string assemblyName, string typeName)
			{
				var assembly = Assembly.Load(assemblyName);
				var type = assembly.GetType(typeName);
				return type;
			}
		}

		public static SerializationBinder TypeConvertor { get; private set; }

		static UDPHelper()
		{
			TypeConvertor = new Serializer();
		}

		public static byte[] Serialize<T>(this T o)
		{
			byte[] arr;
			MemoryStream memoryStream;
			using (memoryStream = new MemoryStream())
			{
				BinaryFormatter bf = new BinaryFormatter();
				bf.Serialize(memoryStream, o);
				arr = memoryStream.ToArray();
			}
			return arr;
		}

		public static T Deserialize<T>(this byte[] buffer)
			where T : class
		{
			return Deserialize<T>(buffer, TypeConvertor);
		}

		public static T Deserialize<T>(this byte[] buffer, SerializationBinder typeConvertor)
			where T : class
		{
			//AddtypeConvertor<T>(typeConvertor);

			T returnValue = null;
			using (MemoryStream memoryStream = new MemoryStream(buffer))
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter {Binder = typeConvertor};

				memoryStream.Position = 0;
				try
				{
					returnValue = (T)binaryFormatter.Deserialize(memoryStream);
				}
				catch (InvalidCastException e)
				{
					if (typeof(T) == typeof(Client))
						return null;
				}
			}

			return returnValue; 
		}

		[DebuggerStepThrough]
		public static byte[] EncodeString(this string s)
		{
			return s.EncodeString(Encoding.Default);
		}

		[DebuggerStepThrough]
		public static string DecodeBytes(this byte[] b)
		{
			return b.DecodeBytes(Encoding.Default);
		}

		[DebuggerStepThrough]
		public static byte[] EncodeString(this string s, Encoding encoding)
		{
			return encoding.GetBytes(s);
		}

		[DebuggerStepThrough]
		public static string DecodeBytes(this byte[] b, Encoding encoding)
		{
			return encoding.GetString(b);
		}
	}
}