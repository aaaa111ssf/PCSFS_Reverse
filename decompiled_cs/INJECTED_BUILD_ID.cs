public static class INJECTED_BUILD_ID
{
	private static readonly string version = "597";

	public static string GetVersion()
	{
		if (version.Contains("CAPTAIN_BUILD_ID"))
		{
			return "?";
		}
		return version;
	}
}
