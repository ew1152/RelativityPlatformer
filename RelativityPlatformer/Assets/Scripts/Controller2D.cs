using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (BoxCollider2D))]
public class Controller2D : MonoBehaviour {

	public LayerMask collisionMask;
	public LayerMask enemyCollisionMask;

	const float skinWidth = .02f;
	public int horizontalRayCount = 4;
	public int verticalRayCount = 4;

	float horizontalRaySpacing;
	float verticalRaySpacing;

	public Camera cam;
	float camSize;
	Vector3 camPos;

	public GameObject background;
	Vector3 backgroundPos;
	bool backgroundLerping;

	public BoxCollider2D col;
	RaycastOrigins raycastOrigins;
	public collisionInfo collisions;

	// Use this for initialization
	void Start () {
		col = GetComponent<BoxCollider2D>();
		CalculateRaySpacing ();
		camPos = new Vector3(0,0,-10);
		camSize = cam.orthographicSize;
	}


	public void Move(Vector3 velocity) {
		collisions.Reset ();
		UpdateRaycastOrigins ();
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
			cam.transform.position = camPos;
			camPos.x -= 1.5f * Player.lightCounter;
			backgroundPos.x += 0.75f * velocity.x;
			if (background.transform.position.x < 0) {
				backgroundPos.x = 0;
			}
			if (transform.position.x < 0) {
				backgroundPos.x = 0;
			}


			//current issue is that background snaps back when running back to start

			backgroundPos.x += 1.5f * 0.75f * Player.lightCounter;
			background.transform.position = backgroundPos;
			backgroundPos.x -= 1.5f * 0.75f * Player.lightCounter;
			if (transform.position.x < 0) {
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

	public IEnumerator backgroundLerpBack() {
		float lerpRate = 0.01f;
		while (transform.position.x < 0) {
			backgroundPos.x = Mathf.Lerp (backgroundPos.x, 0, lerpRate);
			background.transform.position = backgroundPos;
			lerpRate += 0.01f;
			yield return null;
		}
		backgroundLerping = false;
	}

	void HorizontalCollisions(ref Vector3 velocity) {
		float directionX = Mathf.Sign (velocity.x);
		float rayLength = Mathf.Abs (velocity.x) + skinWidth;

		for (int i = 0; i < horizontalRayCount; i++) {
			Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

			Debug.DrawRay (rayOrigin, Vector2.right * directionX * rayLength, Color.red);

			if (hit) {
				velocity.x = (hit.distance - skinWidth) * directionX;

				//this means that another raycast in the loop can't accidentally hit a further object than the one in this instance
				rayLength = hit.distance;

				if (directionX == -1)
					collisions.left = true;
				if (directionX == 1)
					collisions.right = true;
			}

			RaycastHit2D hitEnemy = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, enemyCollisionMask);

			if (hitEnemy) {
				velocity.x = 6 * Mathf.Sign(velocity.x) * Time.deltaTime;

				rayLength = hit.distance;

				if (directionX == -1)
					collisions.left = true;
				if (directionX == 1)
					collisions.right = true;
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
}
