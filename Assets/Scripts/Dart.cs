using UnityEngine;
using System.Collections;

public class Dart : Projectile {
	
	private const float DART_SPEED = 10;
	
	private bool m_isDetached = false;
	
	private Vector3 m_direction;
	
	void Update()
	{
		if (Mathf.Approximately (Time.timeScale, 0.0f)) {
			return;
		}
		
		if (m_isDetached) {
			Vector3 pos = transform.localPosition;			
			
			if( pos.y >= Global.HEAVEN_Y )
			{
				m_isDetached = false;
				GameObject.Destroy(gameObject);				
				return;
			}
			
			Vector3 nextPos = pos + DART_SPEED * Time.deltaTime * m_direction;
			
			// Test hit
			Vector2 feetPos = Player.Instance.FeetPosition;
			Vector2 bound = Player.Instance.BoundingSize;
			
			float myx1 = pos.x;
			float myx2 = nextPos.x;
			if( myx1 > myx2 )
			{
				float temp = myx1;
				myx1 = myx2;
				myx2 = temp;
			}
			
			if( Utils.TestRectsHit(myx1, pos.y,
								 myx2, nextPos.y,
			                     feetPos.x - bound.x * 0.5f, feetPos.y + bound.y * 0.5f,
			                     feetPos.x + bound.x * 0.5f, feetPos.y + bound.y) )
			{
				Player.Instance.TakeHit(1);
				
				ProjectilesManager.Instance.Create(ProjectilesManager.ZAP, nextPos);
				
				GameObject.Destroy(this.gameObject);	
				return;
			}
			
			// Fly
			pos = nextPos;
			
			if( pos.y >= Global.HEAVEN_Y )
			{	
				GameObject.Destroy(this.gameObject);				
				return;
			}
			transform.localPosition = pos;			
			
			transform.Rotate(new Vector3(0, 0, 1000 * Time.deltaTime));			
		}
	}
	
	public override void Detach()
	{		
		transform.parent = ProjectilesManager.Instance.Root;		
		m_isDetached = true;		
		
		this.audio.Play();
	}	
	
	public override void SetDirection(Vector3 dir)
	{		
		m_direction = dir;		
		this.transform.LookAt(transform.localPosition + m_direction);
		
		Vector3 pos = transform.localPosition;		
		pos.z = Global.PROJECTILE_Z;
		transform.localPosition = pos;
	}
}
