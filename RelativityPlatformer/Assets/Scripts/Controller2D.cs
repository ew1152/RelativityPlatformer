using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (BoxCollider2D))]
public class Controller2D : MonoBehaviour {

	public LayerMask collisionMask;
	public LayerMask slopeMask;
	public LayerMask enemyCollisionMask;
	public LayerMask enemyHurtboxMask;

	public static int health;

	const float skinWidth = .02f;
	public int horizontalRayCount = 4;
	public int verticalRayCount = 4;

	float maxClimbAngle = 70;
	float maxDescendAngle = 60;

	float horizontalRaySpacing;
	float verticalRaySpacing;

	public Camera cam;
	float camSize;
	Vector3 camPos;

	public GameObject background;
	Vector3 backgroundPos;
	bool backgroundLerping;

	bool touchingEnemy;
	bool hitStun;
	float dirHit;

	public BoxCollider2D col;
	RaycastOrigins raycastOrigins;
	public collisionInfo collisions;

	// Use this for initialization
	void Start () {
		col = GetComponent<BoxCollider2D>();
		CalculateRaySpacing ();
		camPos = new Vector3(0,0,-10);
		if (Checkpoint.checkpointReached) {
			transform.position = Checkpoint.checkpointPos;
			camPos.x = Checkpoint.checkpointPos.x;
			cam.transform.position = camPos;
			backgroundPos.x = 0.75f * Checkpoint.checkpointPos.x;
			background.transform.position = backgroundPos;
		}
		camSize = cam.orthographicSize;
		health = 5;
	}

	public void Move(Vector3 velocity) {
		Debug.Log (cam.transform.position);
		collisions.Reset ();
		UpdateRaycastOrigins ();
		collisions.velocityOld = velocity;

		//implement change in slope velocity here; should be after setting lightCounter variable, before translation (including altered translation for slope upward movement)

		if (velocity.y < 0) {
			DescendSlope (ref velocity);
		}

		if (velocity.x != 0)
			HorizontalCollisions (ref velocity);

		if (velocity.y != 0)
			VerticalCollisions (ref velocity);

		transform.Translate (velocity);
//		Debug.Log (transform.position.x);
		if (transform.position.x > 0 || camPos.x > 0) {
			camPos.x += velocity.x;
			if (camPos.x < 0) {
				camPos.x = 0;
			}
//			camPosLight.x = camPos.x + Player.lightCounter;
			camPos.x += 1.5f * Player.lightCounter;
			camPos.x += 0.75f * Player.velocityCamVar;
			cam.transform.position = camPos;
			camPos.x -= 1.5f * Player.lightCounter;
			camPos.x -= 0.75f * Player.velocityCamVar;
			backgroundPos.x += 0.75f * velocity.x;
			if (background.transform.position.x < 0) {
				backgroundPos.x = 0;
			}
			if (transform.position.x < 0) {
				backgroundPos.x = 0;
			}


			//current issue is that background snaps back when running back to start

			backgroundPos.x += 1.5f * 0.75f * Player.lightCounter;
			backgroundPos.x += 0.75f * 0.75f * Player.velocityCamVar;
			background.transform.position = backgroundPos;
			backgroundPos.x -= 1.5f * 0.75f * Player.lightCounter;
			backgroundPos.x -= 0.75f * 0.75f * Player.velocityCamVar;

			if (transform.position.x <= 0) {
				backgroundPos.x = 0;
				background.transform.position = backgroundPos;
			}
		}
		if (cam.transform.position.x < 0) {
			camPos.x = 0;
			cam.transform.position = camPos;
		}
		if (background.transform.position.x < 0) {
			backgroundPos.x = 0;
			background.transform.position = backgroundPos;
		}
		if (transform.position.x < 0) {
			backgroundPos.x = 0;
			background.transform.position = backgroundPos;
//			backgroundLerping = true;
//			StartCoroutine (backgroundLerpBack ());
		}
		cam.orthographicSize = camSize + Mathf.Abs(Player.lightCounter);
	}

	void HorizontalCollisions(ref Vector3 velocity) {
		float directionX = Mathf.Sign (velocity.x);
		float rayLength = Mathf.Abs (velocity.x) + skinWidth;

		for (int i = 0; i < horizontalRayCount; i++) {
			Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);

			RaycastHit2D hitSlope = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, slopeMask);

