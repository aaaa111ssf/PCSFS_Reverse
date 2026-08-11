using SFS.Analytics;
using UnityEngine;

public class CommunityPanel : MonoBehaviour
{
	public void OpenYoutube()
	{
		AnalyticsUtility.SendEvent("social_media_click", ("platform", "youtube"));
		Application.OpenURL("https://www.youtube.com/channel/UCOpgvpnGyZw4IRT_kuebiWA");
	}

	public void OpenDiscord()
	{
		AnalyticsUtility.SendEvent("social_media_click", ("platform", "discord"));
		Application.OpenURL("https://discordapp.com/invite/hwfWm2d");
	}

	public void OpenReddit()
	{
		AnalyticsUtility.SendEvent("social_media_click", ("platform", "reddit"));
		Application.OpenURL("https://www.reddit.com/r/SpaceflightSimulator/");
	}
}
