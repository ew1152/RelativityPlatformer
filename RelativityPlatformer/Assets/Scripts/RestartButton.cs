using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RestartButton : MonoBehaviour {

	public GameObject player;

	public void Restart () {
		player.SendMessage ("Restart");
	}
		
}
