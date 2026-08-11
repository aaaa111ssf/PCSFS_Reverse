using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SFS.Analytics;
using SFS.Audio;
using SFS.Input;
using SFS.Translations;
using UnityEngine;
using UnityEngine.Diagnostics;

namespace SFS.UI;

public class HomeManager : BasicMenu
{
	public static HomeManager main;

	[Space]
	[Space]
	public MusicPlaylistPlayer menuMusic;

	public Material starsMaterial;

	public SalesSystem salesSystem;

	public DevelopmentMenu developmentMenu;

	[Space]
	public GameObject steamBanner;

	public GameObject sequelBanner;

	public Button settingsButton;

	public GameObject fullVersionButton;

	public BasicMenu fullVersionSalePage;

	public Button youtubeButton;

	public Button discordButton;

	public Button forumsButton;

	public Button tiktokButton;

	public Button instagramButton;

	public Button vkButton;

	private void Awake()
	{
		main = this;
	}

	private void Start()
	{
		if (FileLocations.GetBaseFolder().GetFile("crash_test.txt").Exists())
		{
			BuildValidatorHook();
		}
		fullVersionButton.SetActive(DevSettings.ShowFullVersionButton);
		menuMusic.StartPlaying(5f);
		starsMaterial.SetColor("_Color", new Color(1f, 1f, 1f, 1f));
		string versionText = GetVersion();
		if (!string.IsNullOrEmpty(Base.sceneLoader.sceneSettings.openShop))
		{
			if (Base.sceneLoader.sceneSettings.openShop == "Full Version")
			{
				fullVersionSalePage.Open();
			}
			return;
		}
		LanguageSettings.main.Initialize(delegate
		{
			ShowUpgradeVersionMenu(versionText, delegate
			{
				ShowIsNewPlayerMenu(delegate
				{
				});
			});
		});
	}

	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
	private void BuildValidatorHook()
	{
		Utils.ForceCrash(ForcedCrashCategory.AccessViolation);
	}

	private static void ShowIsNewPlayerMenu(Action<bool> callback)
	{
	}

	private static void ShowUpgradeVersionMenu(string versionText, Action callback)
	{
		callback();
	}

	private static string GetVersion()
	{
		return Application.version;
	}

	public void OpenFullVersion()
	{
		fullVersionSalePage.Open();
	}

	private void OpenYoutube()
	{
		AnalyticsUtility.SendEvent("social_media_click", ("platform", "youtube"));
		Application.OpenURL("https://www.youtube.com/channel/UCOpgvpnGyZw4IRT_kuebiWA");
	}

	private void OpenDiscord()
	{
		AnalyticsUtility.SendEvent("social_media_click", ("platform", "discord"));
		Application.OpenURL("https://discordapp.com/invite/hwfWm2d");
	}

	private void OpenForums()
	{
		AnalyticsUtility.SendEvent("social_media_click", ("platform", "forums"));
		Application.OpenURL("https://sfsforum.com/index.php");
	}

	private void OpenTikTok()
	{
		AnalyticsUtility.SendEvent("social_media_click", ("platform", "tiktok"));
		Application.OpenURL("https://www.tiktok.com/@spaceflightsimulator");
	}

	private void OpenInstagram()
	{
		AnalyticsUtility.SendEvent("social_media_click", ("platform", "instagram"));
		Application.OpenURL("https://www.tiktok.com/@spaceflight.sim.official");
	}

	private void OpenVK()
	{
		AnalyticsUtility.SendEvent("social_media_click", ("platform", "vk"));
		Application.OpenURL("https://vk.com/public194508161");
	}

	public void OpenSettings()
	{
		Menu.settings.Open();
	}

	public void OpenCredits()
	{
		Menu.read.Open(() => Loc.main.Credits_Text, CloseMode.Current, background: false);
	}

	public void OpenPC()
	{
		Application.OpenURL("https://spaceflight-simulator-steam.azurewebsites.net/");
	}

	public void OpenSequelLandingPage()
	{
		Application.OpenURL("https://teamcuriosity.com/spaceflightsimulator2/");
	}

	public static void OpenTutorials_Static()
	{
		string link_Orbit = FbRemoteSettings.GetString("Video_Tutorial_Orbit", "https://www.youtube.com/watch?v=5uorANMdB60");
		string link_Moon = FbRemoteSettings.GetString("Video_Tutorial_Moon", "https://www.youtube.com/watch?v=bMv5LmSNgdo");
		string link_Dock = FbRemoteSettings.GetString("Video_Tutorial_Dock", "https://www.youtube.com/watch?v=PkW87qJYEzg");
		SizeSyncerBuilder.Carrier carrier;
		List<MenuElement> list = new List<MenuElement>
		{
			new SizeSyncerBuilder(out carrier).HorizontalMode(SizeMode.MaxChildSize),
			ButtonBuilder.CreateButton(carrier, () => Loc.main.Video_Orbit, delegate
			{
				Application.OpenURL(link_Orbit);
			}, CloseMode.Current).MinSize(300f, 60f),
			ButtonBuilder.CreateButton(carrier, () => Loc.main.Video_Moon, delegate
			{
				Application.OpenURL(link_Moon);
			}, CloseMode.Current).MinSize(300f, 60f),
			ButtonBuilder.CreateButton(carrier, () => Loc.main.Video_Dock, delegate
			{
				Application.OpenURL(link_Dock);
			}, CloseMode.Current).MinSize(300f, 60f)
		};
		ScreenManager.main.OpenScreen(MenuGenerator.CreateMenu(CancelButton.Close, CloseMode.Current, delegate
		{
		}, delegate
		{
		}, list.ToArray()));
	}

	public override void Close()
	{
		MenuGenerator.OpenConfirmation(CloseMode.Current, () => Loc.main.Close_Game, () => Loc.main.Close, Application.Quit);
	}
}
