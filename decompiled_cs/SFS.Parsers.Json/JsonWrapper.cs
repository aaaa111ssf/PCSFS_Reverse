using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace SFS.Parsers.Json;

public static class JsonWrapper
{
	public class VersionedData<T>
	{
		public T data;
	}

	private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
	{
		ContractResolver = new MainContractResolver(),
		ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
		NullValueHandling = NullValueHandling.Ignore
	};

	public static bool TryLoadJson<T>(IFile file, out T data)
	{
		try
		{
			if (!file.Exists())
			{
				data = default(T);
				return false;
			}
			string json = file.ReadText();
			data = FromJson<T>(json);
			return data != null;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			data = default(T);
			return false;
		}
	}

	public static void SaveAsJson(IFile file, object data, bool pretty)
	{
		file.WriteText(ToJson(data, pretty));
	}

	public static T FromJson<T>(string json)
	{
		VersionedData<T> versionedData = FromJsonVersionData<T>(json);
		try
		{
			if (versionedData == null)
			{
				return JsonConvert.DeserializeObject<T>(json, SerializerSettings);
			}
		}
		catch (Exception)
		{
		}
		return default(T);
	}

	private static VersionedData<T> FromJsonVersionData<T>(string json)
	{
		try
		{
			VersionedData<T> versionedData = JsonConvert.DeserializeObject<VersionedData<T>>(json, SerializerSettings);
			if (versionedData.data == null)
			{
				return null;
			}
			return versionedData;
		}
		catch
		{
		}
		return null;
	}

	public static string ToJson(object data, bool pretty)
	{
		return JsonConvert.SerializeObject(data, (pretty || Application.isEditor) ? Formatting.Indented : Formatting.None, SerializerSettings);
	}

	public static string Serialize(this JsonSerializer serializer, object data)
	{
		using StringWriter stringWriter = new StringWriter();
		serializer.Serialize(stringWriter, data);
		return stringWriter.ToString();
	}
}
