using UnityEngine;
using System.Collections;

public class AdsController : MonoBehaviour {

	private static AdsController g_instance = null;
	
	public static AdsController Instance
	{
		get { return g_instance; }
	}
	
	private const int DYING_TIMES_TO_SHOW_ADS = 5;
	
	private int m_dyingTimes = 0;	
	private bool m_willQuit = false;
	
	void Awake()	
	{
		g_instance = this;
	}

	// Use this for initialization
	void Start () {
		VservPlugin vservPlugin = new VservPlugin();
		vservPlugin.SetShowAt("start");
		vservPlugin.DisplayAd("c5c58ba3");
		
		m_dyingTimes = 0;
		m_willQuit = false;
	}
	
	public void OnQuit()
	{
		m_willQuit = true;
		
		VservPlugin vservPlugin = new VservPlugin();
		vservPlugin.SetShowAt("end");
		vservPlugin.DisplayAd("c5c58ba3");
	}
	
	public void OnDying()
	{
		++m_dyingTimes;
	}
	
	public void OnMiddle()
	{		
		if( m_dyingTimes >= DYING_TIMES_TO_SHOW_ADS )
		{	
			m_dyingTimes = 0;
			
			VservPlugin vservPlugin = new VservPlugin();
			vservPlugin.SetShowAt("in");
			vservPlugin.DisplayAd("c5c58ba3");
		}
	}
	
	void OnApplicationQuit()
	{
		VservPlugin vservPlugin = new VservPlugin();
		vservPlugin.DestroyApp();
	}
	
	void OnApplicationPause()
	{
	}
	
	void OnApplicationFocus()
	{
		if( m_willQuit )
		{
			Application.Quit();
		}
	}
}
