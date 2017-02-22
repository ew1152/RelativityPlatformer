using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour {

	public static bool checkpointReached;
	public static Vector3 checkpointPos;

	// Use this for initialization
	void Start () {
		checkpointPos.y = 0;
		checkpointPos.z = 0;
	}

	void OnTriggerEnter(Collider col) {
		Debug.Log ("Colliding!");
		if (col.tag == "Player") {
			Debug.Log ("Saving!");
			checkpointPos.x = transform.position.x;
			checkpointReached = true;
		}
	}
}
