using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class velocityBar : MonoBehaviour {

	public GameObject player;
	Vector2 scale;
	float width;
	float height;
	Vector2 pos;
	float posX;
	float posY;

	// Use this for initialization
	void Start () {
		width = gameObject.GetComponent<Image> ().rectTransform.sizeDelta.x;
		height = gameObject.GetComponent<Image> ().rectTransform.sizeDelta.y;
		scale.y = height;
		posX = gameObject.GetComponent<Image> ().rectTransform.anchoredPosition.x;
		posY = gameObject.GetComponent<Image> ().rectTransform.anchoredPosition.y;
		pos.y = posY;
	}

	// Update is called once per frame
	void Update () {
		scale.x = Mathf.Abs(player.GetComponent<Player> ().moveSpeed / player.GetComponent<Player>().maxSpeed) * width;
		pos.x = posX + (0.5f * (scale.x)) - (0.5f * width);
		gameObject.GetComponent<Image> ().rectTransform.sizeDelta = scale;
		gameObject.GetComponent<Image> ().rectTransform.anchoredPosition = pos;
	}
}
