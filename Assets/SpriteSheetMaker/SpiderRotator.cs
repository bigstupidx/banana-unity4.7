using UnityEngine;
using System.Collections;

public class SpiderRotator : MonoBehaviour {
	
	float rotation = -90.0f;
	float m_wait = 1.0f;
	
	// Update is called once per frame
	void LateUpdate () {		
		if( m_wait > 0.0f )
		{
			m_wait -= Time.deltaTime;
			return;
		}
	
		Quaternion quat = transform.localRotation;
		Vector3 euler = quat.eulerAngles;
		rotation += Time.deltaTime * 30;
		euler.x = rotation;
		quat.eulerAngles = euler;
		transform.localRotation = quat;
	}
}
