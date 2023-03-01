using Kamgam.Looping25D.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace Kamgam.Looping25D
{
	public class DemoWithTerrain : MonoBehaviour
	{
        [Header("References")]
		public Transform SpawnPosition;
		public Cameraman2D Cameraman2D;

		// Runtime References
		public Rigidbody2D BallBody;
		protected CircleCollider2D ballCollider;
        public CircleCollider2D BallCollider
        {
            get
            {
                if (ballCollider == null)
                {
                    ballCollider = BallBody.transform.GetComponent<CircleCollider2D>();
                }
                return ballCollider;
            }
        }

		public float Force = 450f;

		public void Start()
        {
			Application.targetFrameRate = 60;
			Restart();
		}

		public void Restart()
        {
			BallBody.transform.position = SpawnPosition.position;
			BallBody.velocity = Vector2.zero;
			BallBody.angularVelocity = 0f;

			// inform cameraman
			Cameraman2D.SetObjectToTrack(BallBody);
			Cameraman2D.SetCameraToMove(Camera.main.transform);
		}

        public void Update()
		{
#if ENABLE_INPUT_SYSTEM

			// New Input System
			Debug.LogWarning("The demo uses the OLD input system.");
#else

			// Old Input System
			if (Input.GetKey(KeyCode.Backspace))
			{
				Restart();
			}

			if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
			{
				RollBackward();
			}

			if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
			{
				RollForward();
			}

			if (Input.GetKeyDown(KeyCode.Space))
			{
				Jump();
			}

#endif


			bool groundFound = updateGroundHit();

			// Debug
			/*
			if (groundFound)
			{
				Debug.DrawRay(groundHit.point, Vector3.right * 0.2f, Color.red, 2f);
				Debug.DrawRay(groundHit.point, groundHit.normal, Color.blue, 2f);
			}
			//*/
		}

		public void RollBackward()
        {
			var direction = groundHit.collider == null ? Vector2.left : new Vector2(-groundHit.normal.y, groundHit.normal.x);
			BallBody.AddForce(direction * Force);
        }

		public void RollForward()
		{
			var direction = groundHit.collider == null ? Vector2.right : new Vector2(groundHit.normal.y,  -groundHit.normal.x);
			BallBody.AddForce(direction * Force);
		}

		public void Jump()
        {
			if (groundHit.distance < 2.3f)
			{
				BallBody.AddForce(groundHit.normal.normalized * Force, ForceMode2D.Impulse);
			}
		}


		#region ground detection

		RaycastHit2D[] tmpRayCastHits = new RaycastHit2D[1];
		RaycastHit2D groundHit = new RaycastHit2D();

		/// <summary>
		/// Performs 4 ray casts to determine an approximate position on the ground.
		/// </summary>
		/// <returns></returns>
		bool updateGroundHit()
		{
			// disable raycasts hitting triggers
			bool hitTriggers = Physics2D.queriesHitTriggers;
			Physics2D.queriesHitTriggers = false;

			try
			{
				float minDistance = 90999f;
				bool hitGround = false;

				// down
				int results = BallCollider.Raycast(Vector2.down, tmpRayCastHits, 10);
				updateGroundHitFromTmp(ref hitGround, ref results, ref minDistance);

				// up
				results = BallCollider.Raycast(Vector2.up, tmpRayCastHits, 10);
				updateGroundHitFromTmp(ref hitGround, ref results, ref minDistance);

				// left
				results = BallCollider.Raycast(Vector2.left, tmpRayCastHits, 10);
				updateGroundHitFromTmp(ref hitGround, ref results, ref minDistance);

				// right
				results = BallCollider.Raycast(Vector2.right, tmpRayCastHits, 10);
				updateGroundHitFromTmp(ref hitGround, ref results, ref minDistance);

				return hitGround;
			}
			finally
			{
				Physics2D.queriesHitTriggers = hitTriggers;
			}
		}

		protected void updateGroundHitFromTmp(ref bool hitGround, ref int results, ref float minDistance)
		{
			if (results > 0 && tmpRayCastHits[0].distance < minDistance)
			{
				groundHit = tmpRayCastHits[0];
				minDistance = groundHit.distance;
				hitGround = true;
			}
		}
		#endregion
	}
}
