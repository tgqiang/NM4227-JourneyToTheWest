using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

	public AudioClip m_PlatformClip;
	public AudioClip m_DoorOpenClip;
	public AudioClip m_TrainAmbientClip;
	public AudioClip m_TrainStoppingClip;
	AudioSource m_audioPlayer;


	void Start() {
		m_audioPlayer = GetComponent<AudioSource> ();
	}


	public void PlayDoorOpenSound() {
		m_audioPlayer.Stop ();
		m_audioPlayer.PlayOneShot (m_DoorOpenClip);
	}


	public void PlayTrainStoppingSound() {
		m_audioPlayer.Stop ();
		m_audioPlayer.PlayOneShot (m_TrainStoppingClip);
	}


	public void PlayTrainAmbient() {
		m_audioPlayer.clip = m_TrainAmbientClip;
		m_audioPlayer.Play ();
	}
}
