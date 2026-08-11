using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFS.Parsers.Ini;

public class IniConverter
{
	private ref struct StringReader(string input, int pos = 0)
	{
		public readonly string input = input;

		public int pos = pos;

		public bool IsAtEnd()
		{
			return pos >= input.Length;
		}

		public char Read()
		{
			return input[pos++];
		}

		public void SetPos(int length)
		{
			length = Math.Min(input.Length - pos, length);
			pos += length;
		}

		public string Read(int length)
		{
			length = Math.Min(input.Length - pos, length);
			string result = input.Substring(pos, length);
			pos += length;
			return result;
		}

		public char Peek()
		{
			return input[pos];
		}

		public string Peek(int length)
		{
			return input.Substring(pos, Math.Min(input.Length - pos, length));
		}

		public bool PeekEquals(int length, string compare)
		{
			if (pos + length >= input.Length)
			{
				return false;
			}
			for (int i = 0; i < length; i++)
			{
				if (input[pos + i] != compare[i])
				{
					return false;
				}
			}
			return true;
		}

		public StringReader Split()
		{
			StringReader result = new StringReader(input);
			result.pos = pos;
			return result;
		}

		public void Merge(StringReader reader)
		{
			pos = reader.pos;
		}
	}

	public IniDataEnv data = new IniDataEnv();

	private static string[] commentPrefixes = new string[2] { "//", "#" };

	public IniConverter()
	{
		data = new IniDataEnv();
	}

	public IniConverter(string iniText)
	{
		LoadIni(iniText);
	}

	public IniDataSection GetSection(string section)
	{
		if (!data.sections.ContainsKey(section))
		{
			data.sections[section] = new IniDataSection(section);
		}
		return data.sections[section];
	}

	public void LoadIni(string iniText)
	{
		string[] array = iniText.Split(new string[2] { "\n", "\r\n" }, StringSplitOptions.None);
		IniDataSection iniDataSection = data.Global;
		int num = 0;
		StringBuilder stringBuilder = new StringBuilder(30);
		StringBuilder valueBuilder = new StringBuilder(100);
		StringBuilder commentBuilder = new StringBuilder(100);
		StringBuilder keyNameBuilder = new StringBuilder(30);
		StringBuilder sectionNameBuilder = new StringBuilder(30);
		foreach (string obj in array)
		{
			string input = obj.Trim();
			if (string.IsNullOrWhiteSpace(obj))
			{
				num++;
				continue;
			}
			StringReader reader = new StringReader(input);
			string sectionName;
			if (ReadComment(ref reader, out var comment))
			{
				stringBuilder.AppendLine(comment);
			}
			else if (ReadSection(ref reader, out sectionName))
			{
				iniDataSection = data.GetSection(sectionName);
				iniDataSection.whitespacesBefore = num;
				num = 0;
				if (stringBuilder.Length > 0)
				{
					iniDataSection.comment = stringBuilder.ToString();
				}
				stringBuilder.Clear();
			}
			else
			{
				if (!ReadKey(ref reader, out var keyName))
				{
					continue;
				}
				ReadValue(ref reader, out var value, out var aftComment);
				if (iniDataSection.data.ContainsKey(keyName))
				{
					IniDataEnv.Value value2 = iniDataSection[keyName];
					value2.value = value2.value + "\n" + value;
					continue;
				}
				IniDataEnv.Value value3 = new IniDataEnv.Value(value);
				value3.aftLineComment = aftComment;
				value3.whitespacesBefore = num;
				num = 0;
				if (stringBuilder.Length > 0)
				{
					value3.preLineComment = stringBuilder.ToString();
				}
				stringBuilder.Clear();
				iniDataSection[keyName] = value3;
			}
		}
		bool ReadComment(ref StringReader reference, out string reference2)
		{
			string[] array2 = commentPrefixes;
			foreach (string text in array2)
			{
				if (reference.PeekEquals(text.Length, text))
				{
					reference.SetPos(text.Length);
					while (!reference.IsAtEnd())
					{
						commentBuilder.Append(reference.Read());
					}
					reference2 = commentBuilder.ToString();
					commentBuilder.Clear();
					return true;
				}
			}
			reference2 = null;
			return false;
		}
		bool ReadKey(ref StringReader reference, out string reference2)
		{
			StringReader reader2 = reference.Split();
			while (!reader2.IsAtEnd())
			{
				if (reader2.Peek() == '=')
				{
					reader2.Read();
					reference.Merge(reader2);
					reference2 = keyNameBuilder.ToString();
					keyNameBuilder.Clear();
					return true;
				}
				keyNameBuilder.Append(reader2.Read());
			}
			keyNameBuilder.Clear();
			reference2 = null;
			return false;
		}
		bool ReadSection(ref StringReader reference, out string reference2)
		{
			if (reference.Peek() == '[')
			{
				reference.Read();
				while (reference.Peek() != ']')
				{
					sectionNameBuilder.Append(reference.Read());
				}
				reference2 = sectionNameBuilder.ToString();
				sectionNameBuilder.Clear();
				return true;
			}
			reference2 = null;
			return false;
		}
		bool ReadValue(ref StringReader reference2, out string reference3, out string reference)
		{
			reference = null;
			while (!reference2.IsAtEnd() && !ReadComment(ref reference2, out reference))
			{
				valueBuilder.Append(reference2.Read());
			}
			reference3 = valueBuilder.ToString();
			valueBuilder.Clear();
			return reference3.Length > 0;
		}
	}

