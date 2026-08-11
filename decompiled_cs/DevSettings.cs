using UnityEngine;

public class DevSettings : MonoBehaviour
{
	public static readonly string[] DisableParts = new string[3] { "Test", "New Build", "Crew_New" };

	public static bool DisableAstronauts => true;

	public static bool DisableNewBuild => true;

	public static bool FullVersion => true;

	public static bool ShowFullVersionButton => false;
}
