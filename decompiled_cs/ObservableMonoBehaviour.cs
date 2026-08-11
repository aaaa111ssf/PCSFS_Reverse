using System;
using SFS.Variables;

public class ObservableMonoBehaviour : MonoBehaviourCached, I_ObservableMonoBehaviour
{
	private Action onDestroyDelegate;

	Action I_ObservableMonoBehaviour.OnDestroy
	{
		get
		{
			return onDestroyDelegate;
		}
		set
		{
			onDestroyDelegate = value;
		}
	}

	protected void OnDestroy()
	{
		onDestroyDelegate?.Invoke();
	}
}
