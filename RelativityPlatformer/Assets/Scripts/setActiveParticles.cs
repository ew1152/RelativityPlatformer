using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class setActiveParticles : MonoBehaviour {

	public ParticleSystem linesLeft;
	public ParticleSystem linesRight;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Player.runningRight) {
			linesRight.enableEmission = true;
		} else {
			linesRight.enableEmission = false;
		}
		if (Player.runningLeft) {
			linesLeft.enableEmission = true;
		} else {
			linesLeft.enableEmission = false;
		}
	}
}
