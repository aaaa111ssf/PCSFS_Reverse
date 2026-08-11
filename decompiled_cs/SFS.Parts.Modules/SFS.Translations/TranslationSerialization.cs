using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SFS.Parsers.Ini;
using UnityEngine;

namespace SFS.Translations;

public static class TranslationSerialization
{
	public static string Serialize<T>(T translation)
	{
		List<(PropertyInfo, Group)> fieldReferences = GetFieldReferences<T>();
		foreach (var (propertyInfo, obj) in fieldReferences)
		{
			if (obj.subExports.Count == 0)
			{
				continue;
			}
			Field field = propertyInfo.GetValue(translation) as Field;
			foreach (Func<string, string> subExport in obj.subExports)
			{
				field?.SetSub(field.subs.Count, subExport(field));
			}
		}
		IniConverter iniConverter = new IniConverter();
		foreach (var (propertyInfo2, obj2) in fieldReferences)
		{
			if (propertyInfo2.GetCustomAttribute<Unexported>() != null)
			{
				continue;
			}
			FieldReference fieldReference = new FieldReference(propertyInfo2.Name, obj2.Name);
			Field obj3 = propertyInfo2.GetValue(translation) as Field;
			bool flag = obj3.subs.Count > 1 || obj2.hasSubs || propertyInfo2.GetCustomAttributes<MarkAsSub>().Any();
			bool flag2 = true;
			foreach (KeyValuePair<int, string> sub in obj3.subs)
			{
				IniDataSection section = iniConverter.GetSection(fieldReference.group);
				IniDataEnv.Value value = new IniDataEnv.Value(sub.Value);
				section[flag ? (fieldReference.name + "{" + sub.Key + "}") : fieldReference.name] = value;
				LocSpace customAttribute = propertyInfo2.GetCustomAttribute<LocSpace>();
				Documentation customAttribute2 = propertyInfo2.GetCustomAttribute<Documentation>();
				if (!flag2)
				{
					continue;
				}
				flag2 = false;
				if (customAttribute2 != null)
				{
					if (customAttribute2.attachToGroup)
					{
						section.comment = customAttribute2.comment;
					}
					else if (customAttribute2.afterLine)
					{
						value.aftLineComment = customAttribute2.comment;
					}
					else
					{
						value.preLineComment = customAttribute2.comment;
					}
				}
				if (customAttribute != null)
				{
					if (customAttribute.attachToGroup)
					{
						section.whitespacesBefore = customAttribute.amount;
					}
					else
					{
						value.whitespacesBefore = customAttribute.amount;
					}
				}
			}
		}
		return iniConverter.Serialize();
	}

	public static SFS_Translation Deserialize(string iniText, out List<FieldReference> unused, out List<FieldReference> missing, out List<FieldReference> changed)
	{
		Dictionary<FieldReference, Field> dictionary = ParseTranslations(iniText);
		SFS_Translation sFS_Translation = new SFS_Translation();
		unused = new List<FieldReference>(dictionary.Keys);
		missing = new List<FieldReference>();
		changed = new List<FieldReference>();
		foreach (var fieldReference2 in GetFieldReferences<SFS_Translation>())
		{
			PropertyInfo item = fieldReference2.Item1;
			Group item2 = fieldReference2.Item2;
			FieldReference fieldReference = new FieldReference(item.Name, item2.Name);
			Unexported customAttribute = item.GetCustomAttribute<Unexported>();
			if (!dictionary.ContainsKey(fieldReference))
			{
				if (customAttribute == null)
				{
					missing.Add(fieldReference);
				}
				continue;
			}
			unused.Remove(fieldReference);
			Field field = dictionary[fieldReference];
			Field field2 = item.GetMethod.Invoke(sFS_Translation, new object[0]) as Field;
			foreach (KeyValuePair<int, string> sub in field2.subs)
			{
				if (!field.HasSub(sub.Key) || sub.Value != field2.GetSub(sub.Key))
				{
					changed.Add(fieldReference);
				}
			}
			field2.subs = field.subs;
		}
		return sFS_Translation;
	}

	private static Dictionary<FieldReference, Field> ParseTranslations(string iniText)
	{
		IniConverter iniConverter = new IniConverter(iniText);
		Dictionary<FieldReference, Field> dictionary = new Dictionary<FieldReference, Field>();
		string[] sectionNames = iniConverter.GetSectionNames();
		foreach (string text in sectionNames)
		{
			IniDataSection section = iniConverter.GetSection(text);
			string text2 = text;
			foreach (KeyValuePair<string, IniDataEnv.Value> datum in section.data)
			{
				string text3 = datum.Key;
				int result = 0;
				int num = text3.LastIndexOf('{');
				if (num >= 0)
				{
					if (text3[text3.Length - 1] != '}')
					{
						Debug.LogError("Wrong Translation key syntax:" + text3);
						continue;
					}
					if (int.TryParse(text3.Substring(num + 1, text3.Length - num - 2), out result))
					{
						text3 = text3.Substring(0, num);
					}
				}
				FieldReference key = new FieldReference(text3, text2);
				Field field = (dictionary.ContainsKey(key) ? dictionary[key] : (dictionary[key] = new Field()));
				field.SetSub(result, datum.Value.value);
			}
		}
		return dictionary;
	}

	public static T CreateTranslation<T>() where T : new()
	{
		return new T();
	}

	public static SFS_Translation CreateTranslation()
	{
		SFS_Translation sFS_Translation = new SFS_Translation();
		foreach (var fieldReference in GetFieldReferences<SFS_Translation>())
		{
			PropertyInfo item = fieldReference.Item1;
			if (item.GetValue(sFS_Translation) is Field value)
			{
				sFS_Translation.fields[item.Name] = value;
			}
		}
		return sFS_Translation;
	}

	public static List<(PropertyInfo, Group)> GetFieldReferences<T>()
	{
		List<(PropertyInfo, Group)> list = new List<(PropertyInfo, Group)>();
		Group item = new Group("None");
		PropertyInfo[] properties = typeof(T).GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			try
			{
				if (propertyInfo.PropertyType == typeof(Field))
				{
					if (propertyInfo.GetCustomAttribute<Group>() != null)
					{
						item = propertyInfo.GetCustomAttribute<Group>();
					}
					list.Add((propertyInfo, item));
				}
			}
			catch (Exception ex)
			{
				Debug.Log("Lang prop error: " + propertyInfo.Name + "\n" + ex);
			}
		}
		return list;
	}
}
