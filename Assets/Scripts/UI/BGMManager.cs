using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMManager : MonoBehaviour {

	[Header("Ambient Sounds")]
	public AudioClip m_TrainAmbientClip;
	public AudioClip m_BusStopAmbientClip;
	public AudioClip m_BusAmbientClip;
	public AudioClip m_PerfectEndingWalkingClip;
	public AudioClip m_NeutralEndingWalkingClip;
	public AudioClip m_BadEndingWalkingClip;
	public AudioClip m_WorstEndingWalkingClip;
	public AudioClip m_EndingSongClip;

	public PhoneManager m_PhoneScript;
	public GameState m_GameState;
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


	public void PlayBusStopAmbient() {
		m_AudioPlayer.clip = m_BusStopAmbientClip;
		m_AudioPlayer.Play ();
	}


	public void PlayBusAmbient() {
		m_AudioPlayer.clip = m_BusAmbientClip;
		m_AudioPlayer.Play ();
	}


	public void PlayWalkingAmbient(int score) {
		if (score == m_PhoneScript.m_MaxResponsesNeeded) {
			m_AudioPlayer.clip = m_PerfectEndingWalkingClip;
		} else if (!m_PhoneScript.HasAtLeastTwoReplies) {
			m_AudioPlayer.clip = m_WorstEndingWalkingClip;
		} else if (score >= m_GameState.m_MinimumPassingScore) {
			m_AudioPlayer.clip = m_NeutralEndingWalkingClip;
		} else {
			m_AudioPlayer.clip = m_BadEndingWalkingClip;
		}

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
