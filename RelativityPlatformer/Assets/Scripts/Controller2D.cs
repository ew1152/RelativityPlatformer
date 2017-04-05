using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (BoxCollider2D))]
public class Controller2D : MonoBehaviour {

	//layermask used for standard collisions with ground, walls, etc.
	public LayerMask collisionMask;
	//layermask used for collisions with slopes
	public LayerMask slopeMask;
	//layermask used when being damaged by enemy
	public LayerMask enemyCollisionMask;
	//layermask used when killing enemy
	public LayerMask enemyHurtboxMask;
	//layermask used when interacting with second enemy type
	public LayerMask enemyMask2;

	//amount of health the player has
	public static int health;
	//amount of lives the player has
	public static int lives = 5;

	//value used for small inset on bounds of player width/height from which raycasts are sent;
	//this is to minimize complications with raycasts originating exactly on their point of collision
	const float skinWidth = .02f;
	//number of raycasts that are sent out from the side of the player when moving; this allows control over how
	//thorough the process for collision detection needs to be; the more raycasts being sent, the thinner the
	//vertical distance between each raycast, and the thinner an obstacle would have to be to go undetected by
	//these raycasts.
	public int horizontalRayCount = 4;
	//same as for horizontalRayCount, but for vertical raycasts
	public int verticalRayCount = 4;
	//the distance between each horizontal raycast sent out from the player. This is calculated in CalculateRaySpacing()
	//based on the horizontalRayCount integer and the height of the player.
	float horizontalRaySpacing;
	//same as horizontalRaySpacing, but for vertical raycasts
	float verticalRaySpacing;

	//the maximum angle (in degrees) of a slope that can be climbed by a player.
	float maxClimbAngle = 70;
	//the maximum angle of a slope that can be descended by a player (a player can still "go down" a slope
	//that is steeper than this, but is not automatically connected to the ground while doing so.
	float maxDescendAngle = 60;

	//While this could technically be achieved using camera.main, I figured that storing the camera as an
	//object with a shorter name would be more convenient, considering how frequently I manipulate its position
	//and scaling.
	public Camera cam;
	//I use this float to store and manipulate the orthographicSize of the stored camera; this is used to
	//effectively "zoom out" when moving at high velocities, and then zoom back in when not.
	float camSize;
	//This Vector3 is used to store and manipulate the position of the main camera; this is primarily used for
	//tracking the camera to the position of the player after the player has moved past the starting position,
	//but is also used to shift the camera's position based on how fast the player is moving/in what direction
	Vector3 camPos;

	//This is the game object in which I store all sprites in the background of the scene; this is so that I
	//can move them all in parallax with the player, without having to add a script to each individual object.
	public GameObject background;
	Vector3 backgroundPos;

	//this is essentially the portion of the player which interacts with objects within the level; literally,
	//it sets the bounds from which raycasts originate, which are then what interacts with the environment.
	public BoxCollider2D col;
	//references a struct set at the end of the script, allowing easy access to Vector2 variables referencing
	//the four corners of the player's collider.
	RaycastOrigins raycastOrigins;
	//referencing another established struct which allows easy storage of the information that a player is colliding
	//with something to the left, right, top, or bottom; also used to store various information relating to slope
	//collisions
	public collisionInfo collisions;


	void Start () {
		//this is a bit recommended by the 2D platformer controller tutorial that I was using for reference;
		//it ensures that, whether you have assigned the player's box collider to the script within the inspector,
		//you will still always have it assigned.
		col = GetComponent<BoxCollider2D>();
		CalculateRaySpacing ();
		//sets the starting position of the camera
		camPos = new Vector3(0,0,-10);
		//References a static bool in the Checkpoint script to see if the player has reached a checkpoint; if so,
		//reloads the scene but starting the player and camera at the position of that checkpoint.
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
		//resets all collision info to false, so that collisions are only determined on a frame-by-frame basis
		collisions.Reset ();
		//sets the origins of the collision-detecting raycasts to the current (modified) bounds of the player's collider
		UpdateRaycastOrigins ();
		//
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

				if (hit.collider.tag == "Barrier" && !Player.isInvuln) {
					gameObject.SendMessage ("playerHit");
				}

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

			RaycastHit2D hitEnemy2 = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, enemyMask2);

			if (hitEnemy2 && Player.isInvuln == false) {
				rayLength = hit.distance;
				if (Mathf.Abs (Player.lightCounter) > 0) {
					hitEnemy2.collider.SendMessage ("Death");
					health += 1;
					gameObject.BroadcastMessage ("Gear");
				} else {
					
					if (directionX == -1)
						collisions.left = true;
					if (directionX == 1)
						collisions.right = true;

					gameObject.SendMessage ("playerHit");
				}
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

			RaycastHit2D hitEnemy2 = Physics2D.Raycast (rayOrigin, Vector2.up * directionY, rayLength, enemyMask2);

			if (hitEnemy2 && Player.isInvuln == false) {
				rayLength = hit.distance;
				if (Mathf.Abs (Player.lightCounter) > 0) {
//					velocity.y = (hit.distance - skinWidth) * directionY;
//					gameObject.SendMessage ("bounceOnEnemy");
					hitEnemy2.collider.gameObject.SendMessageUpwards ("Death");
					health += 1;
					gameObject.BroadcastMessage ("Gear");
				} else {

					velocity.y = (hit.distance - skinWidth) * directionY;
					gameObject.SendMessage ("bounceOnEnemy2");

					gameObject.SendMessage ("playerHit");
				}
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
