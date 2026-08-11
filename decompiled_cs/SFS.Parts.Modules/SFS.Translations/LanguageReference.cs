using System;

namespace SFS.Translations;

[Serializable]
public class LanguageReference
{
	public string codeName;

	public bool custom;

	[NonSerialized]
	public string displayName;

	public LanguageReference()
	{
	}

	public LanguageReference(string codeName, string displayName, bool custom = false)
	{
		this.codeName = codeName;
		this.custom = custom;
		this.displayName = displayName;
	}
}
