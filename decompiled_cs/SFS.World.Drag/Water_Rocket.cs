using System;
using System.Collections.Generic;
using System.Linq;
using SFS.Parts;
using SFS.Parts.Modules;
using SFS.WorldBase;
using UnityEngine;

namespace SFS.World.Drag;

public class Water_Rocket : MonoBehaviour
{
	public Rocket rocket;

	private int frameIndex;

	private void FixedUpdate()
	{
		frameIndex++;
		rocket.floating = false;
		Planet planet = rocket.location.planet.Value;
		if (!planet.data.hasWater)
		{
			return;
		}
		Double2 value = rocket.location.position.Value;
		double magnitude = value.magnitude;
		double num = magnitude - planet.Radius;
		double worldTime = ((WorldTime.main != null) ? WorldTime.main.worldTime : 0.0);
		float num2 = rocket.GetSizeRadius() + 5f;
		if (num > (double)num2)
		{
			return;
		}
		float num3 = (float)Math.Atan2(value.y, value.x) - MathF.PI / 2f;
		Matrix2x2 matrix2x = Matrix2x2.Angle(0f - num3);
		Matrix2x2 localToWorld = Matrix2x2.Angle(num3);
		float waterHeight = (WorldView.ToLocalPosition(value.normalized * planet.Radius) * matrix2x).y;
		float num4 = (SandboxSettings.main.settings.noGravity ? 0f : ((float)planet.GetGravity(magnitude)));
		Vector2 vector = value.normalized;
		Vector2 vector2 = WorldView.main.velocityOffset.Value;
		bool floating = false;
		bool flag = rocket.partHolder.parts.Count == 1;
		List<Part> list = new List<Part>();
		foreach (Part part in rocket.partHolder.parts)
		{
			PolygonCollider[] modules = part.GetModules<PolygonCollider>();
			float partArea = 0f;
			PolygonCollider[] array = modules;
			foreach (PolygonCollider polygonCollider in array)
			{
				float flipped;
				if (polygonCollider.isActiveAndEnabled)
				{
					flipped = Mathf.Sign(polygonCollider.transform.lossyScale.x * polygonCollider.transform.lossyScale.y);
					Vector2[] verticesWorld = polygonCollider.polygon.polygonFast.GetVerticesWorld(polygonCollider.transform);
					Calculate(verticesWorld[^1], verticesWorld[0]);
					for (int j = 0; j < verticesWorld.Length - 1; j++)
					{
						Calculate(verticesWorld[j], verticesWorld[j + 1]);
					}
				}
				void Calculate(Vector2 vector11, Vector2 vector10)
				{
					partArea += (vector10.x - vector11.x) * (vector10.y + vector11.y) / 2f * flipped;
				}
			}
			float totalArea = 0f;
			Vector2 centerOfArea = Vector2.zero;
			array = modules;
			foreach (PolygonCollider polygonCollider2 in array)
			{
				if (!polygonCollider2.isActiveAndEnabled)
				{
					continue;
				}
				float flipped2 = Mathf.Sign(polygonCollider2.transform.lossyScale.x * polygonCollider2.transform.lossyScale.y);
				Vector2[] verticesWorld2 = polygonCollider2.polygon.polygonFast.GetVerticesWorld(polygonCollider2.transform);
				float num5 = float.PositiveInfinity;
				float num6 = float.NegativeInfinity;
				for (int k = 0; k < verticesWorld2.Length; k++)
				{
					Vector2 vector3 = (verticesWorld2[k] *= matrix2x);
					if (vector3.x < num5)
					{
						num5 = vector3.x;
					}
					if (vector3.x > num6)
					{
						num6 = vector3.x;
					}
				}
				float a = GetWaveHeight(num5);
				float b = GetWaveHeight(num6);
				float[] array2 = new float[verticesWorld2.Length];
				for (int l = 0; l < verticesWorld2.Length; l++)
				{
					Vector2 vector4 = verticesWorld2[l];
					float num7 = Mathf.Lerp(a, b, Mathf.InverseLerp(num5, num6, vector4.x));
					verticesWorld2[l] = new Vector2(vector4.x, vector4.y - num7);
					array2[l] = num7;
				}
				Calculate2(verticesWorld2[^1], verticesWorld2[0], array2[^1], array2[0]);
				for (int m = 0; m < verticesWorld2.Length - 1; m++)
				{
					Calculate2(verticesWorld2[m], verticesWorld2[m + 1], array2[m], array2[m + 1]);
				}
				void Calculate2(Vector2 vector10, Vector2 vector11, float heightA, float heightB)
				{
					if (vector10.y > 0f)
					{
						if (vector11.y > 0f)
						{
							return;
						}
						float num17 = (0f - vector10.y) / (vector11.y - vector10.y);
						vector10 += (vector11 - vector10) * num17;
						heightA += (heightB - heightA) * num17;
					}
					else if (vector11.y > 0f)
					{
						float num18 = (0f - vector10.y) / (vector11.y - vector10.y);
						vector11 = vector10 + (vector11 - vector10) * num18;
						heightB = heightA + (heightB - heightA) * num18;
					}
					float num19 = Mathf.Max(vector10.y, vector11.y);
					if (num19 < -0.0001f)
					{
						float num20 = (vector11.x - vector10.x) * (0f - num19) * flipped2;
						Vector2 vector12 = new Vector2((vector11.x + vector10.x) / 2f, (num19 + heightA + heightB) / 2f);
						totalArea += num20;
						centerOfArea += vector12 * num20;
					}
					float num21 = Mathf.Abs(vector11.y - vector10.y);
					if (num21 > 0.0001f)
					{
						float num22 = (vector11.x - vector10.x) * num21 / 2f * flipped2;
						Vector2 vector13 = new Vector2(Mathf.Lerp(vector10.x, vector11.x, (vector10.y < vector11.y) ? 0.333f : 0.666f), Mathf.Lerp(vector10.y + heightA, vector11.y + heightB, (vector10.y > vector11.y) ? 0.333f : 0.666f));
						totalArea += num22;
						centerOfArea += vector13 * num22;
					}
				}
			}
			WheelModule wheelModule = part.GetModules<WheelModule>().FirstOrDefault();
			if (wheelModule != null)
			{
				float num8 = wheelModule.wheelSize / 2f;
				partArea = MathF.PI * num8 * num8;
				Vector2 vector5 = wheelModule.transform.position * matrix2x;
				float num9 = GetWaveHeight(vector5.x);
				float num10 = vector5.y - num9;
				if (num10 < num8)
				{
					if (num10 <= 0f - num8)
					{
						totalArea = partArea;
						centerOfArea = new Vector2(vector5.x, num10 + num9) * totalArea;
					}
					else
					{
						float num11 = num8 - num10;
						float num12 = num8 * num8 * Mathf.Acos((num8 - num11) / num8) - (num8 - num11) * Mathf.Sqrt(2f * num8 * num11 - num11 * num11);
						float num13 = Mathf.Acos((num8 - num11) / num8);
						float num14 = 4f * num8 * Mathf.Pow(Mathf.Sin(num13), 3f) / (3f * (2f * num13 - Mathf.Sin(2f * num13)));
						float num15 = num10 - num8 + num14;
						totalArea = num12;
						centerOfArea = new Vector2(vector5.x, num15 + num9) * totalArea;
					}
				}
			}
			if (Mathf.Abs(totalArea) < 0.01f)
			{
				continue;
			}
			floating = true;
			centerOfArea /= totalArea;
			totalArea = Mathf.Abs(totalArea);
			float num16 = totalArea / partArea;
			Vector2 vector6 = vector * (num16 * part.mass.Value * num4 / (float)part.density);
			Vector2 vector7 = centerOfArea * localToWorld;
			Vector2 vector8 = rocket.rb2d.GetPointVelocity(vector7) + vector2;
			float magnitude2 = vector8.magnitude;
			Vector2 vector9 = -(vector8 / magnitude2) * (Mathf.Pow(magnitude2, 1.2f) * 2f * totalArea);
			rocket.rb2d.AddForceAtPosition(vector6 + vector9, vector7, ForceMode2D.Force);
			if (flag)
			{
				rocket.rb2d.angularVelocity /= 1f + Time.fixedDeltaTime * 0.5f;
			}
			if (magnitude2 > 25f && !SandboxSettings.main.settings.unbreakableParts)
			{
				EffectManager.CreateSplashEffect(new Vector2(centerOfArea.x, waterHeight) * localToWorld, (part.mass.Value + 0.5f) * magnitude2 * 0.1f);
				rocket.rb2d.AddForceAtPosition(-vector8 * Mathf.Min(part.mass.Value, rocket.rb2d.mass / 2f), vector7, ForceMode2D.Impulse);
				list.Add(part);
			}
			else if (num16 > 0.1f)
			{
				if (frameIndex - 1 > part.frameIndex_WaterDamage && magnitude2 > 2f)
				{
					EffectManager.CreateSplashEffect(new Vector2(centerOfArea.x, waterHeight) * localToWorld, 3f);
				}
				part.frameIndex_WaterDamage = frameIndex;
			}
		}
		rocket.floating = floating;
		foreach (Part item in list)
		{
			item.DestroyPart(createExplosion: true, updateJoints: true, DestructionReason.WaterCollision);
		}
		float GetWaveHeight(float x)
		{
			double num17 = (WorldView.ToGlobalPosition(new Vector2(x, waterHeight) * localToWorld).AngleRadians / (Math.PI * 2.0) + 10.0) % 1.0 * 6.0;
			double num18 = worldTime % (Math.PI * 2.0);
			double num19 = num17 * (double)planet.commonDenominator % 1.0 * (Math.PI * 2.0) * (double)planet.surfaceWavesRepeat;
			return waterHeight + (float)Math.Cos(num18 + num19) * planet.data.water.wavesSize.y;
		}
	}
}
