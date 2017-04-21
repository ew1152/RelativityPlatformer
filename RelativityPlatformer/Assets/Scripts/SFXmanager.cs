using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SFXmanager : MonoBehaviour {

	public AudioMixer SFX;

	public AudioSource jumpSource;
	public AudioClip[] jumpClips;

	public AudioSource hitSource;
	public AudioClip[] hitClips;

	public AudioSource gearSource;
	public AudioClip[] gearClips;

	public AudioSource killSource;
	public AudioClip[] killClips;

	void Jump() {
		int randInt = Random.Range (0, 7);
		jumpSource.pitch = Random.Range (0.9f, 1f);
		jumpSource.PlayOneShot (jumpClips [randInt]);
	}

	void Jump2() {
		int randInt = Random.Range (0, 7);
		jumpSource.pitch = Random.Range (1.2f, 1.3f);
		jumpSource.PlayOneShot (jumpClips [randInt]);
	}

	void Gear() {
		int randInt = Random.Range (0, 3);
		gearSource.pitch = 0.8f;
		gearSource.volume = Random.Range (0.8f, 1f);
		if (gearSource.isPlaying) {
			gearSource.Stop ();
			gearSource.pitch += 0.15f;
			Debug.Log (gearSource.pitch);
		}
		gearSource.PlayOneShot (gearClips [randInt]);
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		SFX.SetFloat ("SFX Pitch", (1 - Mathf.Abs (Player.lightCounter) / 8));
		SFX.SetFloat ("SFX lowpass freq", (7000 - (2000 * (Mathf.Abs (Player.lightCounter)))));
		SFX.SetFloat ("SFX volume", (1 - (Mathf.Abs (Player.lightCounter) / 8)));
	}
}
