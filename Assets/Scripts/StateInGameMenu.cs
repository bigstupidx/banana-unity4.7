using UnityEngine;
using System.Collections;

public class StateInGameMenu : GameState {

	private float m_dyingTimeOut;

	public override void OnEnter()
	{
		m_dyingTimeOut = 3.0f;
	}
	
	public override void OnUpdate()
	{
		if( Player.Instance.IsDying )
		{
			m_dyingTimeOut -= Time.deltaTime;
			if( m_dyingTimeOut <= 0.0f )
			{
				StateManager.Instance.SetState(StateManager.Instance.MainMenu);
			}
		}
	}
	
	public override void OnExit()
	{
		m_dyingTimeOut = 3.0f;
	}
}
