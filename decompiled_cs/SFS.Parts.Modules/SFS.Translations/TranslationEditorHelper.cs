using System.Collections.Generic;
using System.IO;

namespace SFS.Translations;

public static class TranslationEditorHelper
{
	public static void SaveToFile(SFS_Translation translation, string codeName)
	{
		string contents = TranslationSerialization.Serialize(translation);
		File.WriteAllText(TranslationManager.GetFilePath(codeName), contents);
	}

	public static void UpdateIdsForLanguage(string codeName)
	{
		string text = TranslationManager.LoadTextAsset(codeName);
		List<FieldReference> unused;
		List<FieldReference> missing;
		List<FieldReference> changed;
		SFS_Translation translation = ((text != null) ? TranslationSerialization.Deserialize(text, out unused, out missing, out changed) : TranslationSerialization.CreateTranslation());
		SaveToFile(translation, codeName);
	}
}
