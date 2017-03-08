using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy2 : MonoBehaviour {

	public LayerMask collisionMask;
	public LayerMask slopeMask;
	public LayerMask playerMask;
	public LayerMask blockMask;

	const float skinWidth = .02f;
	public int horizontalRayCount = 4;
	public int verticalRayCount = 4;

	float horizontalRaySpacing;
	float verticalRaySpacing;

	bool onSlope;
	//the maximum angle (in degrees) of a slope that can be climbed by a player.
	float maxClimbAngle = 70;
	//the maximum angle of a slope that can be descended by a player (a player can still "go down" a slope
	//that is steeper than this, but is not automatically connected to the ground while doing so.
	float maxDescendAngle = 60;

	float gravity;
	float moveSpeed;
	bool movingLeft;
	float direction;

	Vector3 startingPos;

	bool playerEffective;
	float vulnTimer;

	Color enemyShading;

	public BoxCollider2D col;
	RaycastOrigins raycastOrigins;
	public collisionInfo collisions;

	public Vector3 velocity;

	// Use this for initialization
	void Start () {
		vulnTimer = 0;
		playerEffective = false;
		col = GetComponent<BoxCollider2D>();
		CalculateRaySpacing ();
		gravity = -50;
		moveSpeed = 5;
		movingLeft = true;
		direction = -1;
		enemyShading.r = gameObject.GetComponent<SpriteRenderer>().color.r;
		enemyShading.g = gameObject.GetComponent<SpriteRenderer>().color.g;
		enemyShading.b = gameObject.GetComponent<SpriteRenderer>().color.b;
		enemyShading.a = 1;

		startingPos.x = transform.position.x;
		startingPos.y = transform.position.y;
		startingPos.z = transform.position.z;
	}


	public void Update() {
		if (Mathf.Abs (Player.xPos - startingPos.x) > 30 && Mathf.Abs (Player.xPos - transform.position.x) > 30) {
			transform.position = startingPos;
			direction = Mathf.Sign (Player.xPos - startingPos.x);
		} else {
			if (Mathf.Abs(Player.lightCounter) > 0) {
				playerEffective = true;
				Vulnerable ();
			} else {
				playerEffective = false;
				enemyShading.g = 0;
				enemyShading.a = 1;
				vulnTimer = 0;
				gameObject.GetComponent<SpriteRenderer>().color = enemyShading;
			}
			if (collisions.below || collisions.above)
				velocity.y = 0;
			if (collisions.left)
				direction = 1;
			if (collisions.right)
				direction = -1;

			velocity.x = moveSpeed / (1 + Mathf.Abs (Player.lightCounter * 0.75f)) * direction;
			velocity.y += gravity * Time.deltaTime;

			Move (velocity * Time.deltaTime);
		}

	}

	void Move(Vector3 velocity) {
		collisions.Reset ();
		UpdateRaycastOrigins ();

		if (velocity.x != 0)
			HorizontalCollisions (ref velocity);

		if (velocity.y != 0)
			VerticalCollisions (ref velocity);



		transform.Translate (velocity);
	}

	void Vulnerable() {
		enemyShading.g = Player.lightCounter/3;
		vulnTimer += 1;
		if (vulnTimer % 3 >= 2) {
			enemyShading.a = 1;
		} else {
			enemyShading.a = 0;
		}
		gameObject.GetComponent<SpriteRenderer>().color = enemyShading;
	}

	void HorizontalCollisions(ref Vector3 velocity) {
		float directionX = Mathf.Sign (velocity.x);
		float rayLength = Mathf.Abs (velocity.x) + skinWidth;

		for (int i = 0; i < horizontalRayCount; i++) {
			Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
			RaycastHit2D hitBlock = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, blockMask);
			RaycastHit2D hitSlope = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, slopeMask);

			Debug.DrawRay (rayOrigin, Vector2.right * directionX * rayLength, Color.red);

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

			if (hitBlock) {
				velocity.x = (hit.distance - skinWidth) * directionX;

				//this means that another raycast in the loop can't accidentally hit a further object than the one in this instance
				rayLength = hit.distance;

				if (directionX == -1)
					collisions.left = true;
				if (directionX == 1)
					collisions.right = true;
			}

			RaycastHit2D hitPlayer = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, playerMask);

			if (hitPlayer && !playerEffective) {
				Debug.Log ("Hitting player!");
				hitPlayer.collider.gameObject.SendMessage ("stationaryHitEnemy", velocity.x);
			}
		}
	}

	//ref extremely unnecessary, just change to independent variable if you combine the scripts
	void VerticalCollisions(ref Vector3 velocity) {
		float directionY = Mathf.Sign (velocity.y);
		float rayLength = Mathf.Abs (velocity.y) + skinWidth;

		for (int i = 0; i < verticalRayCount; i++) {
			Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
			RaycastHit2D hitSlope = Physics2D.Raycast (rayOrigin, Vector2.up * directionY, rayLength, slopeMask);
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

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
				if (directionY == -1)
					collisions.below = true;
				if (directionY == 1)
					collisions.above = true;
			}

			//makes sure enemies can turn on reaching ledge

			//			if (!hit && !hitSlope && canTurn) {
			//				if (direction == -1)
			//					collisions.left = true;
			//				if (direction == 1)
			//					collisions.right = true;
			//				StartCoroutine ("turnTimer");
			//			}

		}
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

	//	IEnumerator turnTimer() {
	//		canTurn = false;
	//		yield return new WaitForSeconds (0.2f);
	//		canTurn = true;
	//	}

	void UpdateRaycastOrigins() {
		Bounds bounds = col.bounds;
		bounds.Expand (skinWidth * -2);

		raycastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
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

	void Death() {
		Destroy (gameObject);
	}
}
