using System;
using UnityEngine;

public static class AndroidMigration
{
	public static bool HasStartedMigration { get; private set; }

	public static bool HasMigrated { get; private set; }

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void TryMigrateFiles()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			Debug.Log("Not migrating");
			return;
		}
		IFile file = FileLocations.androidNewStorage.GetFile("migration_log.txt");
		if (!FileLocations.divertAndroidToMediaFolder)
		{
			file.AppendText("Not migrating");
			Debug.Log("Not migrating");
			return;
		}
		if (FileLocations.androidMigrationMarkerFile.Exists())
		{
			file.AppendText("Already migrated");
			Debug.Log("Already migrated");
			return;
		}
		Debug.Log("Migrating files from:" + FileLocations.androidLegacyStorage.Path + " to:" + FileLocations.androidNewStorage.Path);
		HasStartedMigration = true;
		try
		{
			FileLocations.androidLegacyStorage.Copy(FileLocations.androidNewStorage);
			FileLocations.androidMigrationMarkerFile.WriteText("We have migrated your files :)");
			HasMigrated = true;
		}
		catch (Exception ex)
		{
			file.AppendText(ex.ToString());
		}
	}
}
