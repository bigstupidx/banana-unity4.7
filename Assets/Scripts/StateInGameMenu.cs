using UnityEngine;
using System.Collections;

public class StateInGameMenu : GameState {

	private float m_dyingTimeOut;
	
	public UILabel ScoreLabel;
	public UISlider HealthBar;
	public UIButton ButtonSnow;
	public UILabel LabelSnowQuantity;
	public UIButton ButtonFire;
	public UILabel LabelFireQuantity;		
	
	void Awake()
	{
		UIEventListener.Get (FindChild ("ButtonPause")).onClick += (obj) =>
		{
			StartCoroutine (OnPauseButtonClick ());
		};
		
		UIEventListener.Get (ButtonSnow.gameObject).onClick += (obj) =>
		{
			if( PlayerStash.Instance.PurchasedSnow > 0 && !Player.Instance.IsDying )
			{
				Blizzard.Instance.Duration += 25.0f;
				--PlayerStash.Instance.PurchasedSnow;
			}
		};
		
		UIEventListener.Get (ButtonFire.gameObject).onClick += (obj) =>
		{
			if( PlayerStash.Instance.PurchasedFire > 0 && !Player.Instance.IsDying )
			{
				Apocalypse.Instance.RemainingFireballs += 10;		
				--PlayerStash.Instance.PurchasedFire;
			}
		};
	}
	
	private IEnumerator OnPauseButtonClick()
	{
		yield return StartCoroutine(Utils.WaitForRealSeconds(0.25f));
		
		Time.timeScale = 0.0f;
		StateManager.Instance.PushState(StateManager.Instance.PauseMenu);
	}
	
	public override void OnEnter()
	{
		EnemiesManager.Instance.Paused = false;		
		
		m_dyingTimeOut = 3.0f;
		Time.timeScale = 1.0f;
	}
	
	public override void OnUpdate()
	{
		if( Player.Instance.IsDying )
		{
			m_dyingTimeOut -= Time.deltaTime;
			if( m_dyingTimeOut <= 0.0f )
			{
				Blizzard.Instance.Reset();
				Apocalypse.Instance.Reset();
				StateManager.Instance.PushState(StateManager.Instance.GameOver);
			}
			return;
		}
		
		HealthBar.value = ((float)Player.Instance.HP) / Player.Instance.MaxHP;
		ScoreLabel.text = PlayerStash.Instance.CurrentScore.ToString();
		
		if( PlayerStash.Instance.PurchasedSnow > 0 )
		{
			LabelSnowQuantity.enabled = true;
			LabelSnowQuantity.text = PlayerStash.Instance.PurchasedSnow.ToString();
			ButtonSnow.gameObject.SetActive(true);
		}
		else
		{
			LabelSnowQuantity.enabled = false;
			ButtonSnow.gameObject.SetActive(false);
		}
		
		if( PlayerStash.Instance.PurchasedFire > 0 )
		{
			LabelFireQuantity.enabled = true;
			LabelFireQuantity.text = PlayerStash.Instance.PurchasedFire.ToString();
			ButtonFire.gameObject.SetActive(true);
		}
		else
		{
			LabelFireQuantity.enabled = false;
			ButtonFire.gameObject.SetActive(false);
		}
		
		Vector3 pos = Camera.main.transform.localPosition;
		pos.y += (9.0f - pos.y) * Time.deltaTime * 2;
		Camera.main.transform.localPosition = pos;	
		
		if( PlayerStash.Instance.CurrentScore >	PlayerStash.Instance.HighScore )
		{
			StateGameOver.HasHighscoreBeaten = true;
		}
	}
	
	public override void OnExit()
	{
		EnemiesManager.Instance.Paused = true;	
	}
	
	public override void OnBackKey()
	{
		Time.timeScale = 0.0f;
		StateManager.Instance.PushState(StateManager.Instance.PauseMenu);
	}
}
