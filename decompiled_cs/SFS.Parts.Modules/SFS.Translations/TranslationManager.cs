using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using SFS.UI;
using UnityEngine;

namespace SFS.Translations;

public class TranslationManager : MonoBehaviour
{
	public const string TRANSLATION_FOLDER = "Translations";

	public const string TRANSLATION_FILE_EXTENSION = ".txt";

	public const string EXAMPLE_FILE_NAME = "Example.txt";

	public const string README_FILE_NAME = "READ_ME.txt";

	public static TranslationManager main;

	public List<Font> fonts = new List<Font>();

	[NonSerialized]
	public SFS_Translation currentTranslation;

	[NonSerialized]
	public Font currentFont;

	public static string GetFilePath(string fileNameNoExtension)
	{
		return Path.Combine(Application.dataPath, "Resources", "Translations", fileNameNoExtension + ".txt");
	}

	public static string LoadTextAsset(string fileNameNoExtension)
	{
		return Resources.Load<TextAsset>(Path.Combine("Translations", fileNameNoExtension))?.text;
	}

	private void Awake()
	{
		main = this;
		currentFont = fonts[0];
	}

	private void Start()
	{
		IFile examplePath = FileLocations.PublicTranslationFolder.GetFile("Example.txt");
		new Thread((ParameterizedThreadStart)delegate
		{
			examplePath.WriteText(TranslationSerialization.Serialize(TranslationSerialization.CreateTranslation<SFS_Translation>()));
		}).Start();
		if (FileLocations.TranslationCacheFolder.Exists())
		{
			FileLocations.TranslationCacheFolder.Delete();
		}
	}

	public void SetLanguage(SFS_Translation newLanguage)
	{
		currentTranslation = newLanguage;
		string font = newLanguage.Font.subs[0];
		currentFont = fonts.FirstOrDefault((Font a) => a.name == font) ?? fonts[0];
	}

	public static string GetDisplayName(LanguageReference a)
	{
		LanguageReference[] availableLanguages = GetAvailableLanguages();
		foreach (LanguageReference languageReference in availableLanguages)
		{
			if (languageReference != null && a.codeName == languageReference.codeName && a.custom == languageReference.custom)
			{
				return languageReference.displayName;
			}
		}
		string codeName = a.codeName;
		if (codeName == null || codeName == "Default")
		{
			return "English";
		}
		return "-";
	}

	public static List<LanguageReference> GetOfficialLanguages()
	{
		return new List<LanguageReference>
		{
			new LanguageReference("English", "English"),
			new LanguageReference("Brazilian", "Português (Brasil)"),
			new LanguageReference("Chinese", "简体中文"),
			new LanguageReference("Dutch", "Nederlands"),
			new LanguageReference("French", "Français"),
			new LanguageReference("German", "Deutsch"),
			new LanguageReference("Hindi", "ह\u093fन\u094dद\u0940"),
			new LanguageReference("Indonesian", "Bahasa Indonesia"),
			new LanguageReference("Italian", "Italiano"),
			new LanguageReference("Japanese", "日本語"),
			new LanguageReference("Korean", "한국어"),
			new LanguageReference("Malay", "Bahasa Melayu"),
			new LanguageReference("Polish", "Polski"),
			new LanguageReference("Russian", "Русский"),
			new LanguageReference("Spanish", "Español"),
			new LanguageReference("Turkish", "Türkçe"),
			new LanguageReference("Czech", "Čeština"),
			new LanguageReference("Hungarian", "Magyar"),
			new LanguageReference("Romanian", "Română"),
			new LanguageReference("Ukrainian", "Українська"),
			new LanguageReference("Greek", "Ελληνικά")
		};
	}

	public static LanguageReference[] GetAvailableLanguages()
	{
		List<LanguageReference> officialLanguages = GetOfficialLanguages();
		IFile[] array = FileLocations.PublicTranslationFolder.GetFiles().ToArray();
		if (array.Length != 0)
		{
			officialLanguages.Add(null);
		}
		IFile[] array2 = array;
		foreach (IFile file in array2)
		{
			if (file.GetExtension() == "txt" && file.Name != "Example.txt" && file.Name != "READ_ME.txt")
			{
				officialLanguages.Add(new LanguageReference(file.GetNameWithoutExtension(), file.GetNameWithoutExtension() + " (Custom)", custom: true));
			}
		}
		return officialLanguages.ToArray();
	}

	public static bool LoadAndSetLanguage(LanguageReference language)
	{
		var (text, language2) = LoadLanguage(language, delegate(string missingLog)
		{
			if (Application.isEditor || language.custom)
			{
				Menu.read.ShowReport(missingLog, null);
			}
		});
		if (language.codeName != text)
		{
			return false;
		}
		main.SetLanguage(language2);
		Loc.SetLanguage(text, language2);
		return true;
	}

	public static (string codeName, SFS_Translation translation) LoadLanguage(LanguageReference language, Action<string> log)
	{
		try
		{
			if (string.IsNullOrEmpty(language.codeName))
			{
				return DefaultEnglish();
			}
			if (language.custom)
			{
				IFile file = FileLocations.PublicTranslationFolder.GetFile(language.codeName + ".txt");
				if (file.Exists())
				{
					return Deserialize(file.ReadText());
				}
				return DefaultEnglish();
			}
			string text = LoadTextAsset(language.codeName);
			if (text == null)
			{
				Menu.read.Open(() => "Language " + language.displayName + " not found");
				if (language.codeName == "English")
				{
					return (codeName: "English", translation: new SFS_Translation());
				}
				return DefaultEnglish();
			}
			return Deserialize(text);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return DefaultEnglish();
		}
		static (string, SFS_Translation) DefaultEnglish()
		{
			return LoadLanguage(new LanguageReference("English", "English"), null);
		}
		(string, SFS_Translation) Deserialize(string iniText)
		{
			List<FieldReference> unused;
			List<FieldReference> missing;
			List<FieldReference> changed;
			SFS_Translation item = TranslationSerialization.Deserialize(iniText, out unused, out missing, out changed);
			ShowDeserializationReport(missing, unused, log);
			return (language.codeName, item);
		}
	}

	private static void ShowDeserializationReport(List<FieldReference> missing, List<FieldReference> unused, Action<string> log)
	{
		if (unused.Count + missing.Count == 0)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Your custom translation is out of date");
		stringBuilder.AppendLine("(English will be used for missing translations)");
		stringBuilder.AppendLine("");
		if (missing.Count > 0)
		{
			stringBuilder.AppendLine("");
			stringBuilder.AppendLine("Missing:");
			foreach (FieldReference item in missing)
			{
				stringBuilder.AppendLine(item.group + " / " + item.name);
			}
		}
		if (unused.Count > 0)
		{
			stringBuilder.AppendLine("");
			stringBuilder.AppendLine("Unnecessary:");
			foreach (FieldReference item2 in unused)
			{
				stringBuilder.AppendLine(item2.group + " / " + item2.name);
			}
		}
		log?.Invoke(stringBuilder.ToString());
	}
}
