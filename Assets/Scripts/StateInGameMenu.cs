using UnityEngine;
using System.Collections;

public class StateInGameMenu : GameState {

	private float m_dyingTimeOut;
	
	public UILabel ScoreLabel;
	public UISlider HealthBar;
	
	void Awake()
	{
	}

	public override void OnEnter()
	{
		m_dyingTimeOut = 3.0f;
		
		EnemiesManager.Instance.Paused = false;
		PlayerStash.Instance.CurrentScore = 0;
	}
	
	public override void OnUpdate()
	{
		if( Player.Instance.IsDying )
		{
			m_dyingTimeOut -= Time.deltaTime;
			if( m_dyingTimeOut <= 0.0f )
			{
				PlayerStash.Instance.RecordHighScore();
			
				StateManager.Instance.SetState(StateManager.Instance.MainMenu);
			}
			return;
		}
		
		HealthBar.value = ((float)Player.Instance.HP) / Player.Instance.MaxHP;
		ScoreLabel.text = PlayerStash.Instance.CurrentScore.ToString();
		
		Vector3 pos = Camera.main.transform.localPosition;
		pos.y += (9.0f - pos.y) * Time.deltaTime * 2;
		Camera.main.transform.localPosition = pos;
	}
	
	public override void OnExit()
	{
		m_dyingTimeOut = 3.0f;
		EnemiesManager.Instance.Paused = true;
	}
}
