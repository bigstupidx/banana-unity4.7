using UnityEngine;
using System.Collections;

public class PlayerStash : MonoBehaviour {

	private static PlayerStash g_instance = null;
	
	public static PlayerStash Instance 
	{
		get { return g_instance; }
	}
	
	void Awake()
	{
		g_instance = this;
		
		m_isSound = PlayerPrefs.GetInt("IsSound", 1) != 0;
		m_currentScore = new ObsecuredInt(PlayerPrefs.GetString("d64148fac8fb5fc0d1847723e5741e6c"));
		m_highScore = new ObsecuredInt(PlayerPrefs.GetString("5f3bf5b30938ae565eaa5ffeff1babe3"));
		m_purchasedAxe = new ObsecuredInt(PlayerPrefs.GetString("684c19d5b68aa835dc7444240d9bb309"));
		m_purchasedCossbow = new ObsecuredInt(PlayerPrefs.GetString("3a327194ca41d8ee1ceb4779da0200c8"));
		m_purchasedSnow = new ObsecuredInt(PlayerPrefs.GetString("cffd8be9288fab9d702c845368980cec"));
		m_purchasedFire = new ObsecuredInt(PlayerPrefs.GetString("7b0eacc9164c3e63ba470262763bc771"));
	}
	
	void OnApplicationQuit() 
	{
		PlayerPrefs.Save();
	}
	
	public bool RecordHighScore()
	{
		if( CurrentScore > HighScore )
		{
			HighScore = CurrentScore;
			PlayerPrefs.Save();	
			return true;
		}		
		
		return false;
	}
	
	public bool IsSound
	{
		get { return m_isSound; }
		set
		{
			if( m_isSound != value )
			{
				m_isSound = value;
				PlayerPrefs.SetInt("IsSound", m_isSound ? 1 : 0);
			}
		}
	}	
	
	public int CurrentScore
	{
		get { return m_currentScore; }
		set
		{
			m_currentScore = value;
			PlayerPrefs.SetString("d64148fac8fb5fc0d1847723e5741e6c", m_currentScore.Gut);			
		}
	}
	
	public int HighScore
	{
		get { return m_highScore; }
		set
		{
			m_highScore = value;
			PlayerPrefs.SetString("5f3bf5b30938ae565eaa5ffeff1babe3", m_highScore.Gut);
		}
	}
	
	public int PurchasedAxe
	{
		get { return m_purchasedAxe; }
		set
		{
			m_purchasedAxe = value;
			PlayerPrefs.SetString("684c19d5b68aa835dc7444240d9bb309", m_purchasedAxe.Gut);
		}
	}
	
	public int PurchasedCrossbow
	{
		get { return m_purchasedCossbow; }
		set
		{
			m_purchasedCossbow = value;
			PlayerPrefs.SetString("3a327194ca41d8ee1ceb4779da0200c8", m_purchasedCossbow.Gut);
		}
	}
	
	public int PurchasedSnow
	{
		get { return m_purchasedSnow; }
		set
		{
			m_purchasedSnow = value;
			PlayerPrefs.SetString("cffd8be9288fab9d702c845368980cec", m_purchasedSnow.Gut);
		}
	}
	
	public int PurchasedFire
	{
		get { return m_purchasedFire; }
		set
		{
			m_purchasedFire = value;
			PlayerPrefs.SetString("7b0eacc9164c3e63ba470262763bc771", m_purchasedFire.Gut);
		}
	}
	
	private bool m_isSound;
	private ObsecuredInt m_highScore;
	private ObsecuredInt m_currentScore;
	
	private ObsecuredInt m_purchasedAxe;
	private ObsecuredInt m_purchasedCossbow;
	private ObsecuredInt m_purchasedSnow;
	private ObsecuredInt m_purchasedFire;
}
