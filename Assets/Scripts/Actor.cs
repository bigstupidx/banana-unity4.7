using UnityEngine;
using System.Collections;

public class Actor : MonoBehaviour {

	protected Animator m_animator;
	protected int m_lastAnimationLoop;

	protected ObsecuredInt m_HP;

	protected virtual void Awake()
	{
		m_animator = this.GetComponent<Animator> ();
	}

	protected void SetAnimationCounter()
	{
		m_lastAnimationLoop = Mathf.FloorToInt(GetAnimationTime());
	}

	protected void ResetAnimationCounter()
	{
		m_lastAnimationLoop = 0;
	}

	protected float GetAnimationTime()
	{
		return m_animator.GetCurrentAnimatorStateInfo (0).normalizedTime;
	}

	protected void FaceTo(float rot, float speed)
	{
		Quaternion quat = transform.localRotation;
		Vector3 euler = quat.eulerAngles;
		euler.y += Utils.shortestAngle (euler.y, rot) * Time.deltaTime * speed;
		quat.eulerAngles = euler;
		transform.localRotation = quat;
	}
}
