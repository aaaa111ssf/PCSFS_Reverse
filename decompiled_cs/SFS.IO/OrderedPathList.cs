using System;
using System.Collections.Generic;
using SFS.Parsers.Json;

namespace SFS.IO;

public class OrderedPathList
{
	private const string OrderFileName = "display_order.ordr";

	private IFolder root;

	private List<string> fileNames;

	public OrderedPathList(IFolder root, IStorageObject[] paths)
	{
		this.root = root;
		fileNames = new List<string>();
		if (JsonWrapper.TryLoadJson<List<string>>(root.GetFile("display_order.ordr"), out var data))
		{
			string[] array = data.ToArray();
			foreach (string text in array)
			{
				if (root.GetFile(text).Exists() || root.GetFolder(text).Exists())
				{
					fileNames.Add(text);
				}
			}
		}
		Array.Sort(paths, (IStorageObject a, IStorageObject b) => a.GetLastModifiedTime().CompareTo(b.GetLastModifiedTime()));
		foreach (IStorageObject storageObject in paths)
		{
			if (!fileNames.Contains(storageObject.Name))
			{
				fileNames.Add(storageObject.Name);
			}
		}
	}

	public List<string> GetOrder()
	{
		List<string> list = new List<string>();
		foreach (string fileName in fileNames)
		{
			IFile file = root.GetFile(fileName);
			IFolder folder = root.GetFolder(fileName);
			if (file.Exists())
			{
				list.Add(file.GetNameWithoutExtension());
			}
			else if (folder.Exists())
			{
				list.Add(folder.Name);
			}
		}
		return list;
	}

	public void Rename(string oldName, string newName)
	{
		if (fileNames.Contains(oldName))
		{
			int index = fileNames.IndexOf(oldName);
			fileNames[index] = newName;
			SaveOrder();
		}
	}

	public void Move(string name, int newIndex)
	{
		if (fileNames.Contains(name))
		{
			newIndex = Math.Max(0, newIndex);
			fileNames.Remove(name);
			fileNames.Insert(newIndex, name);
			SaveOrder();
		}
	}

	public void Remove(string name)
	{
		fileNames.RemoveAll((string fileName) => fileName == name);
		SaveOrder();
	}

	private void SaveOrder()
	{
		JsonWrapper.SaveAsJson(root.GetFile("display_order.ordr"), fileNames, pretty: false);
	}
}
