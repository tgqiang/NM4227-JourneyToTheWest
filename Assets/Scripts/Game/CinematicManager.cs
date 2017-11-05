using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CinematicManager : MonoBehaviour {

	[Header("Targets")]
	public PhoneManager m_PhoneScript;
	public GameObject m_NextButton;

	[Header("Black Boxes")]
	public GameObject m_TopBox;
	public GameObject m_BottomBox;

	[Header("Attributes")]
	public Vector2 m_MovePosition;
	public float m_BoxMoveDuration;


	public void ShowNextButton(UnityAction func) {
		m_NextButton.GetComponent<Button> ().onClick.AddListener (func);
		m_NextButton.GetComponent<Button> ().onClick.AddListener (HideButton);
		m_NextButton.SetActive (true);
	}


	public void HideButton() {
		m_NextButton.GetComponent<Button> ().onClick.RemoveAllListeners ();
		m_NextButton.SetActive (false);
	}

	
	public void DeactivateCinematicEffect () {
		StartCoroutine (DeactivateCinematicEffectCoroutine ());
	}


	public IEnumerator ActivateCinematicEffectCoroutine() {
		m_PhoneScript.SetIsCutscenePlaying ();

		iTween.MoveTo (m_TopBox, Vector2.zero, m_BoxMoveDuration);
		iTween.MoveTo (m_BottomBox, Vector2.zero, m_BoxMoveDuration);

		yield return new WaitForSeconds (m_BoxMoveDuration + 0.5f);
	}


	public IEnumerator DeactivateCinematicEffectCoroutine() {
		iTween.MoveTo (m_TopBox, m_MovePosition, m_BoxMoveDuration);
		iTween.MoveTo (m_BottomBox, -m_MovePosition, m_BoxMoveDuration);

		yield return new WaitForSeconds (m_BoxMoveDuration + 0.5f);

		m_PhoneScript.NotifyCutsceneEnded ();
	}
}