			if (hitSlope) {
				float slopeAngle = Vector2.Angle (hitSlope.normal, Vector2.up);
//				Debug.Log ("Hitting slope! " + slopeAngle + " " + i);
				if (i == 0 && slopeAngle <= maxClimbAngle) {
					//ensures that downward slopes can transition smoothly into upward slopes
					if (collisions.descendingSlope) {
						collisions.descendingSlope = false;
						velocity = collisions.velocityOld;
					}
					float distanceToSlopeStart = 0;
					if (slopeAngle != collisions.slopeAngleOld) {
						distanceToSlopeStart = hitSlope.distance - skinWidth;
						velocity.x -= distanceToSlopeStart * directionX;
					}
					ClimbSlope (ref velocity, slopeAngle);
					velocity.x += distanceToSlopeStart * directionX;
				} 

				//this is how I had solved the "clipping and stopping upon reaching a new slope" issue before the tutorial covered the more elegant solution in VerticalCollisions
//				else if (i == 0 && slopeAngle > maxClimbAngle) {
//					collisions.slopeAngle = slopeAngle;
//				}
//				if (collisions.slopeAngle > maxClimbAngle && slopeAngle <= maxClimbAngle && i > 0) {
//					ClimbSlope (ref velocity, slopeAngle);
//				}
				//				velocity.x = (hit.distance - skinWidth) * directionX;

				if (slopeAngle > maxClimbAngle) {
					velocity.x = (hitSlope.distance - skinWidth) * directionX;
					if (directionX == -1)
						collisions.left = true;
					if (directionX == 1)
						collisions.right = true;
				}
			}

			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

			Debug.DrawRay (rayOrigin, Vector2.right * directionX * rayLength, Color.red);

			if (hit && (!collisions.climbingSlope || collisions.slopeAngle > maxClimbAngle)) {


//				float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);
//
//				if (i == 0 && slopeAngle <= maxClimbAngle) {
//					ClimbSlope (ref velocity, slopeAngle);
//				}

				velocity.x = (hit.distance - skinWidth) * directionX;


				//this means that another raycast in the loop can't accidentally hit a further object than the one in this instance
				rayLength = hit.distance;

				if (collisions.climbingSlope) {
					velocity.y = Mathf.Tan (collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs (velocity.x);
				}

				if (directionX == -1)
					collisions.left = true;
				if (directionX == 1)
					collisions.right = true;
			}



			RaycastHit2D hitEnemy = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, enemyCollisionMask);

