using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transparency : MonoBehaviour {

	float tranLevel;
	SpriteRenderer sprite;
	Color origColor;
	Color color;

	// Use this for initialization
	void Start () {
		tranLevel = 1;
		sprite = GetComponent<SpriteRenderer> ();
		origColor = new Color (sprite.color.r, sprite.color.g, sprite.color.b, sprite.color.a);
		color = origColor;
	}
	
	// Update is called once per frame
	void Update () {
		if (Player.lightCounter != 0) {
			tranLevel = (3 - Mathf.Abs(Player.lightCounter)) / 3;
//			Debug.Log (tranLevel);
			color.a = tranLevel;
			sprite.color = color;
			//		foreach (GameObject child in transform) {
			//			SpriteRenderer sprite = child.GetComponent<SpriteRenderer>();
			//			Color color = new Color (0, 0, 0, tranLevel);
			//			sprite.color = color;
			//		}
		} else {
			color = origColor;
			sprite.color = color;
		}
	}
}
