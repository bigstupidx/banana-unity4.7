using UnityEngine;
using System.Collections;

public class ProjectilesManager : MonoBehaviour {

	private static ProjectilesManager g_instance = null;

	// The scene root of projectiles
	public Transform Root;

	// Templates
	public const int ARROW = 0;

	public GameObject[] Templates;

	void Awake()
	{
		g_instance = this;
	}

	public static ProjectilesManager Instance
	{
		get { return g_instance; }
	}

	public T Create<T>(int template, Transform parent) where T:MonoBehaviour
	{
		GameObject obj = GameObject.Instantiate (Templates[template]) as GameObject;
		obj.transform.parent = parent;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localRotation = Quaternion.identity;
		return obj.GetComponent<T> ();
	}
}
