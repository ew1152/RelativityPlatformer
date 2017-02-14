using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {

	public GameObject player;
	public Vector3 position;

	// Use this for initialization
	void Start () {
		position.x = 0;
		position.y = 0;
		position.z = -10;
	}
	
	// Update is called once per frame
	void Update () {
		Debug.Log (position.x + " " + player.transform.position.x);
		if (player.transform.position.x < 0) {
			position.x = 0;
		} else {
			position.x = player.transform.position.x;
		}
		transform.position = position;
	}
}
