using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Analytics;

public static class FileLocations
{
	public static readonly IFolder androidLegacyStorage;

	public static readonly IFolder androidNewStorage;

	public static readonly IFile androidMigrationMarkerFile;

	public static readonly bool divertAndroidToMediaFolder;

	public static IFolder SavingFolder
	{
		get
		{
			if (!Application.isMobilePlatform && !Application.isEditor)
			{
				return GetBaseFolder().Parent.GetFolder("Saving");
			}
			return GetBaseFolder().GetFolder("Saving");
		}
	}

	public static IFolder CacheFolder => GetBaseFolder().GetFolder("Cache").Create();

	public static IFolder LogsFolder => GetBaseFolder().GetFolder("Logs").Create();

	public static IFolder SolarSystemsFolder => GetBaseFolder().GetFolder("Custom Solar Systems").Create();

	public static IFolder CustomAssetsFolderOld => ModsFolder.GetFolder("Custom Assets");

	public static IFolder CustomAssetsFolder => ModsFolder.GetFolder("Custom_Assets").Create();

	public static IFolder ModsFolder => GetBaseFolder().GetFolder((Application.isMobilePlatform || Application.isEditor) ? "Mods" : "../Mods");

	public static IFolder PublicTranslationFolder => GetBaseFolder().GetFolder(Application.isEditor ? "Translations" : "Custom Translations").Create();

	public static IFolder TranslationCacheFolder => CacheFolder.GetFolder("Translations").Create();

	public static IFolder BlueprintsFolder => SavingFolder.GetFolder("Blueprints").Create();

	public static IFolder WorldsFolder => SavingFolder.GetFolder("Worlds").Create();

	private static bool SaveInGameFolder
	{
		get
		{
			if (!Application.isEditor && SystemInfo.operatingSystemFamily != OperatingSystemFamily.Windows)
			{
				return SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX;
			}
			return true;
		}
	}

	public static IFile GetSettingsPath(string name)
	{
		return SavingFolder.GetFolder("Settings").Create().GetFile(name + ".txt");
	}

	public static void IncreaseCount(string name)
	{
		GetNotificationsPath(name).WriteText((GetCount(name) + 1).ToString());
	}

	public static int GetCount(string name)
	{
		IFile notificationsPath = GetNotificationsPath(name);
		if (!notificationsPath.Exists() || !int.TryParse(notificationsPath.ReadText(), out var result))
		{
			return 0;
		}
		return result;
	}

	public static bool GetOneTimeNotification(string name)
	{
		IFile notificationsPath = GetNotificationsPath(name);
		if (notificationsPath.Exists())
		{
			return false;
		}
		Write(notificationsPath);
		return true;
	}

	public static bool GetOneTimeNotification_Repeated(string name, TimeSpan repeatTime, int repeatSessions, bool onParseFailed)
	{
		IFile notificationsPath = GetNotificationsPath(name);
		if (!notificationsPath.Exists())
		{
			Write(notificationsPath);
			return true;
		}
		string[] array = notificationsPath.ReadText().Split('|');
		DateTime result = default(DateTime);
		int result2 = 0;
		if (array.Length != 2 || !DateTime.TryParse(array[0], out result) || !int.TryParse(array[1], out result2))
		{
			Write(notificationsPath);
			return onParseFailed;
		}
		if (DateTime.Now > result + repeatTime || AnalyticsSessionInfo.sessionCount > result2 + repeatSessions)
		{
			Write(notificationsPath);
			return true;
		}
		return false;
	}

	public static bool HasNotification(string name)
	{
		return GetNotificationsPath(name).Exists();
	}

	public static void WriteNotification(string name)
	{
		IFile notificationsPath = GetNotificationsPath(name);
		if (!notificationsPath.Exists())
		{
			Write(notificationsPath);
		}
	}

	private static void Write(IFile file)
	{
		file.WriteText(DateTime.Now.ToString(CultureInfo.InvariantCulture) + "|" + AnalyticsSessionInfo.sessionCount);
	}

	public static IFile GetNotificationsPath(string name)
	{
		return SavingFolder.GetFile("Notifications/" + name + ".txt");
	}

	public static IFolder GetBaseFolder()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			if (AndroidMigration.HasStartedMigration && !AndroidMigration.HasMigrated)
			{
				return androidLegacyStorage;
			}
			if (!divertAndroidToMediaFolder)
			{
				return androidLegacyStorage;
			}
			return androidNewStorage;
		}
		_ = Application.isEditor;
		if (!SaveInGameFolder)
		{
			return new DefaultFolder(Application.persistentDataPath);
		}
		return new DefaultFolder(Application.dataPath);
	}

	static FileLocations()
	{
		androidLegacyStorage = new DefaultFolder(Application.persistentDataPath);
		androidNewStorage = new DefaultFolder("/storage/emulated/0/Android/media/" + Application.identifier + "/");
		androidMigrationMarkerFile = androidNewStorage.GetFileUnsafe("migration_marker.txt");
	}
}
