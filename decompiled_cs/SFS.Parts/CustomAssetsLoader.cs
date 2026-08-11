using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using SFS.UI;
using SFS.WorldBase;
using UnityEngine;

namespace SFS.Parts;

public static class CustomAssetsLoader
{
	public static readonly StringBuilder Report = new StringBuilder();

	public static Stack<Action> onUnload = new Stack<Action>();

	public static bool finishedLoading;

	public static void LoadAllCustomAssets()
	{
		Load();
	}

	public static void UnloadAll()
	{
		while (onUnload.Count > 0)
		{
			onUnload.Pop()();
		}
	}

	private static void Load()
	{
		LoadSequence();
	}

	public static async UniTask LoadSequence()
	{
		_ = 1;
		try
		{
			PlanetLoader.GetSolarSystemsForModsMenu();
			await CustomAssetsPacksLoader.LoadAssetPacks();
			await TextureLoader.LoadTexturePacks();
			if (Report.Length > 0)
			{
				Menu.read.ShowReport(Report, delegate
				{
				});
			}
			finishedLoading = true;
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
	}
}
