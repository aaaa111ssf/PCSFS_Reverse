using System;
using System.Collections.Generic;
using System.Linq;
using SFS.Input;
using SFS.Parts;
using SFS.Translations;
using SFS.UI;
using UnityEngine;

namespace ModLoader.UI;

public class ModsMenu : BasicMenu
{
	public static ModsMenu main;

	public RectTransform elementsHolder;

	public GameObject elementPrefab;

	private static List<ModsListElement.ModData> elements = new List<ModsListElement.ModData>();

	private static Pool<ModsListElement> elementPool;

	private void Awake()
	{
		main = this;
		elementPool = new Pool<ModsListElement>(Create, Reset);
		ModsListElement Create()
		{
			GameObject obj = UnityEngine.Object.Instantiate(elementPrefab, elementsHolder);
			obj.gameObject.SetActive(value: true);
			return obj.GetComponent<ModsListElement>();
		}
		static void Reset(ModsListElement element)
		{
			element.gameObject.SetActive(value: false);
		}
	}

	private void Start()
	{
		elementPrefab.SetActive(value: false);
		menuHolder.gameObject.SetActive(value: false);
	}

	public override void OnOpen()
	{
		if (!CustomAssetsLoader.finishedLoading)
		{
			Menu.read.ShowReport(Loc.main.Mods_Still_Loading, delegate
			{
			}, CloseMode.Stack);
			return;
		}
		base.OnOpen();
		DrawUI();
	}

	public static void AddMod(ModsListElement.ModData data)
	{
		ModsSettings.Data settings = ModsSettings.main.settings;
		switch (data.type)
		{
		case ModsListElement.ModType.Mod:
			settings.modsActive.TryAdd(data.saveName, value: true);
			break;
		case ModsListElement.ModType.AssetsPack:
			settings.assetPacksActive.TryAdd(data.saveName, value: true);
			break;
		case ModsListElement.ModType.TexturesPack:
			settings.texturePacksActive.TryAdd(data.saveName, value: true);
			break;
		case ModsListElement.ModType.SolarSystem:
			settings.solarSystemsActive.TryAdd(data.saveName, value: true);
			break;
		}
		ModsSettings.main.SaveAll();
		elements.Add(data);
	}

	public static void RemoveMod(ModsListElement.ModData data)
	{
		elements.Remove(data);
	}

	public void DrawUI()
	{
		elementPool.Reset();
		foreach (ModsListElement.ModData element in elements)
		{
			ModsListElement item = elementPool.GetItem();
			item.gameObject.SetActive(value: true);
			item.DrawUI(element);
			if (element.icon != null)
			{
				item.iconHolder.gameObject.SetActive(value: true);
				item.iconImage.sprite = Sprite.Create(element.icon, new Rect(0f, 0f, element.icon.width, element.icon.height), new Vector2(0.5f, 0.5f));
			}
			else if (element.loadingTexture != null && element.loadingTexture.IsCompleted && element.loadingTexture.Result != null)
			{
				item.iconHolder.gameObject.SetActive(value: true);
				Texture2D result = element.loadingTexture.Result;
				item.iconImage.sprite = Sprite.Create(result, new Rect(0f, 0f, result.width, result.height), new Vector2(0.5f, 0.5f));
			}
			else
			{
				item.iconHolder.gameObject.SetActive(value: false);
			}
		}
	}

	public void OpenModsFolder()
	{
		Application.OpenURL(new Uri(FileLocations.ModsFolder.Location).AbsoluteUri);
	}

	public void OpenModDownloads()
	{
		Application.OpenURL("https://jmnet.one/sfs/forum/index.php?forums/authorised-game-mods.101/");
	}

	public void CloseWindow()
	{
		ModsSettings.main.SaveAll();
		Action onConfirm = ApplicationUtility.Relaunch;
		if (elementPool.Items.Any((ModsListElement e) => e.IsChanged))
		{
			MenuGenerator.OpenConfirmation(CloseMode.Current, () => Loc.main.Changes_Warning, () => Loc.main.Relaunch, onConfirm, null, Close);
		}
		else
		{
			Close();
		}
	}
}
