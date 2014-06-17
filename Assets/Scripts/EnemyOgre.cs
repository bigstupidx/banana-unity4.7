using UnityEngine;
using System.Collections;

public class EnemyOgre : EnemyMelee {

	protected override void Start()
	{
		base.Start();
		m_HP = MaxHP + (((EnemiesManager.Instance.SpawnGeneration - 1) * MaxHP) >> 1);
	}

}

