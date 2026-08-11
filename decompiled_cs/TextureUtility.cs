using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class TextureUtility
{
	public static async Task<Texture2D> GetRemoteTexture(string url)
	{
		if (string.IsNullOrEmpty(url))
		{
			return null;
		}
		UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
		await www.SendWebRequest();
		return (www.result != UnityWebRequest.Result.Success) ? null : DownloadHandlerTexture.GetContent(www);
	}

	public static void SaveToFile(this Texture2D tex, IFile file)
	{
		bool flag = false;
		if (!tex.isReadable)
		{
			flag = true;
			tex = tex.GetReadableCopy();
		}
		file.WriteBytes(tex.EncodeToPNG());
		if (flag)
		{
			Object.Destroy(tex);
		}
	}

	public static Texture2D GetReadableCopy(this Texture2D source)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
		Graphics.Blit(source, temporary);
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = temporary;
		Texture2D texture2D = new Texture2D(source.width, source.height);
		texture2D.ReadPixels(new Rect(0f, 0f, temporary.width, temporary.height), 0, 0);
		texture2D.Apply();
		RenderTexture.active = active;
		RenderTexture.ReleaseTemporary(temporary);
		return texture2D;
	}

	public static Texture2D FromFile(IFile file, bool returnPlaceholderIfNull = true)
	{
		if (!file.Exists())
		{
			throw new FileNotFoundException("Can't find " + file.Name + " texture");
		}
		Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, 1, linear: true);
		if (!texture2D.LoadImage(file.ReadBytes()))
		{
			if (!returnPlaceholderIfNull)
			{
				return null;
			}
			return Texture2D.whiteTexture;
		}
		texture2D.Apply();
		return texture2D;
	}
}
