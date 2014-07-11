using UnityEngine;
using System.Collections;
using GoogleMobileAds.Api;

public class AdsController : MonoBehaviour {

	private static AdsController g_instance = null;
	
	public static AdsController Instance
	{
		get { return g_instance; }
	}
	
	private const int DYING_TIMES_TO_SHOW_ADS = 5;
	
	private int m_dyingTimes = 0;	
	private bool m_willQuit = false;
	
	private static InterstitialAd interstitial;
	
	void Awake()	
	{
		g_instance = this;		
	}
	
	public static void RequestInterstitial()
	{
		// Initialize an InterstitialAd.
		interstitial = new InterstitialAd("ca-app-pub-9930885820523333/5093352206");
		// Create an empty ad request.
		AdRequest request = new AdRequest.Builder().Build();				
		// Load the interstitial with the request.
		interstitial.LoadAd(request);
	}

	// Use this for initialization
	void Start () {		
		m_dyingTimes = 0;
		m_willQuit = false;
		
		if( interstitial != null && interstitial.IsLoaded() )
		{
			interstitial.Show();
					
			// Initialize an InterstitialAd.
			interstitial = new InterstitialAd("ca-app-pub-9930885820523333/5093352206");
			
			// Create an empty ad request.
			AdRequest request = new AdRequest.Builder().Build();				
			
			// Load the interstitial with the request.
			interstitial.LoadAd(request);
		}
	}
	
	public void OnQuit()
	{
		if( interstitial.IsLoaded() )
		{
			m_willQuit = true;
			interstitial.Show();
		}
		else
		{
			Application.Quit();
		}
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
			
			if( interstitial.IsLoaded() )
			{
				interstitial.Show();
				
				// Initialize an InterstitialAd.
				interstitial = new InterstitialAd("ca-app-pub-9930885820523333/5093352206");
				
				// Create an empty ad request.
				AdRequest request = new AdRequest.Builder().Build();				
				
				// Load the interstitial with the request.
				interstitial.LoadAd(request);
			}
		}
	}
	
	void OnApplicationQuit()
	{
		
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
