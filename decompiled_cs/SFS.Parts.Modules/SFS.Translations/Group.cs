using System;
using System.Collections.Generic;

namespace SFS.Translations;

public class Group : Attribute
{
	public bool hasSubs;

	public List<Func<string, string>> subExports = new List<Func<string, string>>();

	public string Name { get; }

	public Group(string name)
	{
		Name = name;
	}
}
