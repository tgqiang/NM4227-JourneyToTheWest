using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSpillingCutscene : MonoBehaviour {

	[Header("Targets")]
	public GameObject m_Player;
	public GameObject m_BagPivot;
	public GameObject m_CutscenePanel;
	public GameObject m_CutsceneText;
	public GameObject m_CameraFadePanel;
	public PhoneManager m_PhoneScript;

	[Header("Transition Attributes")]
	public Vector3 m_BagRotation;
	public Sprite m_BagOpenSprite;
	public Sprite m_PlayerNormalSprite;
	public Sprite m_PlayerSurprisedSprite;
	public float m_BagTiltDuration;
	public float m_TextPersistDuration;
	public float m_CameraFadeDuration;

	[Header("Audio")]
	public AudioClip m_ZipperRipClip;
	public AudioClip m_ItemSpillingClip;

	AudioSource m_AudioSource;


	void Awake() {
		m_AudioSource = EventSystem.current.GetComponent<AudioSource> ();
	}


	public IEnumerator CutsceneCoroutine() {
		m_Player.GetComponent<Animator> ().enabled = false;

		m_AudioSource.PlayOneShot (m_ZipperRipClip);
		yield return new WaitForSeconds (1f);

		//m_Player.transform.localScale = new Vector3 (-1f, 1f, 1f);
		m_Player.GetComponent<Image> ().sprite = m_PlayerSurprisedSprite;
		m_BagPivot.transform.Find ("Image").gameObject.GetComponent<Image> ().sprite = m_BagOpenSprite;
		iTween.RotateTo (m_BagPivot, m_BagRotation, m_BagTiltDuration);
		yield return new WaitForSeconds (.3f);
		m_AudioSource.PlayOneShot (m_ItemSpillingClip);
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

		//m_Player.transform.localScale = Vector3.one;
		m_Player.GetComponent<Image> ().sprite = m_PlayerNormalSprite;
		m_CutscenePanel.SetActive (false);

		iTween.FadeTo (m_CameraFadePanel, 0f, m_CameraFadeDuration);
		yield return new WaitForSeconds (m_CameraFadeDuration + 0.5f);
	}
}
