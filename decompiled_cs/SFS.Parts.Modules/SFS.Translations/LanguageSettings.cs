using System;
using System.Collections.Generic;
using System.Linq;
using SFS.IO;
using SFS.Input;
using SFS.UI;
using SFS.Variables;
using TMPro;
using UnityEngine;

namespace SFS.Translations;

public class LanguageSettings : SettingsBase<LanguageReference>
{
	public static LanguageSettings main;

	public TextAdapter languageName;

	public GameObject settingsManager;

	public Event_Local onChange = new Event_Local();

	public TMP_Dropdown languageDropdown;

	private bool isInitialized;

	protected override string FileName => "LanguageSettings_2";

	private void Awake()
	{
		main = this;
	}

	private void Start()
	{
		if (Application.isEditor)
		{
			Initialize(onChange.Invoke);
		}
		languageDropdown.onValueChanged.AddListener(LanguageDropdownChanged);
	}

	public void Initialize(Action callback)
	{
		if (isInitialized)
		{
			callback();
			return;
		}
		isInitialized = true;
		Load();
		List<string> list = (from language in TranslationManager.GetAvailableLanguages()
			where language != null
			select language.displayName).ToList();
		languageDropdown.ClearOptions();
		languageDropdown.AddOptions(list);
		languageDropdown.value = list.IndexOf(TranslationManager.GetDisplayName(settings));
		callback();
	}

	private void SelectLanguage(LanguageReference settings, Action callback)
	{
		if (TranslationManager.LoadAndSetLanguage(settings))
		{
			base.settings = settings;
			Save();
			callback();
			if (languageName != null)
			{
				languageName.Text = TranslationManager.GetDisplayName(base.settings);
			}
		}
	}

	protected override void OnLoad()
	{
		TranslationManager.LoadAndSetLanguage(settings);
		if (languageName != null)
		{
			languageName.Text = TranslationManager.GetDisplayName(settings);
		}
	}

	public void OpenLanguageSelector()
	{
		OpenLanguageSelector(onChange.Invoke);
	}

	private void OpenLanguageSelector(Action callback)
	{
		SizeSyncerBuilder.Carrier carrier;
		List<MenuElement> list = new List<MenuElement> { new SizeSyncerBuilder(out carrier).HorizontalMode(SizeMode.MaxChildSize) };
		LanguageReference[] availableLanguages = TranslationManager.GetAvailableLanguages();
		foreach (LanguageReference languageReference in availableLanguages)
		{
			if (languageReference != null)
			{
				LanguageReference copy = languageReference;
				string buttonText = copy.displayName;
				list.Add(ButtonBuilder.CreateButton(carrier, () => buttonText, delegate
				{
					ScreenManager.main.CloseCurrent();
					SelectLanguage(copy, callback);
				}, CloseMode.None).MinSize(300f, 60f));
			}
		}
		ScreenManager.main.OpenScreen(MenuGenerator.CreateMenu(CancelButton.None, CloseMode.None, delegate
		{
		}, delegate
		{
		}, list.ToArray()));
	}

	private void LanguageDropdownChanged(int index)
	{
		SelectLanguage(TranslationManager.GetAvailableLanguages()[index], delegate
		{
		});
		languageDropdown.Hide();
	}
}
