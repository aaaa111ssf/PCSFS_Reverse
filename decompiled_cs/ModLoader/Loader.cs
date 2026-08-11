using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ModLoader.UI;
using UnityEngine;

namespace ModLoader;

public class Loader : MonoBehaviour
{
	public static Loader main;

	private List<Mod> mods;

	private List<Mod> loadedMods = new List<Mod>();

	private static readonly Regex Version_Regex = new Regex("\\b([0-9]+\\.)+[0-9]+\\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	public Mod[] GetAllMods()
	{
		return mods.ToArray();
	}

	public Mod[] GetLoadedMods()
	{
		return loadedMods.ToArray();
	}

	public void Initialize_EarlyLoad()
	{
		FileLocations.ModsFolder.Create();
		MoveIndividualDLLs();
		LoadModList();
		foreach (Mod mod in mods)
		{
			try
			{
				LoadMod(mod);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				Debug.Log("Failed to load " + mod.DisplayName);
			}
		}
	}

	public void Initialize_Load()
	{
		foreach (Mod loadedMod in loadedMods)
		{
			try
			{
				loadedMod.LoadKeybindings?.Invoke();
				loadedMod.Load();
				Debug.Log("Loaded " + loadedMod.DisplayName);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	private void MoveIndividualDLLs()
	{
		try
		{
			foreach (IFile file in FileLocations.ModsFolder.GetFiles())
			{
				if (file.GetExtension() == "dll")
				{
					file.Move(FileLocations.ModsFolder.GetFolder(file.GetNameWithoutExtension()).Create().GetFile(file.Name));
				}
			}
		}
		catch (Exception exception)
		{
			Debug.Log("Failed to move Individual DLLs due to:");
			Debug.LogException(exception);
		}
	}

	private void LoadModList()
	{
		mods = new List<Mod>();
		List<IFile> list = new List<IFile>();
		List<Assembly> list2 = new List<Assembly>();
		foreach (IFolder folder in FileLocations.ModsFolder.GetFolders())
		{
			if (!(folder.Name == FileLocations.CustomAssetsFolder.Name) && !(folder.Name == FileLocations.CustomAssetsFolderOld.Name))
			{
				try
				{
					IFile file = folder.GetFile(folder.Name + ".dll");
					Assembly item = Assembly.LoadFrom(file.Path);
					list.Add(file);
					list2.Add(item);
				}
				catch (Exception arg)
				{
					Debug.LogError($"Failed to load mod in folder: {folder.Name}.\nError:{arg}");
				}
			}
		}
		for (int i = 0; i < list2.Count; i++)
		{
			IFile file2 = list[i];
			Assembly assembly = list2[i];
			try
			{
				Type type = assembly.GetTypes().FirstOrDefault((Type t) => typeof(Mod).IsAssignableFrom(t));
				if (type == null)
				{
					throw new Exception("Mod has no entry point: " + file2.GetNameWithoutExtension());
				}
				Mod mod = Activator.CreateInstance(type) as Mod;
				if (mods.Any((Mod m) => m.ModNameID == mod.ModNameID))
				{
					throw new Exception("Mod with the same ID is already loaded: " + file2.GetNameWithoutExtension());
				}
				mod.ModFolder = file2.Parent.Path;
				mods.Add(mod);
			}
			catch (ReflectionTypeLoadException innerException)
			{
				Debug.LogException(new Exception("Mod is most likely missing a dependency: " + file2.GetNameWithoutExtension(), innerException));
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	private void LoadMod(Mod mod)
	{
		if (!loadedMods.Any((Mod x) => x.ModNameID == mod.ModNameID) && LoadDependencies(mod))
		{
			ModsMenu.AddMod(new ModsListElement.ModData
			{
				author = mod.Author,
				description = mod.Description,
				icon = null,
				name = mod.DisplayName,
				type = ModsListElement.ModType.Mod,
				version = mod.ModVersion,
				saveName = mod.ModNameID,
				loadingTexture = TextureUtility.GetRemoteTexture(mod.IconLink)
			});
			if (ModsSettings.main.settings.modsActive.GetValueOrDefault(mod.ModNameID, defaultValue: true))
			{
				mod.Early_Load();
				loadedMods.Add(mod);
			}
		}
	}

	[Obsolete("Version verification will always return true")]
	private static bool VerifyVersion(string targetVersion, string currentVersion)
	{
		return true;
	}

	private bool LoadDependencies(Mod mod)
	{
		if (mod.Dependencies == null)
		{
			return true;
		}
		foreach (KeyValuePair<string, string> item in mod.Dependencies)
		{
			if (item.Key == mod.ModNameID)
			{
				Debug.LogWarning("Mod:" + mod.DisplayName + " with id:" + mod.ModNameID + ", has a reference to itself. Aborting");
				continue;
			}
			if (mods.All((Mod x) => x.ModNameID != item.Key))
			{
				Debug.Log(mod.DisplayName + " need dependency: " + item.Key);
				return false;
			}
			Mod mod2 = mods.Find((Mod x) => x.ModNameID == item.Key);
			if (!ModsSettings.main.settings.modsActive.GetValueOrDefault(mod2.ModNameID, defaultValue: true))
			{
				Debug.Log("Dependency mod of " + mod.DisplayName + " is disabled: " + mod2.DisplayName);
				return false;
			}
			LoadMod(mod2);
		}
		return true;
	}
}
