using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_FollowPlayer : MonoBehaviour {

	public Transform target;
	public Vector2 placement = new Vector2(-10, 1);

	public float trackingSpeed = 10.0f;

	Vector3 targetPos = Vector3.zero;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void LateUpdate ()
	{
		Vector3 newTarget = target.position + Vector3.forward * placement.x + Vector3.up * placement.y;
		targetPos = Vector3.Lerp(targetPos, newTarget, Time.deltaTime * trackingSpeed);

		transform.position = targetPos;
		transform.LookAt(target.position, Vector3.up);
	}
}
