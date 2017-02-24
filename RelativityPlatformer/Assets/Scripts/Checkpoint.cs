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

	void OnTriggerEnter2D(Collider2D col) {
		Debug.Log (col.tag);
		if (col.tag == "Player") {
			Debug.Log ("Saving!");
			checkpointPos.x = transform.position.x;
			checkpointReached = true;
		}
	}
}
