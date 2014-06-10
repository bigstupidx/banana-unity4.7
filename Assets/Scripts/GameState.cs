using UnityEngine;
using System.Collections;

public abstract class GameState : MonoBehaviour {

	public abstract void OnEnter();
	public abstract void OnUpdate();
	public abstract void OnExit();
	
	protected GameObject FindChild(string searchName)
	{
		Transform[] transforms = this.GetComponentsInChildren<Transform> ();

		foreach (Transform child in transforms) {
			if( child.name.Equals(searchName) )
			{
				return child.gameObject;
			}
		}

		return null;
	}
}
