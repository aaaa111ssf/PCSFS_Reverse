using System;
using System.Collections.Generic;
using System.Globalization;
using SFS.IO;
using SFS.Input;
using SFS.Translations;
using SFS.World;
using SFS.WorldBase;
using UnityEngine;

namespace SFS.UI;

public class CreateWorldMenu : BasicMenu
{
	public WorldsMenu worldsMenu;

	public Button createWorldButton;

	public TextBoxAdapter worldNameField;

	public TextAdapter solarSystemButtonText;

	public ButtonGroup difficultyGroup;

	public ButtonGroup modeGroup;

	public ToggleButton quicksavesToggle;

	public GameObject togglesHolder;

	public TextAdapter difficultyInfo;

	private static readonly List<WorldMode.Mode> ModeIndices = new List<WorldMode.Mode>
	{
		WorldMode.Mode.Sandbox,
		WorldMode.Mode.Challenge
	};

	private static readonly List<Difficulty> DifficultyIndices = new List<Difficulty>
	{
		Difficulty.Normal,
		Difficulty.Hard,
		Difficulty.Realistic
	};

	private string solarSystemName;

	private bool quicksaves;

	private void Start()
	{
		menuHolder.gameObject.SetActive(value: false);
		quicksavesToggle.Bind(ToggleQuicksaves, () => quicksaves);
	}

	public override void Open()
	{
		worldNameField.Text = Loc.main.Default_World_Name;
		SetSolarSystem("");
		modeGroup.SetSelectedIndex(0);
		quicksaves = true;
		quicksavesToggle.UpdateUI(instantAnimation: true);
		difficultyGroup.SetSelectedIndex(0);
		base.Open();
	}

	public override void Close()
	{
		worldNameField.Close();
		base.Close();
	}

	private void SetSolarSystem(string a)
	{
		solarSystemName = a;
		solarSystemButtonText.Text = ((a == "") ? ((string)Loc.main.Default_Solar_System) : a);
	}

	public void OpenSelectSolarSystemMenu()
	{
		SizeSyncerBuilder.Carrier horizontal;
		List<MenuElement> buttons = new List<MenuElement>
		{
			TextBuilder.CreateText(() => Loc.main.Select_Solar_System),
			ElementGenerator.VerticalSpace(50),
			new SizeSyncerBuilder(out horizontal)
		};
		AddButton(Loc.main.Default_Solar_System, "");
		if (DevSettings.FullVersion)
		{
			foreach (string allSolarSystem in PlanetLoader.GetAllSolarSystems())
			{
				AddButton(Loc.main.Custom_Solar_System.Inject(allSolarSystem, "name"), allSolarSystem);
			}
		}
		MenuGenerator.OpenMenu(CancelButton.Cancel, CloseMode.Current, buttons.ToArray());
		void AddButton(string text, string solarSystemName)
		{
			buttons.Add(ButtonBuilder.CreateButton(horizontal, () => text, delegate
			{
				SetSolarSystem(solarSystemName);
			}, CloseMode.Current));
		}
	}

	private void ToggleQuicksaves()
	{
		quicksaves = !quicksaves;
	}

	public void OnDifficultyCycle()
	{
		string text = "";
		if (DifficultyIndices.IsValidIndex(difficultyGroup.SelectedIndex))
		{
			Difficulty difficulty = DifficultyIndices[difficultyGroup.SelectedIndex];
			text += Loc.main.Difficulty_Scale_Stat.Inject((20.0 / difficulty.DefaultRadiusScale).ToString(CultureInfo.InvariantCulture), "scale");
			text = text + "\n" + Loc.main.Difficulty_Isp_Stat.Inject(difficulty.IspMultiplier.ToString(CultureInfo.InvariantCulture), "value");
			text = text + "\n" + Loc.main.Difficulty_Dry_Mass_Stat.Inject(difficulty.DryMassMultiplier.ToString(CultureInfo.InvariantCulture), "value");
			text = text + "\n" + Loc.main.Difficulty_Engine_Mass_Stat.Inject(difficulty.EngineMassMultiplier.ToString(CultureInfo.InvariantCulture), "value");
		}
		difficultyInfo.Text = text;
		UpdateCanCreate();
	}

	public void OnModeCycle()
	{
		togglesHolder.SetActive(ModeIndices.IsValidIndex(modeGroup.SelectedIndex) && ModeIndices[modeGroup.SelectedIndex] == WorldMode.Mode.Challenge);
		UpdateCanCreate();
	}

	private void UpdateCanCreate()
	{
		createWorldButton.SetEnabled(modeGroup.SelectedIndex != -1 && difficultyGroup.SelectedIndex != -1);
	}

	public void CreateWorld()
	{
		if (!FileLocations.WorldsFolder.Exists())
		{
			FileLocations.WorldsFolder.Create();
		}
		string worldName = ((worldNameField.Text == "") ? ((string)Loc.main.Default_World_Name) : worldNameField.Text);
		string text = solarSystemName;
		Difficulty difficulty = DifficultyIndices[difficultyGroup.SelectedIndex];
		WorldMode mode = new WorldMode(ModeIndices[modeGroup.SelectedIndex]);
		Close();
		CreateWorld(worldName, text, difficulty, mode, quicksaves);
		worldsMenu.OnWorldCreated(worldName);
	}

	public static void CreateWorld(string worldName, string solarSystemName = "", Difficulty difficulty = null, WorldMode mode = null, bool quicksaves = true)
	{
		if (difficulty == null)
		{
			difficulty = Difficulty.Normal;
		}
		if (mode == null)
		{
			mode = new WorldMode(WorldMode.Mode.Sandbox);
		}
		worldName = PathUtility.MakeUsable(worldName, Loc.main.Default_World_Name);
		worldName = PathUtility.AutoNameExisting(worldName, FileLocations.WorldsFolder);
		WorldReference worldReference = new WorldReference(worldName);
		SolarSystemReference solarSystem = new SolarSystemReference(solarSystemName);
		mode.allowQuicksaves = mode.mode == WorldMode.Mode.Sandbox || quicksaves;
		int num;
		if (worldName.ToLower() == "test career")
		{
			num = 0;
			if (num != 0)
			{
				mode.mode = WorldMode.Mode.Career;
			}
		}
		else
		{
			num = 0;
		}
		WorldSettings settings = new WorldSettings(solarSystem, difficulty, mode, new WorldPlaytime(DateTime.Now.Ticks, 0.0), new SandboxSettings.Data());
		worldReference.SaveWorldSettings(settings);
		if (num != 0)
		{
			Menu.read.Open(() => "Career mode is currently a very minimal prototype with only a few missions and unlocks\n\nIf this prototype gets positive feedback, we will add tons of missions, unlocks and polish it to perfection\n\n- Spaceflight Simulator developer team");
		}
	}
}
