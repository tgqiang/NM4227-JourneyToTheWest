using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TrainFaultCutscene : MonoBehaviour {

	[Header("Targets")]
	public GameObject m_Train;
	public GameObject m_SeatingPassengers;
	public GameObject m_StandingPassengers;
	public GameObject m_Player;
	public GameObject m_Bag;
	public GameObject m_TrainAnnouncement;
	public GameObject m_CameraFadePanel;
	public CinematicManager m_CinematicScript;

	[Header("Transition Attributes")]
	public Vector3 m_TrainShakePosition;
	public float m_TrainShakeDuration;
	public float m_TrainAnnouncementDuration;
	public float m_TimeBeforePassengersStandUp;
	public float m_TimeBeforePassengersExit;
	public Vector3 m_PassengersExitToPosition;
	public float m_PassengersExitDuration;
	public Vector3 m_PlayerExitToPosition;
	public float m_PlayerExitDuration;
	public float m_CameraFadeDuration;

	[Header("Audio")]
	public AudioClip m_TrainFaultTriggerClip;
	public AudioClip m_TrainAnnoucementClip;
	public AudioClip m_PassengerFootstepsClip;
	public AudioClip m_PlayerFootstepsClip;

	AudioSource m_AudioSource;


	void Awake() {
		m_AudioSource = EventSystem.current.GetComponent<AudioSource> ();
	}


	public IEnumerator CutsceneCoroutine() {
		yield return m_CinematicScript.ActivateCinematicEffectCoroutine ();

		m_AudioSource.Stop ();
		m_AudioSource.PlayOneShot (m_TrainFaultTriggerClip);

		iTween.ShakePosition (m_Train, m_TrainShakePosition, m_TrainShakeDuration);
		yield return new WaitForSeconds (m_TrainShakeDuration + 0.8f);

		m_AudioSource.PlayOneShot (m_TrainAnnoucementClip);
		//m_TrainAnnouncement.SetActive (true);		NOTE: probably don't want to show the announcement since the texts don't match?
		yield return new WaitForSeconds (m_TrainAnnouncementDuration);
		yield return new WaitForSeconds (m_TimeBeforePassengersStandUp);

		m_Player.SetActive (true);
		m_SeatingPassengers.SetActive (false);
		m_StandingPassengers.SetActive (true);

		yield return new WaitForSeconds (m_TimeBeforePassengersExit);

		m_AudioSource.PlayOneShot (m_PassengerFootstepsClip);
		iTween.MoveTo (m_StandingPassengers, iTween.Hash ("position", m_PassengersExitToPosition, "time", m_PassengersExitDuration, "easetype", iTween.EaseType.linear));
		yield return new WaitForSeconds (m_PassengersExitDuration + 1f);

		m_TrainAnnouncement.SetActive (false);

		m_Bag.SetActive (false);
		m_Player.GetComponent<Animator> ().SetBool ("Walking", true);
		m_AudioSource.PlayOneShot (m_PlayerFootstepsClip);
		iTween.MoveTo (m_Player, iTween.Hash ("position", m_PlayerExitToPosition, "time", m_PlayerExitDuration, "easetype", iTween.EaseType.linear));
		yield return new WaitForSeconds (m_PlayerExitDuration);
		m_Player.GetComponent<Animator> ().SetBool ("Walking", false);
	}
}
