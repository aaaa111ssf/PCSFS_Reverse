using SFS.Variables;
using UnityEngine;

namespace InterplanetaryModule;

public class GravityRingModule : MonoBehaviour
{
	public float targetAngularSpeed = 18f;

	public float accelerationTime = 2f;

	private float currentAngularSpeed;

	private float angularVelocity;

	private bool isRotating;

	private bool stopping;

	private float rotationY;

	public Bool_Reference counterclockwise;

	public void Toggle()
	{
		if (!isRotating && !stopping)
		{
			isRotating = true;
			stopping = false;
		}
		else if (isRotating)
		{
			isRotating = false;
			stopping = true;
		}
	}

	private void Update()
	{
		float target = 0f;
		if (isRotating)
		{
			target = targetAngularSpeed;
		}
		else if (stopping)
		{
			float num = Mathf.Repeat(rotationY, 360f);
			if (num < 1f || num > 359f)
			{
				currentAngularSpeed = 0f;
				stopping = false;
				rotationY = 0f;
				base.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
				return;
			}
			target = targetAngularSpeed * Mathf.Clamp01(num / 180f);
		}
		currentAngularSpeed = Mathf.SmoothDamp(currentAngularSpeed, target, ref angularVelocity, accelerationTime);
		float num2 = currentAngularSpeed * Time.deltaTime;
		rotationY += num2;
		base.transform.localRotation = Quaternion.Euler(0f, (float)((!counterclockwise.Value) ? 1 : (-1)) * rotationY, 0f);
	}
}
