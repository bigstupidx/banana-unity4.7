using UnityEngine;
using System.Collections;

public abstract class Projectile : MonoBehaviour {

	public abstract void Detach();
	public abstract void SetDirection(Vector3 dir);
}
