using System;
using SFS.Parsers.Json;
using UnityEngine;

namespace SFS.IO;

public abstract class SettingsBase<T> : MonoBehaviour where T : new()
{
	public T settings;

	protected abstract string FileName { get; }

	private IFile Path => FileLocations.GetSettingsPath(FileName);

	protected abstract void OnLoad();

	protected void Save()
	{
		IFile path = Path;
		string text = JsonWrapper.ToJson(settings, pretty: true);
		path.WriteText(text);
	}

	protected void Load()
	{
		try
		{
			IFile path = Path;
			settings = (path.Exists() ? JsonWrapper.FromJson<T>(path.ReadText()) : new T());
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			settings = new T();
		}
		OnLoad();
	}
}
