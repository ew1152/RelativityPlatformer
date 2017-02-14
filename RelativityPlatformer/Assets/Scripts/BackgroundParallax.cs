using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundParallax : MonoBehaviour {

	Vector3 startingPos;
	float moveX;
	Vector3 paraPosition;

	// Use this for initialization
	void Start () {
		startingPos = transform.position;
		paraPosition = transform.position;
		moveX = 0;
	}
	
	// Update is called once per frame
	void Update () {
		paraPosition.x = moveX * 0.75f + startingPos.x;
		transform.position = paraPosition;
	}
}
