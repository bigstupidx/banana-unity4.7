using UnityEngine;
using System.Collections;

public class Explosive : MonoBehaviour {

	private const float SQUARE_EXPLOSIVE_RANGE = 4.0f * 4.0f;

	public float Duration = 4.0f;
	private bool m_hasDoneDamage = false;
	
	// Update is called once per frame
	void Update () {
		if( !m_hasDoneDamage )
		{
			Vector2 myPos = (Vector2)transform.localPosition;
			foreach( Actor enemy in EnemiesManager.Instance.Enemies )
			{
				Vector2 delta = myPos - enemy.CenterPosition;
				if( delta.sqrMagnitude < SQUARE_EXPLOSIVE_RANGE )
				{
					enemy.TakeHit(6);
				}
			}
			m_hasDoneDamage = true;
		}
	
		Duration -= Time.deltaTime;
		if( Duration <= 0.0f )
		{
			GameObject.Destroy(this.gameObject);
		}
	}
}
