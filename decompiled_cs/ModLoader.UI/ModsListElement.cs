using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SFS.Translations;
using SFS.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ModLoader.UI;

public class ModsListElement : MonoBehaviour
{
	[Serializable]
	public struct ModData
	{
		public ModType type;

		public string name;

		public string version;

		public string description;

		public string author;

		public string saveName;

		public Texture2D icon;

		public Task<Texture2D> loadingTexture;
	}

	[Serializable]
	public enum ModType
	{
		Mod,
		AssetsPack,
		TexturesPack,
		SolarSystem
	}

	public CanvasGroup canvasGroup;

	public ToggleButton activeToggle;

	public TextAdapter nameText;

	public TextAdapter infoText;

	public TextAdapter descriptionText;

	public RectTransform iconHolder;

	public Image iconImage;

	private Func<bool> active;

	private Action toggle;

	private bool activeOld;

	public bool IsChanged => active() != activeOld;

	public void DrawUI(ModData data)
	{
		ModsSettings.Data settings = ModsSettings.main.settings;
		active = data.type switch
		{
			ModType.Mod => () => settings.modsActive[data.saveName], 
			ModType.AssetsPack => () => settings.assetPacksActive[data.saveName], 
			ModType.TexturesPack => () => settings.texturePacksActive[data.saveName], 
			ModType.SolarSystem => () => settings.solarSystemsActive[data.saveName], 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		activeOld = active();
		toggle = data.type switch
		{
			ModType.Mod => delegate
			{
				Dictionary<string, bool> modsActive = settings.modsActive;
				string saveName = data.saveName;
				modsActive[saveName] = !modsActive[saveName];
				ModsSettings.main.SaveAll();
				CheckState();
			}, 
			ModType.AssetsPack => delegate
			{
				Dictionary<string, bool> assetPacksActive = settings.assetPacksActive;
				string saveName = data.saveName;
				assetPacksActive[saveName] = !assetPacksActive[saveName];
				ModsSettings.main.SaveAll();
				CheckState();
			}, 
			ModType.TexturesPack => delegate
			{
				Dictionary<string, bool> texturePacksActive = settings.texturePacksActive;
				string saveName = data.saveName;
				texturePacksActive[saveName] = !texturePacksActive[saveName];
				ModsSettings.main.SaveAll();
				CheckState();
			}, 
			ModType.SolarSystem => delegate
			{
				Dictionary<string, bool> solarSystemsActive = settings.solarSystemsActive;
				string saveName = data.saveName;
				solarSystemsActive[saveName] = !solarSystemsActive[saveName];
				ModsSettings.main.SaveAll();
				CheckState();
			}, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		nameText.Text = data.name;
		activeToggle.Bind(toggle, active);
		StringBuilder stringBuilder = new StringBuilder();
		string value = data.type switch
		{
			ModType.Mod => Loc.main.CodeMod_Name, 
			ModType.AssetsPack => Loc.main.PartAssetPack_Name, 
			ModType.TexturesPack => Loc.main.TexturePack_Name, 
			ModType.SolarSystem => Loc.main.SolarSystemPack_Name, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		stringBuilder.AppendLine(Loc.main.ModType_Label.Inject(value, "type"));
		stringBuilder.AppendLine(Loc.main.Version_Label.Inject(data.version, "version"));
		stringBuilder.AppendLine(Loc.main.Author_Label.Inject(data.author, "name"));
		infoText.Text = stringBuilder.ToString();
		StringBuilder stringBuilder2 = new StringBuilder();
		_ = data.type;
		stringBuilder2.Append(data.description);
		descriptionText.Text = stringBuilder2.ToString();
		CheckState();
	}

	private void CheckState()
	{
		canvasGroup.alpha = (active() ? 1f : 0.6f);
	}
}
