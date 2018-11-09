using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class PlayerPhysics : MonoBehaviour {

	public Rigidbody2D rBody;
	public float maxRpm = 30.0f;
	public float maxTorque = 15.0f;
	[Space]
	public float jumpHeight = 4.0f;
	public float jumpCooldown = 0.2f;

	Player player;

	Vector2 jumpDir = Vector2.up;
	public bool isGrounded = false;
	float jumpTimer = 0;

	// Use this for initialization
	void Start ()
	{
		player = ReInput.players.GetPlayer(0);

		Physics2D.maxRotationSpeed = maxRpm * 60;
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		GetGroundHit();
		
		float moveInput = 0;

		bool goLeft = player.GetButton("Move Left");
		bool goRight = player.GetButton("Move Right");
		bool goBoth = goLeft && goRight;

		if(goBoth) { goLeft = goRight = false; }

		if(jumpTimer > 0)
		{
			jumpTimer -= Time.fixedDeltaTime;
			goBoth = false;
		}

		if(goBoth && isGrounded)
		{
			rBody.AddForce(jumpDir * Mathf.Sqrt(2 * jumpHeight * -Physics.gravity.y * rBody.gravityScale), ForceMode2D.Impulse);
			
			jumpTimer = jumpCooldown;

			Debug.DrawRay(rBody.position, jumpDir, Color.HSVToRGB(Random.Range(0f, 1f), 1, 1), 5, false);
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

		if(moveInput != 0)
		{
			rBody.AddTorque(moveInput * maxTorque, ForceMode2D.Force);
		}
		
	}

	void GetGroundHit()
	{
		
		isGrounded = false;

		ContactPoint2D[] points = new ContactPoint2D[8];
		rBody.GetContacts(points);
		
		int contacts = 0;
		for (int i = 0; i < points.Length; i++)
		{
			if(points[i].collider != null) contacts++;
		}

		isGrounded = contacts > 0;
		Vector3 hitDir = Vector3.zero;
		for (int i = 0; i < contacts; i++)
		{
			if(points[i].collider == null) continue;

			hitDir += new Vector3(points[i].normal.x, points[i].normal.y, 0);
		}

		jumpDir = hitDir.normalized;
		jumpDir = (jumpDir + Vector2.up).normalized;

		if(isGrounded)
		{
			Debug.DrawRay(rBody.position, jumpDir, Color.cyan);
		}
	}
}
