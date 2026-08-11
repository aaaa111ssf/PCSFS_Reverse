using SFS.World;
using SFS.World.PlanetModules;
using UnityEngine;

namespace InterplanetaryModule;

public class SurfaceShadingModule : MonoBehaviour, Rocket.INJ_Location
{
	public Texture2D shadowTexture;

	public SpriteRenderer spriteRenderer;

	public Transform rotationTransform;

	public Color baseColor = Color.white;

	private Location location;

	Location Rocket.INJ_Location.Location
	{
		set
		{
			location = value;
		}
	}

	private void Update()
	{
		float height = (float)(location.position.magnitude - location.planet.Radius);
		PostProcessingModule.Key key = location.planet.data.postProcessing.Evaluate(height);
		float y = base.transform.localEulerAngles.y;
		float num = 0f;
		if (rotationTransform != null)
		{
			num = rotationTransform.localEulerAngles.y;
		}
		float num2 = (num + y) / 360f % 1f;
		num2 = ((num2 < 0f) ? (num2 + 1f) : num2);
		if (0.25 <= (double)num2 && (double)num2 < 0.5)
		{
			num2 += 0.5f;
		}
		if (0.5 <= (double)num2 && (double)num2 < 0.75)
		{
			num2 -= 0.5f;
		}
		Color color = new Color(0f, 0f, 0f, 0f);
		if ((0f <= num2 && (double)num2 < 0.25) || (0.75 <= (double)num2 && num2 < 1f))
		{
			if ((double)num2 > 0.5)
			{
				num2 -= 1f;
			}
			num2 = (0f - num2 + 0.25f) * 2f;
			float num3 = 1f - shadowTexture.GetPixel((int)(num2 * (float)shadowTexture.width), 0).r;
			num3 = 1f - Mathf.Clamp01(num3 * key.shadowIntensity);
			color = new Color(baseColor.r * num3, baseColor.g * num3, baseColor.b * num3);
		}
		spriteRenderer.color = color;
	}
}
