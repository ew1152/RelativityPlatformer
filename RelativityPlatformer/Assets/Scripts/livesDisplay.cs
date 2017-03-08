using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class livesDisplay : MonoBehaviour {

	// Use this for initialization
	void Start () {
		this.transform.GetComponent<UnityEngine.UI.Text> ().text = "Lives: " + Controller2D.lives;
	}
	
	// Update is called once per frame
	void Update () {
		this.transform.GetComponent<UnityEngine.UI.Text> ().text = "Lives: " + Controller2D.lives;
	}
}
