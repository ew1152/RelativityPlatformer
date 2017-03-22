using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour {

	public AudioSource musicMain;
	public AudioSource musicBkgd;
	public AudioSource musicLight;

	public AudioClip musicMainIntro;
	public AudioClip musicMainLoop;
	public AudioClip musicBkgdIntro;
	public AudioClip musicBkgdLoop;
	public AudioClip musicLightIntro;
	public AudioClip musicLightLoop;

	float mainAmp;
	float bkgdAmp;
	float lightAmp;

	// Use this for initialization
	void Start () {
		musicMain.clip = musicMainIntro;
		musicMain.Play ();
		mainAmp = 1f;
		musicBkgd.clip = musicBkgdIntro;
		musicBkgd.Play ();
		bkgdAmp = 1f;
		musicLight.clip = musicLightIntro;
		musicLight.Play ();
		lightAmp = 0f;
	}
	
	// Update is called once per frame
	void Update () {
		if (!musicMain.isPlaying) {
			musicMain.clip = musicMainLoop;
			musicMain.Play ();
		}
		if (!musicBkgd.isPlaying) {
			musicBkgd.clip = musicBkgdLoop;
			musicBkgd.Play ();
		}
		if (!musicLight.isPlaying) {
			musicLight.clip = musicLightLoop;
			musicLight.Play ();
		}
		if (Mathf.Abs(Player.lightCounter) > 3 - (3 * mainAmp) && mainAmp > 0.6f) {
			mainAmp -= 0.01f;
		}
		if (Mathf.Abs(Player.lightCounter) < 3 - (3 * mainAmp) && mainAmp < 1f) {
			mainAmp += 0.01f;
		}
		if (Mathf.Abs(Player.lightCounter) > 3 - (3 * bkgdAmp)) {
			bkgdAmp -= 0.01f;
		}
		if (Mathf.Abs(Player.lightCounter) < 3 - (3 * bkgdAmp)) {
			bkgdAmp += 0.01f;
		}
		if (Mathf.Abs(Player.lightCounter) > 3 * lightAmp) {
			lightAmp += 0.01f;
		}
		if (Mathf.Abs(Player.lightCounter) < 3 * lightAmp) {
			lightAmp -= 0.01f;
		}
		musicMain.volume = mainAmp;
		musicBkgd.volume = bkgdAmp;
		musicLight.volume = lightAmp;
	}
}
