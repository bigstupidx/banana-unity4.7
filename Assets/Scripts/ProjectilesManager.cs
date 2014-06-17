using UnityEngine;
using System.Collections;

public class ProjectilesManager : MonoBehaviour {

	private static ProjectilesManager g_instance = null;

	// The scene root of projectiles
	public Transform Root;

	// Templates
	public const int ARROW = 0;
	public const int POW2 = 1;
	public const int BAM = 2;
	public const int WHAM = 3;
	public const int POW = 4;
	public const int COCKTAIL = 5;
	public const int DART = 6;
	public const int BOOM1 = 7;
	public const int ZAP = 8;	
	public const int EXPLOSIVE = 9;

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
		return obj.GetComponent<T> ();
	}
	
	public void Create(int template, Vector3 position)
	{
		GameObject obj = GameObject.Instantiate (Templates[template]) as GameObject;
		obj.transform.parent = Root;
		obj.transform.localPosition = position;
	}
	
	public void CreateOnActor(int template, Actor actor)
	{
		Vector3 projectilePos = actor.FeetPosition;
		projectilePos.z = Global.PROJECTILE_Z;
		projectilePos.y += actor.BoundingSize.y * (0.25f + Random.Range(0.0f, 0.5f));
		projectilePos.x += actor.BoundingSize.x * (Random.Range(-0.25f, 0.25f));
		ProjectilesManager.Instance.Create(template, projectilePos);
	}
}
