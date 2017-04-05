using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier : MonoBehaviour {

	public ParticleSystem particles;
	public BoxCollider2D col;

	float emissionRate;
	Color partAlphaStorage;
	float startRed;
	float startGreen;
	float startBlue;

	Vector3 pos;
	Vector3 startingPos;
	int direction;
	public float speed;

	// Use this for initialization
	void Start () {
		emissionRate = particles.emission.rateOverTime.constant;
		startRed = particles.startColor.r;
		startGreen = particles.startColor.g;
		startBlue = particles.startColor.b;
		partAlphaStorage.r = startRed;
		partAlphaStorage.g = startGreen;
		partAlphaStorage.b = startBlue;
		partAlphaStorage.a = 1;

		pos.x = transform.position.x;
		pos.y = transform.position.y;
		pos.z = transform.position.z;
		startingPos.x = transform.position.x;
		startingPos.y = transform.position.y;
		startingPos.z = transform.position.z;
		direction = -1;
		speed = 100;
	}
	
	// Update is called once per frame
	void Update () {
		if (Mathf.Abs (Player.lightCounter) > 0) {
			col.enabled = false;
			var em = particles.emission;
			em.rateOverTime = emissionRate - (Mathf.Abs(Player.lightCounter) * 0.75f * emissionRate);
			partAlphaStorage.a = 1 - (Mathf.Abs (Player.lightCounter) / 6);
			partAlphaStorage.r = 1 - (Mathf.Abs (Player.lightCounter) / 6);
			partAlphaStorage.g = 1;
			partAlphaStorage.b = 1 - (Mathf.Abs (Player.lightCounter) / 6);
		} else {
			col.enabled = true;
			var em = particles.emission;
			em.rateOverTime = emissionRate;
			partAlphaStorage.a = 1;
			partAlphaStorage.r = startRed;
			partAlphaStorage.g = startGreen;
			partAlphaStorage.b = startBlue;
		}
		particles.startColor = partAlphaStorage;
//		if (Mathf.Abs (Player.xPos - startingPos.x) > 30 && Mathf.Abs (Player.xPos - transform.position.x) > 30) {
//			transform.position = startingPos;
//			direction = -1;
//		} else {
//			pos.y += speed * direction * Time.deltaTime / (1 + (Player.lightCounter * 10));
//			if (pos.y >= 10) {
//				Debug.Log ("Changing direction!");
//				direction = -1;
//			}
//			if (pos.y <= -10) {
//
//				Debug.Log ("Changing direction!");
//				direction = 1;
//			}
//			transform.position = pos;
//		}
	}
}
