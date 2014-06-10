using UnityEngine;
using System.Collections;

public class Utils {
	
	public static float shortestAngle(float start, float end)
	{
		return ((((end - start) % 360.0f) + 540.0f) % 360.0f) - 180.0f;
	}	
	
	public static Quaternion rotationByEulerZ(float z)
	{
		float rollOver2 = z * (Mathf.Deg2Rad * 0.5f);
		return new Quaternion(0.0f, 0.0f, Mathf.Sin(rollOver2), Mathf.Cos(rollOver2));
	}
	
	public static float snapRotationBy90(float z)
	{
		return Mathf.Round(z / 90.0f) * 90.0f;
	}
	
	public static bool odd(int v)
	{
		return (v & 1) != 0;
	}
	
	public static bool even(int v)
	{
		return (v & 1) == 0;
	}

	public static bool Possible50()
	{
		return (Random.Range(0, 100) & 1) == 0;
	}

	public static IEnumerator WaitForRealSeconds(float time)
	{
		float start = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup < start + time)
		{
			yield return null;
		}
	}
}
