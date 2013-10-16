using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;

namespace UDP
{
	public static class UDPHelper
	{
		private class UDPClientBinder : SerializationBinder
		{
			/// <summary>
			/// En cas de substitution dans une classe dérivée, contrôle la liaison d'un objet sérialisé avec un type.
			/// </summary>
			/// <returns>
			/// Type de l'objet dont le formateur crée une nouvelle instance.
			/// </returns>
			/// <param name="assemblyName">Spécifie le nom <see cref="T:System.Reflection.Assembly"/> de l'objet sérialisé.</param><param name="typeName">Spécifie le nom <see cref="T:System.Type"/> de l'objet sérialisé.</param>
			public override Type BindToType(string assemblyName, string typeName)
			{
				return (typeName == typeof (Client).FullName) ? typeof (Client) : null;
			}
		}
		private static readonly Dictionary<Type, SerializationBinder> TypeConvertor;

		static UDPHelper()
		{
			TypeConvertor = new Dictionary<Type, SerializationBinder>();
			AddtypeConvertor<Client>(new UDPClientBinder());
		}

		/// <summary>
		/// Add a Type convertor to deserialize object
		/// </summary>
		/// <typeparam name="T">Type to convert</typeparam>
		/// <param name="typeConvertor">Convertor for thee type</param>
		public static void AddtypeConvertor<T>(SerializationBinder typeConvertor)
		{
			AddTypeConvertor(typeof(T), typeConvertor);
		}

		/// <summary>
		/// Add a Type convertor to deserialize object
		/// </summary>
		/// <param name="type">Type to convert</param>
		/// <param name="typeConvertor">Convertor for thee type</param>
		public static void AddTypeConvertor(this Type type, SerializationBinder typeConvertor)
		{
			if (TypeConvertor.ContainsKey(type))
				TypeConvertor[type] = typeConvertor;
			else
				TypeConvertor.Add(type, typeConvertor);
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
			Type baseType = typeof(T);
			for (; baseType != null; baseType = baseType.BaseType)
				if (TypeConvertor.ContainsKey(baseType))
					break;
			if (baseType == null)
				return null;

			SerializationBinder typeConvertor = TypeConvertor[typeof (T)];
			return Deserialize<T>(buffer, typeConvertor);
		}

		public static T Deserialize<T>(this byte[] buffer, SerializationBinder typeConvertor)
			where T : class
		{
			AddtypeConvertor<T>(typeConvertor);

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