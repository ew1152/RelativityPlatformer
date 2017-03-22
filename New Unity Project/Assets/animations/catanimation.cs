using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class catanimation : MonoBehaviour {

	Animator catanimator;

	// Use this for initialization
	void Start () {
		catanimator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetAxis ("Horizontal") != 0) {
			catanimator.Play ("cat_walk");
		} else {
			catanimator.Play ("cat_idle");
		}
	}
}
