using UnityEngine;
using System.Collections;

public abstract class Actor : MonoBehaviour {

	protected static int ANIM_IDLE = Animator.StringToHash("Base Layer.Idle");
	protected static int ANIM_ATTACK = Animator.StringToHash("Base Layer.Attack");
	protected static int ANIM_DYING = Animator.StringToHash("Base Layer.Dying");
	protected static int ANIM_CLIMB = Animator.StringToHash("Base Layer.Climb");	
	protected static int VAR_WALK = Animator.StringToHash("Walk");
	protected static int VAR_ATTACK = Animator.StringToHash("Attack");
	protected static int VAR_CLIMB = Animator.StringToHash("Climb");
	protected static int VAR_PUSH_UP = Animator.StringToHash("PushUp");

	protected Animator m_animator;
	protected int m_lastAnimationLoop;

	protected ObsecuredInt m_HP;

	protected float m_speedFactor = 1.0f;

	public Vector2 BoundingSize  = Vector2.one;

	public float MoveSpeed = 1;
	public float ClimbSpeed = 1;
	
	protected Vector3 m_dyingFallRotation;
	private float m_dyingFallGravity;

	protected virtual void Awake()
	{
		m_animator = this.GetComponent<Animator> ();		
		m_dyingFallRotation = Vector3.zero;
		m_dyingFallGravity = 0.0f;		
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

	public void FaceTo(Vector3 targetPos, float speed)
	{
		Vector3 pos = transform.position;
		Vector2 v1 = new Vector2(pos.x, pos.z);
		Vector2 v2 = new Vector2(targetPos.x, targetPos.z);
		Vector2 v = v2 - v1;
		FaceTo(Mathf.Atan2(v.x, v.y) * Mathf.Rad2Deg, speed);
	}

	public Vector2 FeetPosition
	{
		get 
		{ 
			Vector3 rootPos = m_animator.rootPosition;
			return m_animator.isHuman ? (Vector2)rootPos : new Vector2(rootPos.x, rootPos.y - BoundingSize.y * 0.5f);
		}

		set
		{
			Vector3 pos = (Vector3)value;
			if( !m_animator.isHuman )
			{
				pos.y += BoundingSize.y * 0.5f;
			}
			Vector3 delta = pos - m_animator.rootPosition;
			delta.z = 0.0f;
			pos = transform.localPosition + delta;
			transform.localPosition = pos;
		}
	}

	protected void StandOnWall()
	{
		Vector2 feetPos = FeetPosition;
		feetPos.y += (Global.WALL_TOP_Y - feetPos.y) * Time.deltaTime * 5;
		FeetPosition = feetPos;
	}


	protected bool IsClimbingReachWall
	{
		get
		{
			return FeetPosition.y + BoundingSize.y >= Global.WALL_TOP_Y;
		}
	}

	protected bool IsCurrentAnim(int hash)
	{
		return m_animator.GetCurrentAnimatorStateInfo (0).nameHash == hash;
	}

	protected bool CanAttackMelee(Actor other)
	{
		Vector2 myFeetPos = (Vector2)transform.localPosition;
		Vector2 yourFeetPos = (Vector2)other.transform.localPosition;
		return !(myFeetPos.y > yourFeetPos.y + other.BoundingSize.y || myFeetPos.y + this.BoundingSize.y < yourFeetPos.y)
			&& (Mathf.Abs(myFeetPos.x - other.FeetPosition.x) < (BoundingSize.x + other.BoundingSize.x));
	}
	
	public bool IsHorizontalOverlapWith(Actor other)
	{
		return (Mathf.Abs(transform.localPosition.x - other.transform.localPosition.x) < (BoundingSize.x + other.BoundingSize.x) * 0.5f);
	}
	
	public bool IsOverlapWith(Actor other)
	{
		Vector2 myFeet = FeetPosition;
		Vector2 otherFeet = other.FeetPosition;
		return (Mathf.Abs(myFeet.x - otherFeet.x) < (BoundingSize.x + other.BoundingSize.x))
			&& (Mathf.Abs(myFeet.y - otherFeet.y) < (BoundingSize.y + other.BoundingSize.y));
	}
	
	public virtual void TakeHit(int damage)
	{
		m_HP -= damage;
	}
	
	public void FallOnDying()
	{
		m_dyingFallGravity += Global.DYING_FALL_SPEED * Time.deltaTime;
	
		Vector3 pos = transform.localPosition;
		pos.y -= m_dyingFallGravity * Time.deltaTime;
		pos.z = Global.DYING_Z;
		transform.localPosition = pos;
		
		if (pos.y < Global.HELL_Y) {
			GameObject.Destroy (gameObject);
			EnemiesManager.Instance.RemoveEnemy (this);
			return;
		}
		
		transform.Rotate(m_dyingFallRotation * Time.deltaTime);
	}
	
	public abstract bool IsDying
	{
		get;
	}
	
	public abstract bool IsOnWall
	{
		get;
	}
	
	public abstract bool IsNearWall
	{
		get;
	}
	
	public abstract bool IsClimbing
	{
		get;
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.green;

		Vector3 pos = this.transform.position;
		Animator animator = this.GetComponent<Animator> ();
		if (animator != null) {
			pos = animator.rootPosition;
		}

		Gizmos.DrawWireCube(pos, BoundingSize);
	}
}
