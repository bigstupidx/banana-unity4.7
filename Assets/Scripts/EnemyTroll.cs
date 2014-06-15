using UnityEngine;
using System.Collections;

public class EnemyTroll : EnemyMelee {

	public override void TakeHit(int damage)
	{
		m_HP -= damage;
		m_speedFactor = 3.0f;
	}
}
