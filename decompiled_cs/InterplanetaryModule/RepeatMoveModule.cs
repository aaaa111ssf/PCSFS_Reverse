using SFS.Parts.Modules;
using UnityEngine;

namespace InterplanetaryModule;

public class RepeatMoveModule : MonoBehaviour
{
	public bool activated;

	public float time;

	public float duration;

	public bool unscaledTime;

	public EngineModule engineModule;

	public MoveModule moveModule;

	private void Start()
	{
		moveModule.Activate();
	}

	private void Update()
	{
		if (!activated)
		{
			return;
		}
		float num = (unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
		if (num == 0f)
		{
			return;
		}
		float maxDelta = ((duration > 0f) ? (num / duration) : 10000f);
		bool num2 = (bool)engineModule || (engineModule.engineOn.Value && engineModule.throttle_Out.Value > 0f);
		bool flag = moveModule.time.Value >= 1f;
		if (num2)
		{
			time = Mathf.MoveTowards(time, 1f, maxDelta);
			if (time >= 1f)
			{
				time = 0f;
			}
			moveModule.enabled = true;
			moveModule.time.Value = time;
			moveModule.targetTime.Value = time;
		}
		else if (!flag)
		{
			time = Mathf.MoveTowards(time, 1f, maxDelta);
		}
		else
		{
			moveModule.enabled = false;
		}
	}
}
