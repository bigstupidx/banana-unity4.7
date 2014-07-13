using UnityEngine;
using System.Collections;
using System.IO;

public class SpriteRecorder : MonoBehaviour {

	public string prefix = "screenshot";

	public Animator RecordingAnimator;
	public string ClipName;
	public int FrameSkip = 1;
	
	private float m_wait;	
	private int m_frameSkipCounter = 0;

	void Awake()
	{
		Application.targetFrameRate = 15;
		RecordingAnimator.speed = 0.5f;
		
		m_wait = 1.0f;
		RecordingAnimator.enabled = false;
		m_frameSkipCounter = FrameSkip;
	}
	
	void Start()
	{
		RecordingAnimator.CrossFade(ClipName, 0.0f, 0, 0.0f);
	}
	
	void Update()
	{
		if( m_wait > 0.0f )
		{
			m_wait -= Time.deltaTime;
		}
		else
		{
			RecordingAnimator.enabled = true;
		}
		
		if( m_frameSkipCounter > 0 )
		{
			--m_frameSkipCounter;
		}
		else
		{
			m_frameSkipCounter = FrameSkip;
		}
	}

	// Update is called once per frame
	private int sshotnum = 0;
	void OnPostRender () {	
		if( m_wait > 0.0f )
		{
			return;
		}
		
		if( m_frameSkipCounter > 0 )
		{
			return;
		}
	
		float ntime = RecordingAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		if( ntime >= 1.0f )
		{
			return;
		}
				
		int ssn = sshotnum++;
		
		Texture2D sshot = new Texture2D(400, 400);
		sshot.ReadPixels(new Rect((Screen.width / 2 - 200), (Screen.height / 2 - 200), 400, 400), 0, 0, false);				
		sshot.Apply();
		
		byte[] pngShot = sshot.EncodeToPNG();
		Destroy(sshot);
		
		File.WriteAllBytes(Application.dataPath + "/../" + prefix + "_" + ClipName + "_" + string.Format("{0,0}", ssn.ToString("D4")) + ".png", pngShot);		
		
	}
}