			if (hitEnemy && Player.isInvuln == false) {
				velocity.x = -6 * Mathf.Sign(velocity.x) * Time.deltaTime;

				rayLength = hit.distance;

				if (directionX == -1)
					collisions.left = true;
				if (directionX == 1)
					collisions.right = true;

				gameObject.SendMessage ("playerHit");
			}
		}
	}

	void stationaryHitEnemy(float dirEnemy) {
		
		if (Player.isInvuln == false) {
			if (Mathf.Sign (dirEnemy) == -1)
				collisions.left = true;
			if (Mathf.Sign (dirEnemy) == 1)
				collisions.right = true;

			Player.isInvuln = true;
			gameObject.SendMessage ("playerHit");
		}
	}



	void VerticalCollisions(ref Vector3 velocity) {
		float directionY = Mathf.Sign (velocity.y);
		float rayLength = Mathf.Abs (velocity.y) + skinWidth;

		for (int i = 0; i < verticalRayCount; i++) {
			Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
			RaycastHit2D hitSlope = Physics2D.Raycast (rayOrigin, Vector2.up * directionY, rayLength, slopeMask);

			Debug.DrawRay (rayOrigin, Vector2.up * directionY * rayLength, Color.red);
			if (hitSlope) {
				float slopeAngle = Vector2.Angle (hitSlope.normal, Vector2.up);



				//this means that another raycast in the loop can't accidentally hit a further object than the one in this instance
				rayLength = hitSlope.distance;
				if (slopeAngle <= maxDescendAngle) {
					velocity.y = (hitSlope.distance - skinWidth) * directionY;
					if (directionY == -1)
						collisions.below = true;
					if (directionY == 1)
						collisions.above = true;
				} else if (slopeAngle > maxDescendAngle) {
					float moveDistance = Mathf.Abs (velocity.y);
					velocity.x = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (hitSlope.normal.x);
					velocity.y = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (velocity.y);
				}
			}
			if (hit) {
				velocity.y = (hit.distance - skinWidth) * directionY;

				//this means that another raycast in the loop can't accidentally hit a further object than the one in this instance
				rayLength = hit.distance;

				if (collisions.climbingSlope) {
					Debug.Log (collisions.slopeAngle);
					velocity.x = velocity.y / Mathf.Tan (collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign (velocity.x);
				}

				if (directionY == -1)
					collisions.below = true;
				if (directionY == 1)
					collisions.above = true;
			}

			RaycastHit2D hitEnemy = Physics2D.Raycast (rayOrigin, Vector2.up * directionY, rayLength, enemyHurtboxMask);

			if (hitEnemy && directionY == -1) {
				velocity.y = (hit.distance - skinWidth) * directionY;
				gameObject.SendMessage ("bounceOnEnemy");
				hitEnemy.collider.gameObject.SendMessageUpwards ("Death");
			}
		}

		//if climbing slope, sends out a horizontal raycast from base (except velocity.y higher)
		//in order to determine slope of land it WILL be on to avoid accidentally clipping into land
		//on changes in slope
		if (collisions.climbingSlope) {
			float directionX = Mathf.Sign (velocity.x);
			rayLength = Mathf.Abs (velocity.x) + skinWidth;
			Vector2 rayOrigin;
			if (directionX == -1)
				rayOrigin = raycastOrigins.bottomLeft + Vector2.up * velocity.y;
			else
				rayOrigin = raycastOrigins.bottomRight + Vector2.up * velocity.y;
			RaycastHit2D hitSlope = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, slopeMask);

			if (hitSlope) {
				float slopeAngle = Vector2.Angle (hitSlope.normal, Vector2.up);
				if (slopeAngle != collisions.slopeAngle) {
					velocity.x = (hitSlope.distance - skinWidth) * directionX;
					collisions.slopeAngle = slopeAngle;
				}
			}
		}
	}

	void UpdateRaycastOrigins() {
		Bounds bounds = col.bounds;
		bounds.Expand (skinWidth * -2);

		raycastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
	}

	void ClimbSlope(ref Vector3 velocity, float slopeAngle) {
//		Debug.Log ("Climbing! " + velocity.x + " " + velocity.y + " " + slopeAngle);
		float moveDistance = Mathf.Abs (velocity.x);
		float climbVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;

		if (velocity.y <= climbVelocityY) {
			velocity.y = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;
//			if (collisions.slopeAngle > maxClimbAngle) {
//				transform.Translate(new Vector3(0, Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance, 0));
//			}
			velocity.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (velocity.x);
			collisions.below = true;
			collisions.climbingSlope = true;
			collisions.slopeAngle = slopeAngle;
		}
	}

	void DescendSlope(ref Vector3 velocity) {
		float directionX = Mathf.Sign (velocity.x);
		Vector2 rayOrigin;
		if (directionX == -1)
			rayOrigin = raycastOrigins.bottomRight;
		else
			rayOrigin = raycastOrigins.bottomLeft;
		RaycastHit2D hitSlope = Physics2D.Raycast (rayOrigin, -Vector2.up, Mathf.Infinity, slopeMask);

		if (hitSlope) {
			float slopeAngle = Vector2.Angle (hitSlope.normal, Vector2.up);
			if (slopeAngle != 0 && slopeAngle <= maxDescendAngle) {
				//if the distance to the slope is less than how far we have to move on the Y axis, based on the slope angle and our X velocity, then we are close enough to the slope for it to come into effect.
				if (Mathf.Sign (hitSlope.normal.x) == directionX) {
					if (hitSlope.distance - skinWidth <= Mathf.Tan (slopeAngle * Mathf.Deg2Rad) * Mathf.Abs (velocity.x)) {
						float moveDistance = Mathf.Abs (velocity.x);
						float descendVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;
						velocity.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (velocity.x);
						velocity.y -= descendVelocityY;

						collisions.slopeAngle = slopeAngle;
						collisions.descendingSlope = true;
						collisions.below = true;
					}
				}
			}
		}
	}

	void CalculateRaySpacing() {
		Bounds bounds = col.bounds;
		bounds.Expand (skinWidth * -2);

		horizontalRayCount = Mathf.Clamp (horizontalRayCount, 2, int.MaxValue);
		verticalRayCount = Mathf.Clamp (verticalRayCount, 2, int.MaxValue);

		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}

	struct RaycastOrigins {
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}

	public struct collisionInfo {
		public bool above;
		public bool below;
		public bool left;
		public bool right;

		public bool climbingSlope;
		public bool descendingSlope;
		public float slopeAngle, slopeAngleOld;

		public Vector3 velocityOld;

		public void Reset() {
			above = false;
			below = false;
			left = false;
			right = false;
			climbingSlope = false;
			descendingSlope = false;

			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}
}