	public string Serialize()
	{
		StringBuilder iniTextBuilder = new StringBuilder();
		int whitelines = 0;
		data.sections.ForEach(delegate(KeyValuePair<string, IniDataSection> section)
		{
			AppendSection(section.Value);
		});
		return iniTextBuilder.ToString();
		void Append(string txt, bool clearWhitelines)
		{
			whitelines = ((!clearWhitelines) ? whitelines : 0);
			iniTextBuilder.Append(txt);
		}
		void AppendAftComment(string aftComment)
		{
			if (aftComment != null)
			{
				AppendComment(aftComment, canUseWhiteline: false);
			}
			else
			{
				AppendLine("", clearWhitelines: true);
			}
		}
		void AppendComment(string comment, bool canUseWhiteline)
		{
			if (comment != null)
			{
				if (whitelines == 0 && canUseWhiteline)
				{
					EnsureWhitelines(1);
				}
				string[] array = comment.Split(new string[2] { "\n", "\r\n" }, StringSplitOptions.None);
				foreach (string text in array)
				{
					AppendLine("# " + text, clearWhitelines: false);
				}
			}
		}
		void AppendDataLine(string keyName, IniDataEnv.Value value, bool canUseInitialWhiteline)
		{
			if (value.whitespacesBefore != 0 && canUseInitialWhiteline)
			{
				EnsureWhitelines(value.whitespacesBefore);
			}
			AppendComment(value.preLineComment, canUseInitialWhiteline);
			if (value.value.Contains("\n"))
			{
				EnsureWhitelines(1);
				string[] array = value.value.Split('\n');
				for (int i = 0; i < array.Length; i++)
				{
					AppendDataValue(array[i] + ((i < array.Length - 1) ? "\n" : ""));
				}
				AppendAftComment(value.aftLineComment);
				AppendWhiteLine();
			}
			else
			{
				AppendDataValue(value.value);
				AppendAftComment(value.aftLineComment);
			}
		}
		void AppendDataValue(string val)
		{
			Append("    " + P_1.keyName + "=" + val, clearWhitelines: true);
		}
		void AppendLine(string txt, bool clearWhitelines)
		{
			Append(txt + "\n", clearWhitelines);
		}
		void AppendSection(IniDataSection section)
		{
			AppendComment(section.comment, canUseWhiteline: true);
			if (iniTextBuilder.Length > 0)
			{
				EnsureWhitelines(1);
			}
			AppendLine("[" + section.name + "]", clearWhitelines: true);
			KeyValuePair<string, IniDataEnv.Value>[] array = section.data.ToArray();
			for (int i = 0; i < section.data.Count; i++)
			{
				KeyValuePair<string, IniDataEnv.Value> keyValuePair = array[i];
				AppendDataLine(keyValuePair.Key, keyValuePair.Value, i > 0);
			}
		}
		void AppendWhiteLine()
		{
			whitelines++;
			iniTextBuilder.AppendLine();
		}
		void EnsureWhitelines(int amount)
		{
			int num = Math.Max(0, amount - whitelines);
			for (int i = 0; i < num; i++)
			{
				AppendWhiteLine();
			}
		}
	}

	public string[] GetSectionNames()
	{
		return new List<IniDataSection>(data.sections.Values).ConvertAll((IniDataSection data) => data.name).ToArray();
	}
}
