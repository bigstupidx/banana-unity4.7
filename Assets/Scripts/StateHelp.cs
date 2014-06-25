using UnityEngine;
using System.Collections;

public class StateHelp : GameState {

	public UIRect Page1;
	public UIRect Page2;
	public UIToggle TogglePage1;
	public UIToggle TogglePage2;
	
	public UIWidget MonstersPanel;

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
		MonstersPanel.transform.localPosition = Vector3.zero;
	}
	
	public override void OnUpdate()
	{
		if( TogglePage1.value )
		{
			Page1.gameObject.SetActive(true);
			Page2.gameObject.SetActive(false);
			MonstersPanel.gameObject.SetActive(false);
		}
		else if( TogglePage2.value )
		{
			Page1.gameObject.SetActive(false);
			Page2.gameObject.SetActive(true);
			MonstersPanel.gameObject.SetActive(true);
		}
	}
	
	public override void OnExit()
	{
		MonstersPanel.gameObject.SetActive(false);
	}
	
	public override void OnBackKey()
	{
		StateManager.Instance.PopState();
	}

}
