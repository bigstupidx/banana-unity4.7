using UnityEngine;
using System.Collections;

public class SpineBender : MonoBehaviour {

	public Transform Spine;
	
	// Update is called once per frame
	void LateUpdate () {
		Quaternion quat = Spine.localRotation;
		Vector3 euler = quat.eulerAngles;
		euler.z = -60.0f;
		quat.eulerAngles = euler;
		Spine.localRotation = quat;
	}
}
