using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier : MonoBehaviour {

	Vector3 pos;
	Vector3 startingPos;
	int direction;
	public float speed;

	// Use this for initialization
	void Start () {
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
		if (Mathf.Abs (Player.xPos - startingPos.x) > 30 && Mathf.Abs (Player.xPos - transform.position.x) > 30) {
			transform.position = startingPos;
			direction = -1;
		} else {
			pos.y += speed * direction * Time.deltaTime / (1 + (Player.lightCounter * 10));
			if (pos.y >= 10) {
				Debug.Log ("Changing direction!");
				direction = -1;
			}
			if (pos.y <= -10) {

				Debug.Log ("Changing direction!");
				direction = 1;
			}
			transform.position = pos;
		}
	}
}
