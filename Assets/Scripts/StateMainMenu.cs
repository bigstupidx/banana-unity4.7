using UnityEngine;
using System.Collections;

public class StateMainMenu : GameState {

	void Awake()
	{
		UIEventListener.Get (FindChild ("ButtonPlay")).onClick += (obj) =>
		{
			StartCoroutine (OnPlayButtonClick ());
		};
	}
	
	private IEnumerator OnPlayButtonClick()
	{
		yield return StartCoroutine(Utils.WaitForRealSeconds(0.25f));
		
		EnemiesManager.Instance.Reset();
		StateManager.Instance.PushState (StateManager.Instance.InGameMenu);		
	}

	public override void OnEnter()
	{
	}

	public override void OnUpdate()
	{
	}

	public override void OnExit()
	{
	}

}
