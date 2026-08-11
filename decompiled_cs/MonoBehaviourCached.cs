using UnityEngine;

public class MonoBehaviourCached : MonoBehaviour
{
	private Transform _transform;

	private GameObject _gameObject;

	public new Transform transform
	{
		get
		{
			if ((object)_transform == null)
			{
				_transform = base.transform;
			}
			return _transform;
		}
	}

	public new GameObject gameObject
	{
		get
		{
			if ((object)_gameObject == null)
			{
				_gameObject = base.gameObject;
			}
			return _gameObject;
		}
	}
}
