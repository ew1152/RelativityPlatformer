using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class healthDisplay : MonoBehaviour {

	public Sprite fullHeart;
	public Sprite emptyHeart;
	public int heartNum;

	// Use this for initialization
	void Start () {
//		heartNum = int.Parse(gameObject.name.Substring (6, 1));
	}
	
	// Update is called once per frame
	void Update () {
		if (Controller2D.health < heartNum)
			this.transform.GetComponent<UnityEngine.UI.Image>().sprite = emptyHeart;
		else
			this.transform.GetComponent<UnityEngine.UI.Image>().sprite = fullHeart;
	}
}
