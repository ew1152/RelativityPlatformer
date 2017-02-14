using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

	public float maxJumpHeight = 4;
	public float jumpApexTime = .4f;
	float maxFallSpeed;

	public float moveSpeed;
	public float moveAccel;
	float startingSpeed = 6;
	float maxSpeed = 30;
	public static float lightCounter;
	float maxLight = 3;
	float gravity;
	float jumpVel;
	Vector3 velocity;

	bool hasJumped;
	bool right;
	bool left;
	public static bool runningRight;
	public static bool runningLeft;

	Controller2D controller;

	// Use this for initialization
	void Start () {
		moveAccel = 10;
		controller = GetComponent<Controller2D>();
		gravity = -(2 * maxJumpHeight) / (jumpApexTime * jumpApexTime);
		Debug.Log (gravity);
		jumpVel = -gravity * jumpApexTime;
		maxFallSpeed = jumpVel * 2;
	}
	
	// Update is called once per frame
	void Update () {

		//current bug: since 
		if (controller.collisions.right || controller.collisions.left)
			moveSpeed = 0;

		if (controller.collisions.above || controller.collisions.below)
			velocity.y = 0;



		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
//		if (input.x == 1 && velocity.x < maxSpeed) {
//			moveSpeed += moveAccel * Time.deltaTime;
//			speedCounter += Time.deltaTime;
//		} else if (input.x != 1 && input.x != -1 && velocity.x > 0) {
//			moveSpeed -= moveAccel * 2;
//		}
//		if (input.x == -1 && velocity.x > -maxSpeed) {
//			moveSpeed -= moveAccel * Time.deltaTime;
//			speedCounter += Time.deltaTime;
//		} else if (input.x != -1 && input.x != 1 && velocity.x < 0) {
//			moveSpeed += moveAccel * 2;
//		}
		if (input.x > 0) {
//			Debug.Log (moveSpeed + " " + moveAccel + " " + lightCounter);

			if (moveSpeed < 0) {
				moveSpeed += 4 * moveAccel * Time.deltaTime;

				lightReturnUp (4);
				lightReturnDown (4);
			}
			if (moveSpeed >= 0 && moveSpeed < startingSpeed) {
				moveSpeed += 2 * moveAccel * Time.deltaTime;
				lightReturnDown (2);
				lightReturnUp (3);
			}
			if (moveSpeed < maxSpeed && moveSpeed >= startingSpeed) {
				moveSpeed += moveAccel * Time.deltaTime;
				lightReturnUp (4);
				lightReturnDown (1);
			} else if (moveSpeed >= maxSpeed) {
				moveSpeed = maxSpeed;
				if (lightCounter < maxLight) {
					lightCounter += Time.deltaTime;
				} else {
					lightCounter = maxLight;
				}
			}
		}
		if (input.x < 0) {
//			Debug.Log (moveSpeed + " " + moveAccel + " " + lightCounter);
			if (moveSpeed > 0) {
				moveSpeed -= 4 * moveAccel * Time.deltaTime;
				lightReturnDown (4);
				lightReturnUp (4);
			}
			if (moveSpeed <= 0 && moveSpeed > -startingSpeed) {
				moveSpeed -= 2 * moveAccel * Time.deltaTime;
				if (lightCounter < 0) {
					lightCounter += 3 * Time.deltaTime;
					if (lightCounter > 0) {
						lightCounter = 0;
					}
				}
				lightReturnUp (2);
				lightReturnDown (3);
			}
			if (moveSpeed > -maxSpeed && moveSpeed <= -startingSpeed) {
				moveSpeed -= moveAccel * Time.deltaTime;
				lightReturnUp (1);
				lightReturnDown (4);
			} else if (moveSpeed <= -maxSpeed) {
				moveSpeed = -maxSpeed;
				if (lightCounter > -maxLight) {
					lightCounter -= Time.deltaTime;
				} else {
					lightCounter = -maxLight;
				}
			}
		}
		if (input.x == 0) {
//			Debug.Log (moveSpeed + " " + moveAccel + " " + lightCounter);
			if (moveSpeed < -0.5) {
				moveSpeed += 2 * moveAccel * Time.deltaTime;

			}
			if (moveSpeed > 0.5) {
				moveSpeed -= 2 * moveAccel * Time.deltaTime;

			}
			if (moveSpeed >= -0.5 && moveSpeed <= 0.5) {
				moveSpeed = 0;
			}
			lightReturnUp (2);
			lightReturnDown (2);
		}



		if (Input.GetKeyDown (KeyCode.Space) && controller.collisions.below) {
			velocity.y = jumpVel;
			hasJumped = true;
		}

//		if (controller.collisions.below)
//			hasJumped = false;

		if (!controller.collisions.below && hasJumped) {
			if (Input.GetKeyUp (KeyCode.Space) && velocity.y > 0) {
				gravity = -250;
			}
			if (velocity.y < 0) {
				gravity = -(2 * maxJumpHeight) / (jumpApexTime * jumpApexTime);
			}
		}

		//ensures that both aren't emitting simultaneously, but they are emitting before the background fades
		if ((moveSpeed > maxSpeed / 2 && lightCounter > -0.5) || lightCounter > 0) {
			runningRight = true;
		} else {
			runningRight = false;
		}
		if ((moveSpeed < -maxSpeed / 2 && lightCounter < 0.5) || lightCounter < 0) {
			runningLeft = true;
		} else {
			runningLeft = false;
		}

		velocity.x = moveSpeed;
		velocity.y += gravity * Time.deltaTime;
		controller.Move (velocity * Time.deltaTime);
//		Debug.Log (lightCounter + " " + moveSpeed + " " + input.x);
	}

	void lightReturnUp(float rate) {
		if (lightCounter < 0) {
			lightCounter += rate * Time.deltaTime;
			if (lightCounter > 0) {
				lightCounter = 0;
			}
		}
	}

	void lightReturnDown(float rate) {
		if (lightCounter > 0) {
			lightCounter -= rate * Time.deltaTime;
			if (lightCounter < 0) {
				lightCounter = 0;
			}
		}
	}
}
