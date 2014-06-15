using UnityEngine;
using System.Collections;

public class EnemyGoblin : EnemyMelee {
	public override void TakeHit(int damage)
	{
		m_HP -= damage;
		m_speedFactor = 0.0f;
	}
}
