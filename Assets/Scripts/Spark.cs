using UnityEngine;
using System.Collections;

public class Spark : MonoBehaviour {

	public float Duration = 2.0f;

	private float m_timeOut = 0.0f;	
	
	// Update is called once per frame
	void Update () {
		m_timeOut += Time.deltaTime;
		if( m_timeOut >= Duration )
		{
			GameObject.Destroy(this.gameObject);
		}	
	}
}
