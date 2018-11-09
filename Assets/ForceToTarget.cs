using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceToTarget : MonoBehaviour {

	public Transform target;
	[Tooltip("Maximum rate of acceleration")]
	public float maxAcceleration = 1000.0f;
	[Tooltip("Lower value means more free movement, but too low causes overshooting")]
	[Range(0,1)] public float velocityDampening = 0.1f;


	Rigidbody rBody; //optional

	//optional - get rigidbody
	void Start ()
	{
		rBody = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		//give it the target position and you get a nice
		Vector3 force = GetForceToPosition(target.position);

		//use ForceMode.Acceleration for best results
		rBody.AddForce(force, ForceMode.Acceleration);
	}

	Vector3 GetForceToPosition(Vector3 targetPosition)
	{
		Vector3 velocityCompensate = new Vector3(rBody.velocity.x, 0, rBody.velocity.z);
		velocityCompensate = velocityCompensate * velocityDampening;
		Vector3 endPos = (rBody.position + velocityCompensate);
		Vector3 targetDir = target.position - endPos;

		float distance = Vector3.Distance(target.position, endPos);

		float isWithinVelocityRadius = 1 - Mathf.Clamp01((distance / velocityCompensate.magnitude));

		Vector3 finalVector = Vector3.ClampMagnitude(targetDir, 1);
		finalVector = finalVector * maxAcceleration - Vector3.ClampMagnitude(velocityCompensate, isWithinVelocityRadius);

		return finalVector;
	}
}
