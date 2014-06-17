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
	
	private int m_fireballsToSpawn = 0;
	private float m_fireballDelay;
	
	void Awake()
	{
		UIEventListener.Get (ButtonSnow.gameObject).onClick += (obj) =>
		{
			if( PlayerStash.Instance.PurchasedSnow > 0 )
			{
				Blizzard.Instance.Duration += 25.0f;
				--PlayerStash.Instance.PurchasedSnow;
			}
		};
		
		UIEventListener.Get (ButtonFire.gameObject).onClick += (obj) =>
		{
			if( PlayerStash.Instance.PurchasedFire > 0 )
			{
				m_fireballsToSpawn += 10;		
				--PlayerStash.Instance.PurchasedFire;
			}
		};
	}

	public override void OnEnter()
	{
		m_dyingTimeOut = 3.0f;
		
		EnemiesManager.Instance.Paused = false;
		PlayerStash.Instance.CurrentScore = 0;
		
		m_fireballsToSpawn = 0;
		m_fireballDelay = 0.0f;
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
		
		SpawnFireBalls();	
	}
	
	public override void OnExit()
	{
		m_dyingTimeOut = 3.0f;
		EnemiesManager.Instance.Paused = true;
	}
	
	private void SpawnFireBalls()	
	{
		if( m_fireballsToSpawn > 0 )
		{
			m_fireballDelay -= Time.deltaTime;
			if( m_fireballDelay <= 0.0f )
			{
				m_fireballDelay = Random.Range(0.1f, 0.4f);
				
				--m_fireballsToSpawn;		
				
				Vector3 pos;
				pos.x = Random.Range( Global.WALL_MIN_X + 1.0f, Global.WALL_MAX_X - 1.0f );
				pos.y = Random.Range( Global.GROUND_Y + 1.0f, Global.WALL_TOP_Y );				
				
				foreach( Actor enemy in EnemiesManager.Instance.Enemies )
				{
					if( Random.Range(0, 100) < 50 )
					{
						Vector3 enemyPos = (Vector3)enemy.CenterPosition;
						if( enemyPos.x > Global.WALL_MIN_X && enemyPos.x < Global.WALL_MAX_X )
						{
							if( enemyPos.y < Global.GROUND_Y + 1.0f )
							{
								enemyPos.y = Global.GROUND_Y + 1.0f;
							}
							else if( enemyPos.y > Global.WALL_TOP_Y - 1.0f )
							{
								enemyPos.y = Global.WALL_TOP_Y - 1.0f;
							}
							pos = enemyPos;
						}
						
					}
					
					if( Random.Range(0, 100) < 50 )
					{
						break;
					}
				}
				
				pos.z = Global.PROJECTILE_Z;				
				ProjectilesManager.Instance.Create(ProjectilesManager.EXPLOSIVE, pos);
			}
		}
	}
}
