using SFS.UI;
using TMPro;
using UnityEngine;

public class SaveLocationSettings : MonoBehaviour
{
	public GameObject saveLocationHolder;

	public TMP_Text currentLocationText;

	public TMP_Text setLocationButtonText;

	public Button openSaveLocationButton;

	private void Start()
	{
		saveLocationHolder.SetActive(Application.platform == RuntimePlatform.Android);
	}
}
