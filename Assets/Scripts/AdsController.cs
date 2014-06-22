using UnityEngine;
using System.Collections;

public class AdsController : MonoBehaviour {

	private static AdsController g_instance = null;
	
	public static AdsController Instance
	{
		get { return g_instance; }
	}
	
	void Awake()	
	{
		g_instance = this;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	void OnApplicationQuit()
	{
	}
	
	void OnApplicationPause()
	{
	}
}
