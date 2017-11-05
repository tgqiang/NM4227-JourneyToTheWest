using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainArgumentCutscene : MonoBehaviour {

	[Header("Cutscene Panel")]
	public GameObject m_CutscenePanel;

	[Header("Targets")]
	public GameObject m_TrainInterior;
	public GameObject m_ArguingPassenger;
	public GameObject m_Player;
	public GameObject m_AnotherPassenger;
	public GameObject m_CameraFadePanel;
	public PhoneManager m_PhoneScript;

	[Header("Transition Attributes")]
	public Vector3 m_ArguingPassengerMoveToPosition;
	public float m_ArguingPassengerMoveToDuration;
	public float m_TimeDelayBeforePlayerEvictsFromSeat;
	public float m_CursingDuration;
	public Vector3 m_PlayerExpandScale;
	public float m_PlayerScaleDuration;
	public Vector3 m_PlayerMoveToPosition;
	public float m_PlayerMoveToDuration;
	public float m_CameraFadeDuration;


	public IEnumerator CutsceneCoroutine() {
		m_TrainInterior.SetActive (true);

		iTween.FadeTo (m_CameraFadePanel, 0f, m_CameraFadeDuration);
		yield return new WaitForSeconds (m_CameraFadeDuration);

		iTween.MoveTo (m_ArguingPassenger, iTween.Hash ("position", m_ArguingPassengerMoveToPosition, "time", m_ArguingPassengerMoveToDuration, "easetype", iTween.EaseType.linear));
		yield return new WaitForSeconds (m_ArguingPassengerMoveToDuration + 0.5f);

		m_ArguingPassenger.transform.Find ("Cursing").gameObject.SetActive (true);
		yield return new WaitForSeconds (m_CursingDuration);

		m_ArguingPassenger.transform.Find ("Cursing").gameObject.SetActive (false);
		yield return new WaitForSeconds (m_TimeDelayBeforePlayerEvictsFromSeat);

		iTween.ScaleTo (m_Player, m_PlayerExpandScale, m_PlayerScaleDuration);
		yield return new WaitForSeconds (m_PlayerScaleDuration + 1f);

		iTween.MoveTo (m_Player, iTween.Hash ("position", m_PlayerMoveToPosition, "time", m_PlayerMoveToDuration, "easetype", iTween.EaseType.linear));
		yield return new WaitForSeconds (m_PlayerMoveToDuration + 0.5f);

		iTween.FadeTo (m_CameraFadePanel, 1f, m_CameraFadeDuration);
		yield return new WaitForSeconds (m_CameraFadeDuration + 0.5f);

		m_ArguingPassenger.SetActive (false);
		m_AnotherPassenger.SetActive (true);
		(m_Player.transform as RectTransform).anchoredPosition = Vector2.zero;

		iTween.FadeTo (m_CameraFadePanel, 0f, m_CameraFadeDuration);
		yield return new WaitForSeconds (m_CameraFadeDuration + 0.5f);
	}
}
