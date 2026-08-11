using System.Collections;
using System.IO;
using SFS.UI;
using UnityEngine;
using UnityEngine.Networking;

public class BuildVersionText : MonoBehaviour
{
	public const string GIT_VERSION_FILE_NAME = "version.txt";

	public TextAdapter textAdapter;

	private void Start()
	{
		StartCoroutine(LoadVersionText());
	}

	public void CopyTextToClipboard()
	{
		string systemCopyBuffer = "?";
		if (textAdapter != null)
		{
			systemCopyBuffer = textAdapter.Text;
		}
		GUIUtility.systemCopyBuffer = systemCopyBuffer;
		MsgDrawer.main.Log("Copied version to clipboard");
	}

	public void CopyLogs()
	{
		GUIUtility.systemCopyBuffer = ErrorLogger.main.GetLogsDumpBase64Gzip();
		MsgDrawer.main.Log("Copied gzipped logs dump to clipboard");
	}

	private IEnumerator LoadVersionText()
	{
		string uri = Path.Combine(Application.streamingAssetsPath, "version.txt");
		string gitVersion = string.Empty;
		using (UnityWebRequest www = UnityWebRequest.Get(uri))
		{
			yield return www.SendWebRequest();
			if (www.result == UnityWebRequest.Result.Success)
			{
				gitVersion = www.downloadHandler.text;
			}
		}
		string text = "v" + Application.version + " (b" + INJECTED_BUILD_ID.GetVersion() + " - " + gitVersion + ")";
		if (textAdapter != null)
		{
			textAdapter.Text = text;
		}
	}
}
