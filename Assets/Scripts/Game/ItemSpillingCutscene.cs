using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpillingCutscene : MonoBehaviour {

	[Header("Targets")]
	public GameObject m_BagPivot;
	public GameObject m_CutscenePanel;
	public GameObject m_CutsceneText;
	public GameObject m_CameraFadePanel;
	public PhoneManager m_PhoneScript;

	[Header("Transition Attributes")]
	public Vector3 m_BagRotation;
	public float m_BagTiltDuration;
	public float m_TextPersistDuration;
	public float m_CameraFadeDuration;


	public IEnumerator CutsceneCoroutine() {
		iTween.RotateTo (m_BagPivot, m_BagRotation, m_BagTiltDuration);
		yield return new WaitForSeconds (m_BagTiltDuration + 0.5f);

		iTween.FadeTo (m_CameraFadePanel, 1f, m_CameraFadeDuration);
		yield return new WaitForSeconds (m_CameraFadeDuration + 0.5f);

		m_CutscenePanel.SetActive (true);

		iTween.FadeTo (m_CameraFadePanel, 0f, m_CameraFadeDuration);
		yield return new WaitForSeconds (m_CameraFadeDuration + 0.5f);

		m_CutsceneText.SetActive (true);
		yield return new WaitForSeconds (m_TextPersistDuration);

		iTween.FadeTo (m_CameraFadePanel, 1f, m_CameraFadeDuration);
		yield return new WaitForSeconds (m_CameraFadeDuration + 0.5f);

		(m_BagPivot.transform as RectTransform).rotation = Quaternion.Euler (Vector3.zero);
		m_CutscenePanel.SetActive (false);

		iTween.FadeTo (m_CameraFadePanel, 0f, m_CameraFadeDuration);
		yield return new WaitForSeconds (m_CameraFadeDuration + 0.5f);
	}
}
