using UnityEngine;
using System.Collections;

public class StateLeaderboard : GameState {

	void Awake()
	{
		UIEventListener.Get (FindChild ("ButtonBack")).onClick += (obj) =>
		{
			StartCoroutine (OnBackButtonClick ());
		};
	}
	
	private IEnumerator OnBackButtonClick()
	{
		yield return StartCoroutine(Utils.WaitForRealSeconds(0.25f));
		
		StateManager.Instance.PopState();
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
