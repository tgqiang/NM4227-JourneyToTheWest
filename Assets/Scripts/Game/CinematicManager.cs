using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
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

	
	public void DeactivateCinematicEffect (bool hasDialogue = false) {
		StartCoroutine (DeactivateCinematicEffectCoroutine (hasDialogue));
	}


	public IEnumerator ActivateCinematicEffectCoroutine(bool hasDialogue = false) {
		m_PhoneScript.SetIsCutscenePlaying ();

		iTween.MoveTo (m_TopBox, Vector2.zero, m_BoxMoveDuration);

		if (hasDialogue) {
			iTween.MoveTo (m_BottomBox, Vector2.zero, m_BoxMoveDuration);
		} else {
			iTween.MoveTo (m_BottomBox, new Vector2(0f, -150f), m_BoxMoveDuration);
		}

		yield return new WaitForSeconds (m_BoxMoveDuration + 0.5f);
	}


	public IEnumerator DeactivateCinematicEffectCoroutine(bool hasDialogue = false) {
		iTween.MoveTo (m_TopBox, m_MovePosition, m_BoxMoveDuration);
		if (hasDialogue) {
			iTween.MoveTo (m_BottomBox, -m_MovePosition, m_BoxMoveDuration);
		} else {
			iTween.MoveTo (m_BottomBox, new Vector2(0f, -300f), m_BoxMoveDuration);
		}

		yield return new WaitForSeconds (m_BoxMoveDuration + 0.5f);

		m_PhoneScript.NotifyCutsceneEnded ();
	}
}
