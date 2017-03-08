using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictoryButton : MonoBehaviour {

	public GameObject checkpoint;

	public void Restart () {
		checkpoint.SendMessage ("restart");
	}
}
