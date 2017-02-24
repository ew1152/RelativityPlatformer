using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

	//NOTE: currently having camera zoom out/pan in direction of movement before
	//reaching max velocity by increasing lightCounter starting at 3/4 of maxSpeed;
	//however, this is not ideal, as you reach "light speed" too quickly, and the 
	//immediate camera movement paired with still-increasing velocity is a bit too jarring.
	//You can fix this in a number of ways:
	//1. Create a separate static variable that increments with speed, probably starting
	//at 1/2 of maxSpeed, and implement the camera effects using this variable (likely in
	//addition to, not in replacement of, what you have now).
	//2. Decrease moveAccel. You may want to do this anyways, just as a balancing thing.
	//(Side note: may want to also decrease maxSpeed, just in general)

	public float maxJumpHeight = 4;
	public float jumpApexTime = .4f;
	float maxFallSpeed;

	public float moveSpeed;
	public float moveAccel;
	float startingSpeed = 6;
	float maxSpeed = 30;
	public static float lightCounter;
	public static float velocityCamVar;
	float maxVelocityCamVar;
	float maxLight = 3;
	float gravity;
	float jumpVel;
	public static Vector3 velocity;

	public static bool isInvuln;
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
		jumpVel = -gravity * jumpApexTime;
		maxFallSpeed = jumpVel * 2;

		maxVelocityCamVar = 1.52f;
	}
	
	// Update is called once per frame
	void Update () {
		if (transform.position.y < -8) {
			Controller2D.health = 0;
			isInvuln = false;
			SceneManager.LoadScene ("Main");
		}
//		Debug.Log (velocityCamVar);
//		Debug.Log(moveSpeed + " " + velocity.x);

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
			//****
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
			//****
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

		if (transform.position.x > 0) {
			if (moveSpeed > maxSpeed / 2 && velocityCamVar < maxVelocityCamVar) {
				if (input.x > 0)
					velocityCamVar += Time.deltaTime;
				else if (input.x < 0)
					velocityCamVar -= 3 * Time.deltaTime;
				else
					velocityCamVar -= 2 * Time.deltaTime;
			} 
//			else if (moveSpeed > maxSpeed / 4 && moveSpeed < maxSpeed) {
//				if (input.x > 0)
//					velocityCamVar += Time.deltaTime / 2;
//				else if (input.x < 0)
//					velocityCamVar -= 3 * Time.deltaTime;
//				else
//					velocityCamVar -= 2 * Time.deltaTime;
//			}
			if (moveSpeed < -maxSpeed / 2 && velocityCamVar > -maxVelocityCamVar) {
				if (input.x < 0)
					velocityCamVar -= Time.deltaTime;
				else if (input.x > 0)
					velocityCamVar += 3 * Time.deltaTime;
				else
					velocityCamVar += 2 * Time.deltaTime;
			} 
//			else if (moveSpeed < -maxSpeed / 4 && moveSpeed > -maxSpeed) {
//				if (input.x < 0)
//					velocityCamVar -= Time.deltaTime / 2;
//				else if (input.x > 0)
//					velocityCamVar += 3 * Time.deltaTime;
//				else
//					velocityCamVar += 2 * Time.deltaTime;
//			}
			else if (moveSpeed >= -maxSpeed / 2 && moveSpeed <= maxSpeed / 2) {
				if (velocityCamVar < -Time.deltaTime)
					velocityCamVar += 2 * Time.deltaTime;
				else if (velocityCamVar > Time.deltaTime)
					velocityCamVar -= 2 * Time.deltaTime;
				else
					velocityCamVar = 0;
			}
		} else {
			velocityCamVar = 0;
		}


		if (Input.GetKeyDown (KeyCode.Space) && controller.collisions.below && !controller.collisions.above) {
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

	void bounceOnEnemy() {
		gravity = -(2 * maxJumpHeight) / (jumpApexTime * jumpApexTime);
		velocity.y = jumpVel / 2;
		if (Input.GetKey (KeyCode.Space)) {
			velocity.y = jumpVel;
		}
	}
		

	public IEnumerator playerHit() {
		isInvuln = true;
		Color alphaStorage;
		alphaStorage.r = gameObject.GetComponent<SpriteRenderer>().color.r;
		alphaStorage.g = gameObject.GetComponent<SpriteRenderer>().color.g;
		alphaStorage.b = gameObject.GetComponent<SpriteRenderer>().color.b;
		alphaStorage.a = 1;
		Controller2D.health -= 1;
		if (Controller2D.health <= 0) {
			isInvuln = false;
			SceneManager.LoadScene ("Main");
		}
		float invulnTimer = 0;
		while (invulnTimer < 0.75f) {
			invulnTimer += Time.deltaTime;
			if (invulnTimer % 0.25f > 0.125f) {
				alphaStorage.a = 0;
				gameObject.GetComponent<SpriteRenderer> ().color = alphaStorage;
			} else {
				alphaStorage.a = 1;
				gameObject.GetComponent<SpriteRenderer> ().color = alphaStorage;
			}
			yield return null;
		}
		while (invulnTimer < 1.5f) {
			invulnTimer += Time.deltaTime;
			if (invulnTimer % 0.125f > 0.0625f) {
				alphaStorage.a = 0;
				gameObject.GetComponent<SpriteRenderer> ().color = alphaStorage;
			} else {
				alphaStorage.a = 1;
				gameObject.GetComponent<SpriteRenderer> ().color = alphaStorage;
			}
			yield return null;
		}
		while (invulnTimer < 2f) {
			invulnTimer += Time.deltaTime;
			if (invulnTimer % 0.0625f > 0.03125f) {
				alphaStorage.a = 0;
				gameObject.GetComponent<SpriteRenderer> ().color = alphaStorage;
			} else {
				alphaStorage.a = 1;
				gameObject.GetComponent<SpriteRenderer> ().color = alphaStorage;
			}
			yield return null;
		}
		alphaStorage.a = 1;
		gameObject.GetComponent<SpriteRenderer> ().color = alphaStorage;
		isInvuln = false;
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
