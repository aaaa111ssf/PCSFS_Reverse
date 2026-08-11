using System;
using Beebyte.Obfuscator;
using SFS.Builds;
using SFS.Input;
using SFS.Parts.Modules;
using SFS.Translations;
using SFS.UI;
using SFS.Variables;
using SFS.WorldBase;
using UnityEngine;

namespace SFS.World;

public class SandboxSettings : BasicMenu
{
	[Serializable]
	[Skip]
	public class Data
	{
		public bool infiniteFuel;

		public bool noAtmosphericDrag;

		public bool unbreakableParts;

		public bool noGravity;

		public bool noHeatDamage;

		public bool noBurnMarks;

		public bool infiniteBuildArea;

		public bool partClipping;
	}

	public static SandboxSettings main;

	public ToggleButton infiniteBuildArea;

	public ToggleButton partClipping;

	public ToggleButton infiniteFuel;

	public ToggleButton infiniteOxygen;

	public ToggleButton noAtmosphericDrag;

	public ToggleButton unbreakableParts;

	public ToggleButton noGravity;

	public ToggleButton noHeatDamage;

	public ToggleButton noBurnMarks;

	public Button buyInfiniteBuildAreaButton;

	public Button buyCheatsButton;

	public Button preventUse_InfiniteArea;

	public Button preventUse_Cheats;

	public ContainerElement containerElement;

	public SettingsMenuSizer settingsMenuSizer;

	public ButtonPC fullVersionButton;

	public GameObject[] fullVersionElements;

	public Button teleportButton;

	public Button refillButton;

	public Button unlockCheatsButton;

	[HideInInspector]
	public Data settings;

	[HideInInspector]
	public bool initialized;

	public Transform settingsTransform;

	public Event_Local onToggleCheat = new Event_Local();

	private void Awake()
	{
		main = this;
	}

	private void Start()
	{
		if (settings == null)
		{
			settings = new Data();
		}
		infiniteBuildArea.Bind(ToggleInfiniteBuildArea, () => settings.infiniteBuildArea);
		partClipping.Bind(TogglePartClipping, () => settings.partClipping);
		infiniteFuel.Bind(ToggleInfiniteFuel, () => settings.infiniteFuel);
		noAtmosphericDrag.Bind(ToggleNoAtmosphericDrag, () => settings.noAtmosphericDrag);
		unbreakableParts.Bind(ToggleUnbreakableParts, () => settings.unbreakableParts);
		noGravity.Bind(ToggleNoGravity, () => settings.noGravity);
		noHeatDamage.Bind(ToggleNoHeatDamage, () => settings.noHeatDamage);
		noBurnMarks.Bind(ToggleNoBurnMarks, () => settings.noBurnMarks);
		fullVersionButton.onClick += (Action)delegate
		{
			Base.sceneLoader.LoadHomeScene("Full Version");
		};
		GameObject[] array = fullVersionElements;
		for (int num = 0; num < array.Length; num++)
		{
			array[num].SetActive(!DevSettings.FullVersion);
		}
		initialized = true;
		Base.worldBase.insideWorld.OnChange += new Action(UpdatePreventUse);
		teleportButton.onClick += new Action(TeleportMenu.main.OpenFromCheats);
		refillButton.onClick += new Action(Refill);
		unlockCheatsButton.onClick += new Action(UnlockCheats);
	}

	private void UpdatePreventUse()
	{
		bool flag = (bool)Base.worldBase.insideWorld && Base.worldBase.AllowsCheats;
		bool flag2 = (bool)Base.worldBase.insideWorld && Base.worldBase.AllowsCheats;
		if (!DevSettings.FullVersion)
		{
			flag = false;
			flag2 = false;
		}
		preventUse_Cheats.gameObject.SetActive(!flag2);
	}

	private bool ShowUnlockCheats()
	{
		if (!DevSettings.FullVersion)
		{
			return false;
		}
		return !Base.worldBase.AllowsCheats;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		bool flag = PlayerController.main?.player?.Value is Rocket;
		teleportButton.SetEnabled(flag);
		if (PlayerController.main?.player?.Value is Rocket rocket)
		{
			refillButton.SetEnabled(rocket.partHolder.GetModules<ResourceModule>().Length != 0);
		}
		else
		{
			refillButton.SetEnabled(enabled: false);
		}
		unlockCheatsButton.gameObject.SetActive(ShowUnlockCheats());
	}

	private void UnlockCheats()
	{
		MenuGenerator.OpenConfirmation(CloseMode.Current, () => Loc.main.Unlock_Cheats_Warning, () => Loc.main.Unlock_Cheats_Button, delegate
		{
			WorldSettings worldSettings = Base.worldBase.settings;
			worldSettings.mode.mode = WorldMode.Mode.Sandbox;
			worldSettings.mode.allowQuicksaves = true;
			Base.worldBase.paths.SaveWorldSettings(worldSettings);
			UpdatePreventUse();
			OnOpen();
		});
	}

	private void Refill()
	{
		if (PlayerController.main?.player?.Value is Rocket rocket)
		{
			ResourceModule[] modules = rocket.partHolder.GetModules<ResourceModule>();
			foreach (ResourceModule resourceModule in modules)
			{
				resourceModule.AddResource(resourceModule.TotalResourceCapacity - resourceModule.ResourceAmount);
			}
			ScreenManager.main.CloseStack();
			MsgDrawer.main.Log(Loc.main.Refilled_Tanks);
		}
	}

	private void ToggleInfiniteBuildArea()
	{
		Data data = settings;
		data.infiniteBuildArea = !data.infiniteBuildArea;
		OnToggle();
	}

	private void TogglePartClipping()
	{
		Data data = settings;
		data.partClipping = !data.partClipping;
		OnToggle();
	}

	private void ToggleInfiniteFuel()
	{
		Data data = settings;
		data.infiniteFuel = !data.infiniteFuel;
		OnToggle();
	}

	private void ToggleNoAtmosphericDrag()
	{
		Data data = settings;
		data.noAtmosphericDrag = !data.noAtmosphericDrag;
		OnToggle();
	}

	private void ToggleUnbreakableParts()
	{
		Data data = settings;
		data.unbreakableParts = !data.unbreakableParts;
		OnToggle();
	}

	private void ToggleNoGravity()
	{
		Data data = settings;
		data.noGravity = !data.noGravity;
		OnToggle();
	}

	private void ToggleNoHeatDamage()
	{
		Data data = settings;
		data.noHeatDamage = !data.noHeatDamage;
		OnToggle();
	}

	private void ToggleNoBurnMarks()
	{
		Data data = settings;
		data.noBurnMarks = !data.noBurnMarks;
		OnToggle();
	}

	private void OnToggle()
	{
		Base.worldBase.paths.SaveWorldSettings(Base.worldBase.settings);
		UpdateUI(instantAnimation: false);
		if (BuildManager.main != null)
		{
			BuildManager.main.buildGridSize.UpdateBuildSpaceSize();
		}
		onToggleCheat?.Invoke();
	}

	public void UpdateUI(bool instantAnimation)
	{
		if (initialized)
		{
			infiniteBuildArea.UpdateUI(instantAnimation);
			partClipping.UpdateUI(instantAnimation);
			infiniteFuel.UpdateUI(instantAnimation);
			noAtmosphericDrag.UpdateUI(instantAnimation);
			unbreakableParts.UpdateUI(instantAnimation);
			noGravity.UpdateUI(instantAnimation);
			noHeatDamage.UpdateUI(instantAnimation);
			noBurnMarks.UpdateUI(instantAnimation);
		}
	}
}
