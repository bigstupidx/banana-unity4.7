﻿using UnityEngine;
using System.Collections;

public class Arrow : Projectile {

	private const float ARROW_SPEED = 20;

	public GameObject Trail;

	private bool m_isDetached = false;

	void Update()
	{
		if (Mathf.Approximately (Time.timeScale, 0.0f)) {
			return;
		}

		if (m_isDetached) {
			Vector3 pos = transform.localPosition;
			pos.y -= ARROW_SPEED * Time.deltaTime;
			transform.localPosition = pos;

			if( pos.y <= Global.HELL_Y )
			{
				GameObject.Destroy(gameObject);
				return;
			}
		}
	}

	public void Detach()
	{
		Trail.SetActive (true);

		transform.parent = null;
		transform.localRotation = Quaternion.Euler(90, 0, 0);
		m_isDetached = true;

		Vector3 pos = transform.localPosition;
		pos.z = Global.PROJECTILE_Z;
		transform.localPosition = pos;
	}
}