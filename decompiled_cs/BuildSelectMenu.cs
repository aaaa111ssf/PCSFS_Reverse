using System;
using System.Collections;
using SFS.Builds;
using SFS.UI;
using UnityEngine;

public class BuildSelectMenu : MonoBehaviour
{
	public ScrollElement scroller;

	public GameObject container;

	private void Awake()
	{
		Button[] componentsInChildren = GetComponentsInChildren<Button>(includeInactive: true);
		foreach (Button a in componentsInChildren)
		{
			scroller.RegisterScrolling(a);
		}
		SkinMenu[] componentsInChildren2 = GetComponentsInChildren<SkinMenu>(includeInactive: true);
		foreach (SkinMenu obj in componentsInChildren2)
		{
			obj.OnButtonCreated += delegate(object sender, Button e)
			{
				scroller.RegisterScrolling(e);
			};
			obj.expanded.OnChange += (Action<bool>)delegate
			{
				scroller.PercentPosition = Vector2.zero;
				StartCoroutine(ResetScrollPosition());
			};
		}
	}

	public void Toggle(bool show)
	{
		container.SetActive(show);
		scroller.PercentPosition = Vector2.zero;
		StartCoroutine(ResetScrollPosition());
	}

	private IEnumerator ResetScrollPosition()
	{
		yield return new WaitForEndOfFrame();
		scroller.PercentPosition = Vector2.zero;
		yield return null;
		scroller.PercentPosition = Vector2.zero;
	}
}
