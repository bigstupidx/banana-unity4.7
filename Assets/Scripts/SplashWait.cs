using UnityEngine;
using System.Collections;

public class SplashWait : MonoBehaviour {

	private float m_timeout = 1.0f;

	// Update is called once per frame
	void Update () {
		m_timeout -= Time.deltaTime;
		if( m_timeout <= 0 )
		{
			Application.LoadLevel("Main");
		}
		
		if( Input.touchCount > 0 || Input.GetMouseButton(0) )
		{
			m_timeout = 0.0f;			
		}
	}
}
