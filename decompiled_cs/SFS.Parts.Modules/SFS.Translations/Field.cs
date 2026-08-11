using System.Collections.Generic;
using System.Text;
using SFS.Parsers.Regex;

namespace SFS.Translations;

public class Field
{
	public class Builder
	{
		private static readonly StringBuilder stringBuilder = new StringBuilder();

		private string translation;

		public Builder(string translation)
		{
			this.translation = translation;
		}

		public Builder InjectField(Field subs, string variableName, bool formatCapitalization = false)
		{
			SimpleRegex simpleRegex = new SimpleRegex("%" + variableName + "{(?<number>\\d*?)}%");
			while (simpleRegex.Input(translation))
			{
				if (int.TryParse(simpleRegex.GetGroup("number").Value, out var result))
				{
					string sub = subs.GetSub(result);
					translation = translation.Remove(simpleRegex.Match.Index, simpleRegex.Match.Length);
					translation = translation.Insert(simpleRegex.Match.Index, sub);
				}
			}
			return Inject(subs, variableName, formatCapitalization);
		}

		public Builder Inject(string value, string variableName, bool formatCapitalization = false)
		{
			string text = "%" + variableName + "%";
			if (formatCapitalization && !string.IsNullOrEmpty(value) && value.Length > 2)
			{
				value = ((!ShouldCapitalizeWord(translation, text)) ? value.ToLower() : (char.ToUpper(value[0]) + value.Substring(1)));
			}
			translation = translation.Replace(text, value);
			return this;
		}

		public string GetText()
		{
			return translation;
		}

		public static implicit operator string(Builder a)
		{
			return a.translation;
		}

		public static bool ShouldCapitalizeWord(string text, string variableName)
		{
			if (string.IsNullOrEmpty(text) || !text.Contains(variableName))
			{
				return false;
			}
			int num = text.IndexOf(variableName);
			if (num == 0)
			{
				return true;
			}
			if (num >= 2)
			{
				char c = text[num - 1];
				char c2 = text[num - 2];
				if ((c == ' ' && IsSentenceTerminator(c2)) || (c == '\n' && IsSentenceTerminator(c2)) || c == '\n')
				{
					return true;
				}
			}
			if (num > 0 && text[num - 1] == '\n')
			{
				return true;
			}
			return false;
		}
	}

	public Dictionary<int, string> subs = new Dictionary<int, string>();

	public string GetSub(int index)
	{
		if (!subs.ContainsKey(index))
		{
			return subs[0];
		}
		return subs[index];
	}

	public bool HasSub(int index)
	{
		return subs.ContainsKey(index);
	}

	public void SetSub(int index, string sub)
	{
		subs[index] = sub;
	}

	public Builder InjectField(TranslationVariable subs, string variableName, bool formatCapitalization = false)
	{
		return InjectField(subs.Field, variableName, formatCapitalization);
	}

	public Builder InjectField(Field subs, string variableName, bool formatCapitalization = false)
	{
		return GetBuilder(0).InjectField(subs, variableName, formatCapitalization);
	}

	public Builder InjectPlanet(Field subs)
	{
		return GetBuilder(0).InjectField(subs, "planet");
	}

	public Builder Inject(string value, string variableName, bool formatCapitalization = false)
	{
		return GetBuilder(0).Inject(value, variableName, formatCapitalization);
	}

	private Builder GetBuilder(int subIndex)
	{
		return new Builder(GetSub(subIndex));
	}

	public static implicit operator string(Field field)
	{
		if (field == null)
		{
			return string.Empty;
		}
		return field.ToString();
	}

	public override string ToString()
	{
		if (subs.Count == 0)
		{
			return string.Empty;
		}
		return subs[0];
	}

	public static Field Text(string sub)
	{
		return new Field
		{
			subs = { [0] = sub }
		};
	}

	public static Field Subs(params string[] subs)
	{
		Field field = new Field();
		for (int i = 0; i < subs.Length; i++)
		{
			field.subs[i] = subs[i];
		}
		return field;
	}

	public static Field MultilineText(params string[] lines)
	{
		return Text(string.Join("\n", lines));
	}

	private static bool IsSentenceTerminator(char c)
	{
		if (c != '.' && c != '!')
		{
			return c == '?';
		}
		return true;
	}
}
