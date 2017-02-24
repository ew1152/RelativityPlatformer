using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy1 : MonoBehaviour {

	public LayerMask collisionMask;
	public LayerMask slopeMask;
	public LayerMask playerMask;

	const float skinWidth = .02f;
	public int horizontalRayCount = 4;
	public int verticalRayCount = 4;

	float horizontalRaySpacing;
	float verticalRaySpacing;

	float gravity;
	float moveSpeed;
	bool movingLeft;
	float direction;
	bool canTurn;

	Color enemyShading;

	public BoxCollider2D col;
	RaycastOrigins raycastOrigins;
	public collisionInfo collisions;

	public Vector3 velocity;

	// Use this for initialization
	void Start () {
		canTurn = true;
		col = GetComponent<BoxCollider2D>();
		CalculateRaySpacing ();
		gravity = -50;
		moveSpeed = 5;
		movingLeft = true;
		direction = -1;
		enemyShading.r = 1;
		enemyShading.g = 1;
		enemyShading.b = 1;
		enemyShading.a = 1;
	}


//	void OnTriggerEnter2D(Collider2D c) {
//		if (c.gameObject.layer == LayerMask.NameToLayer("Player")) {
//			Debug.Log ("It's a collision!");
//			c.gameObject.SendMessage ("stationaryHitEnemy", velocity.x);
//		}
//	}

	public void Update() {
		if (collisions.below || collisions.above)
			velocity.y = 0;
		if (collisions.left)
			direction = 1;
		if (collisions.right)
			direction = -1;

		velocity.x = moveSpeed / (1 + Mathf.Abs(Player.lightCounter * 0.75f)) * direction;
		velocity.y += gravity * Time.deltaTime;

		Move (velocity * Time.deltaTime);

	}
		
	void Move(Vector3 velocity) {
		collisions.Reset ();
		UpdateRaycastOrigins ();

		if (velocity.x != 0)
			HorizontalCollisions (ref velocity);

		if (velocity.y != 0)
			VerticalCollisions (ref velocity);

		enemyShading.r = 1 - Mathf.Abs(0.1f * Player.lightCounter);
		enemyShading.g = 1 - Mathf.Abs(0.1f * Player.lightCounter);
		enemyShading.b = 1 - Mathf.Abs(0.1f * Player.lightCounter);
		gameObject.GetComponent<SpriteRenderer>().color = enemyShading;

		transform.Translate (velocity);
	}

	void HorizontalCollisions(ref Vector3 velocity) {
		float directionX = Mathf.Sign (velocity.x);
		float rayLength = Mathf.Abs (velocity.x) + skinWidth;

		for (int i = 0; i < horizontalRayCount; i++) {
			Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
			RaycastHit2D hitSlope = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, slopeMask);

			Debug.DrawRay (rayOrigin, Vector2.right * directionX * rayLength, Color.red);

			if (hit || hitSlope) {
				velocity.x = (hit.distance - skinWidth) * directionX;

				//this means that another raycast in the loop can't accidentally hit a further object than the one in this instance
				rayLength = hit.distance;

				if (directionX == -1)
					collisions.left = true;
				if (directionX == 1)
					collisions.right = true;
			}

			RaycastHit2D hitPlayer = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, playerMask);

			if (hitPlayer) {
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

			if (!hit && !hitSlope && canTurn) {
				if (direction == -1)
					collisions.left = true;
				if (direction == 1)
					collisions.right = true;
				StartCoroutine ("turnTimer");
			}

		}
	}

	IEnumerator turnTimer() {
		canTurn = false;
		yield return new WaitForSeconds (0.2f);
		canTurn = true;
	}

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

		public void Reset() {
			above = false;
			below = false;
			left = false;
			right = false;
		}
	}

	void Death() {
		Destroy (gameObject);
	}
		
}
