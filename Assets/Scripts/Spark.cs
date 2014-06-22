using UnityEngine;
using System.Collections;

public class Spark : MonoBehaviour {

	public float Duration = 2.0f;

	private float m_timeOut = 0.0f;	
	
	public AudioClip[] Sounds;
	
	void Start()
	{
		Utils.PlaySoundRandomly(this.audio, Sounds);
	}	
	
	// Update is called once per frame
	void Update () {
		m_timeOut += Time.deltaTime;
		if( m_timeOut >= Duration )
		{
			GameObject.Destroy(this.gameObject);
		}	
	}
}
