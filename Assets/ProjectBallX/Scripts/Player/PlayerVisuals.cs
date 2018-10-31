using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisuals : MonoBehaviour {

	public Rigidbody2D targetBody;
	public Transform model;
	public float maxRpm = 20;

	float rotation = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void LateUpdate ()
	{
		rotation += Mathf.Clamp(targetBody.angularVelocity, -maxRpm * 60, maxRpm * 60) * Time.deltaTime;

		if(model)
		{
			model.localRotation = Quaternion.Euler(0,0,rotation);
			model.position = targetBody.transform.position;
		}
	}
}
