using UnityEngine;

namespace ModLoader;

public class ModLoader : MonoBehaviour
{
	public Loader loader;

	private void Awake()
	{
		Loader.main = loader;
		loader.Initialize_EarlyLoad();
	}

	private void Start()
	{
		loader.Initialize_Load();
	}
}
