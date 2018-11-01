using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class PlayerPhysics : MonoBehaviour {

	public Rigidbody2D rBody;
	public float maxRpm = 30.0f;
	public float maxTorque = 15.0f;

	public Player player;

	// Use this for initialization
	void Start ()
	{
		player = ReInput.players.GetPlayer(0);

		Physics2D.maxRotationSpeed = maxRpm * 60;
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		float moveInput = 0;

		bool goLeft = player.GetButton("Move Left");
		bool goRight = player.GetButton("Move Right");
		bool goBoth = goLeft && goRight;

		if(goBoth)
		{
			//Debug.DrawRay(rBody.position, Vector3.up * 3, Color.cyan);
		}
		else if(goLeft && rBody.angularVelocity < maxRpm * 60)
		{
			moveInput = 1;
			//Debug.DrawRay(rBody.position, -Vector3.right * 3, Color.red);
		}
		else if(goRight && rBody.angularVelocity > -maxRpm * 60)
		{
			moveInput = -1;
			//Debug.DrawRay(rBody.position, Vector3.right * 3, Color.green);
		}

		rBody.AddTorque(moveInput * maxTorque, ForceMode2D.Force);
		//rBody.angularVelocity = Mathf.Clamp(rBody.angularVelocity, -maxRpm * 60, maxRpm * 60);

		Vector3 hitDir = Vector3.zero;
		bool gotHit = false;
		// for (int i = 0; i < 20; i++)
		// {

		// 	Vector2 rayDir = new Vector2(Mathf.Sin((1f / 20) * i * Mathf.PI), Mathf.Cos((1f / 20) * i * Mathf.PI));
		// 	RaycastHit2D[] rHits = Physics2D.RaycastAll(rBody.position, rayDir, 0.6f);

		// 	float hitDist = 0.6f;
		// 	Vector2 dirToAdd = Vector2.zero;
		// 	bool validHit = false;
		// 	for (int j = 0; j < rHits.Length; j++)
		// 	{
		// 		if(rHits[j].distance < hitDist && !rHits[j].collider.transform.IsChildOf(transform))
		// 		{
		// 			dirToAdd = rHits[j].normal;
		// 			hitDist = rHits[j].distance;
		// 			gotHit = true;
		// 			validHit = true;
		// 		}
		// 	}

		// 	if(validHit)
		// 	{
		// 		hitDir += new Vector3(dirToAdd.x, dirToAdd.y, 0);
		// 	}

		// }
		ContactPoint2D[] points = new ContactPoint2D[8];
		rBody.GetContacts(points);

		gotHit = points.Length > 0;
		for (int i = 0; i < points.Length; i++)
		{
			hitDir += new Vector3(points[i].normal.x, points[i].normal.y, 0);
		}

		if(gotHit)
		{
			Debug.DrawRay(rBody.position, hitDir * 3, Color.green);
		}
	}
}
