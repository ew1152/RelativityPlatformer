using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slope : MonoBehaviour {

	float slope;

	// Use this for initialization
	void Start () {
		PolygonCollider2D col = GetComponent<PolygonCollider2D> ();
		Bounds bounds = col.bounds;

	}
	
	// Update is called once per frame
	void Update () {
		
	}


}
