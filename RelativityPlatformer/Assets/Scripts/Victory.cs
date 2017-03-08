using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Victory : MonoBehaviour {

	public bool endReached;
	public Vector3 endPos;
	public GameObject victoryScreen;

	// Use this for initialization
	void Start () {
		victoryScreen.SetActive (false);
		endPos.y = 0;
		endPos.z = 0;
	}

	void OnTriggerEnter2D(Collider2D col) {
		Debug.Log (col.tag);
		if (col.tag == "Player") {
			victoryScreen.SetActive (true);
			Debug.Log ("Victory!");
			endPos.x = transform.position.x;
			endReached = true;
		}
	}

	void restart () {
		Checkpoint.checkpointReached = false;
		Controller2D.lives = 5;
		SceneManager.LoadScene ("Main");
	}
}
