using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SFS.IO;

public class FilePath : BasePath
{
	private static char[] invalidCharacters = new char[9] { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };

	public string FileName => System.IO.Path.GetFileName(base.Path);

	public string CleanFileName => System.IO.Path.GetFileNameWithoutExtension(base.Path);

	public string Extension
	{
		get
		{
			string fileName = FileName;
			if (!fileName.Contains("."))
			{
				return null;
			}
			return fileName.Split('.').Last();
		}
	}

	public FilePath(string initialLocation)
		: base(initialLocation)
	{
	}

	public void WriteText(string text)
	{
		Write(delegate
		{
			File.WriteAllText(this, text);
		});
	}

	public void WriteBytes(byte[] data)
	{
		Write(delegate
		{
			File.WriteAllBytes(this, data);
		});
	}

	public void AppendText(string text)
	{
		if (!FileExists())
		{
			WriteText("");
		}
		File.AppendAllText(this, text);
	}

	public byte[] ReadBytes()
	{
		return File.ReadAllBytes(this);
	}

	public string ReadText()
	{
		if (!FileExists())
		{
			throw new Exception("File " + base.Path + " does not exist!");
		}
		return File.ReadAllText(this);
	}

	public Task<string> ReadTextAsync()
	{
		return File.ReadAllTextAsync(this);
	}

	public void DeleteFile()
	{
		if (FileExists())
		{
			File.Delete(this);
		}
	}

	public FolderPath GetParent()
	{
		return new FolderPath(GetParentPath());
	}

	public bool FileExists()
	{
		return File.Exists(this);
	}

	public void Move(FilePath path)
	{
		File.Copy(this, path, overwrite: true);
		DeleteFile();
	}

	public void Copy(FilePath path)
	{
		File.Copy(this, path, overwrite: true);
	}

	public static string CleanupName(string fileName)
	{
		char[] array = invalidCharacters;
		foreach (char oldChar in array)
		{
			fileName = fileName.Replace(oldChar, '_');
		}
		array = System.IO.Path.GetInvalidPathChars();
		foreach (char oldChar2 in array)
		{
			fileName = fileName.Replace(oldChar2, '_');
		}
		array = System.IO.Path.GetInvalidFileNameChars();
		foreach (char oldChar3 in array)
		{
			fileName = fileName.Replace(oldChar3, '_');
		}
		return fileName;
	}

	public string GetRelativePath(string root)
	{
		return base.Path.Replace(System.IO.Path.GetFullPath(root).Replace("\\", "/"), "");
	}

	private void Write(Action writeAction)
	{
		if (base.Path != null)
		{
			writeAction();
		}
	}
}
