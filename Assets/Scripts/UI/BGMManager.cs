using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMManager : MonoBehaviour {

	[Header("Ambient Sounds")]
	public AudioClip m_TrainAmbientClip;
	public AudioClip m_BusAmbientClip;
	public AudioClip m_PlayerWalkingAmbientClip;
	public AudioClip m_EndingSongClip;

	public float m_AudioFadeTime;
	public AudioSource m_IntroSongPlayer;

	AudioSource m_AudioPlayer;


	void Awake() {
		m_AudioPlayer = GetComponent<AudioSource> ();
	}


	public void FadeOutIntroSong() {
		iTween.AudioTo (m_IntroSongPlayer.gameObject, iTween.Hash ("volume", 0f, "time", m_AudioFadeTime));
	}


	public void PlayTrainAmbient() {
		m_AudioPlayer.clip = m_TrainAmbientClip;
		m_AudioPlayer.Play ();
	}


	public void PlayBusAmbient() {
		m_AudioPlayer.clip = m_BusAmbientClip;
		m_AudioPlayer.Play ();
	}


	public void PlayWalkingAmbient() {
		m_AudioPlayer.clip = m_PlayerWalkingAmbientClip;
		m_AudioPlayer.Play ();
	}


	public void PlayEndingSong() {
		m_AudioPlayer.clip = m_EndingSongClip;
		m_AudioPlayer.Play ();
	}


	public void StopPlayer() {
		m_AudioPlayer.Stop ();
	}
}
