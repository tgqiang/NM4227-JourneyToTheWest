using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MinigameThree : MonoBehaviour {

	[Header("Targets")]
	public GameObject m_PlayerNormal;
	public GameObject m_PlayerFall;

	[Header("Text Trigger")]
	public PhoneManager m_PhoneScript;
	public float m_TimeTillFirstMessageOnBusTravel;
	public float m_TimeTillLastMessageOnBusTravel;
	public float m_TimeTillFirstMessageInMinigame;
	public float m_TimeTillLastMessageInMinigame;

	[Header("Minigame Three Instructions Modal & Countdown")]
	public GameObject m_GameInstructionsModal;
	public GameObject m_GameInstructionsPanel;
	public GameObject m_CountdownText;
	public float m_TimeDelayAfterCountdown;

	[Header("Transition Elements")]
	public GameObject m_CameraFadePanel;
	public CinematicManager m_CinematicScript;
	public float m_TimeDelayBeforeGameRestart;
	public float m_TimeDelayBeforeTransitToGameEnd;
	public float m_CameraFadeDuration;
	public float m_BusIdleTime;
	float totalBusTravelTime;
	bool m_IsIdlingOnBus;

	[Header("Minigame Three UI Elements")]
	public GameObject m_GamePanel;
	public GameObject m_MinigameUI;
	public GameObject m_Timer;
	public Text m_PromptText;
	public Text m_TimerText;
	public Slider m_StabilitySlider;

	[Header("Minigame Three Attributes")]
	public float m_MinigameDuration;
	public float[] m_SliderBoundValues;		// stores lower and upper bounds of the slider
	public float[] m_SliderWarningValues;
	bool m_IsInPlay;
	bool toUpdateTimer;
	float timeElapsed;

	[Header("Minigame Three Backend Model")]
	public const float CLAMP_DIVISOR = 6f;
	public Rigidbody2D m_PlankRigidbody;
	public Transform m_PlankCenter;
	public Rigidbody2D m_Ball;
	Vector2 m_BallInitialPosition = Vector2.zero;
	float m_InitialRotation = 0;
	public float m_RotationIncrement;
	public float[] m_RotationThresholds;

	[Header("Audio")]
	public AudioClip m_CountdownClip;
	public AudioClip m_MinigameStartClip;
	public AudioClip m_SuccessClip;
	public AudioClip m_FailureClip;

	public AudioSource m_TimerAudioSource;
	AudioSource m_AudioSource;

	// Callback to GameState
	public delegate void ReturnFlowToGameState();
	ReturnFlowToGameState callbackToGameState;


	void Awake() {
		m_AudioSource = EventSystem.current.GetComponent<AudioSource> ();
	}


	// Update is called once per frame
	void Update () {
		if (m_IsIdlingOnBus) {
			totalBusTravelTime += Time.deltaTime;

			// 2 messages will be fired on the bus ride - this is the first one.
			if (totalBusTravelTime >= m_TimeTillFirstMessageOnBusTravel &&
				totalBusTravelTime < m_TimeTillFirstMessageOnBusTravel + Time.deltaTime) {
				m_PhoneScript.ReceiveTextFromGF ();
			}

			// 2 messages will be fired on the bus ride - this is the second one.
			if (totalBusTravelTime >= m_TimeTillLastMessageOnBusTravel &&
			    totalBusTravelTime < m_TimeTillLastMessageOnBusTravel + Time.deltaTime) {
				m_PhoneScript.ReceiveTextFromGF ();
			}

			if (totalBusTravelTime > m_BusIdleTime) {
				m_IsIdlingOnBus = false;
				m_MinigameUI.SetActive (true);
			}
		}
		
		if (m_IsInPlay) {
			if (toUpdateTimer) {
				totalBusTravelTime += Time.deltaTime;

				// 2 messages will be fired during minigame #3 - this is the first one.
				if (totalBusTravelTime >= m_BusIdleTime + m_TimeTillFirstMessageInMinigame &&
					totalBusTravelTime < m_BusIdleTime + m_TimeTillFirstMessageInMinigame + Time.deltaTime) {
					m_PhoneScript.ReceiveTextFromGF ();
				}

				// 2 messages will be fired during minigame #3 - this is the second one.
				if (totalBusTravelTime >= m_BusIdleTime + m_TimeTillLastMessageInMinigame &&
					totalBusTravelTime < m_BusIdleTime + m_TimeTillLastMessageInMinigame + Time.deltaTime) {
					m_PhoneScript.ReceiveTextFromGF ();
				}

				UpdateTimer ();

				if (Input.GetKey (KeyCode.A)) {
					RotatePlankAnticlockwise ();
				} else if (Input.GetKey (KeyCode.D)) {
					RotatePlankClockwise ();
				}

				UpdateSliderValue ();
				CheckIfSliderIsWithinBounds ();

				if (timeElapsed > m_MinigameDuration) {
					EndGame ();
				}
			}
		}
	}


	public void PrepareMinigame(ReturnFlowToGameState callback) {
		callbackToGameState = callback;
		m_GamePanel.SetActive (true);
	}


	public void TriggerMinigame() {
		m_CinematicScript.ShowNextButton (ShowButtonAfterBusBoardingCutscene);
	}


	public void ShowButtonAfterBusBoardingCutscene() {
		StartCoroutine (ActivateMinigameCoroutine ());
	}


	public void StartMinigame() {
		StartCoroutine (AnimateCountdownCoroutine ());
	}


	void UpdateTimer () {
		timeElapsed += Time.deltaTime;
		m_TimerText.text = (10 - (Mathf.Floor (timeElapsed))).ToString ();
	}


	void CheckIfSliderIsWithinBounds() {
		float sliderValue = m_StabilitySlider.value;

		if (sliderValue < m_SliderBoundValues [0] ||
		    sliderValue > m_SliderBoundValues [1]) {	// if slider value exceed the bounds
			RestartGame ();
		} else if (sliderValue < m_SliderWarningValues [0] ||
		           sliderValue > m_SliderWarningValues [1]) {
			m_MinigameUI.GetComponent<Image> ().color = new Color (1f, 165f/255f, 0f);
			m_PromptText.color = Color.yellow;
			m_PromptText.text = "Whoa, watch out!";
		} else {
			m_MinigameUI.GetComponent<Image> ().color = Color.white * 150f/255f;
			m_PromptText.text = "Use 'A' and 'D' keys to bring the indicator to the center for at least 10 seconds!";
			m_PromptText.color = Color.white;
		}
	}


	void RestartGame() {
		StartCoroutine (RestartGameCoroutine ());
	}


	void EndGame() {
		StartCoroutine (ReturnToMainGameCoroutine ());
	}


	IEnumerator ActivateMinigameCoroutine() {
		yield return m_CinematicScript.DeactivateCinematicEffectCoroutine ();
		m_IsIdlingOnBus = true;
	}


	IEnumerator AnimateCountdownCoroutine() {
		m_GameInstructionsPanel.SetActive (false);

		m_AudioSource.PlayOneShot (m_CountdownClip, .7f);
		iTween.PunchScale (m_CountdownText, Vector3.one * 2f, .9f);
		yield return new WaitForSeconds (1f);

		m_AudioSource.PlayOneShot (m_CountdownClip, .7f);
		m_CountdownText.GetComponent<Text> ().text = "2";
		iTween.PunchScale (m_CountdownText, Vector3.one * 2f, .9f);
		yield return new WaitForSeconds (1f);

		m_AudioSource.PlayOneShot (m_CountdownClip, .7f);
		m_CountdownText.GetComponent<Text> ().text = "1";
		iTween.PunchScale (m_CountdownText, Vector3.one * 2f, .9f);
		yield return new WaitForSeconds (1f);

		m_AudioSource.PlayOneShot (m_MinigameStartClip, .7f);
		m_GameInstructionsModal.SetActive (false);

		yield return new WaitForSeconds (m_TimeDelayAfterCountdown);

		m_IsInPlay = true;
		m_Timer.SetActive (true);
		m_TimerAudioSource.Play ();
		toUpdateTimer = true;
		m_Ball.gravityScale = 5f;
	}


	IEnumerator RestartGameCoroutine() {
		toUpdateTimer = false;
		m_TimerAudioSource.Stop ();
		timeElapsed = 0;

		m_PlayerNormal.SetActive (false);
		m_PlayerFall.SetActive (true);
		m_AudioSource.PlayOneShot (m_FailureClip);
		m_MinigameUI.GetComponent<Image> ().color = Color.red;
		m_PromptText.text = "You fell flat on your face!";
		m_PromptText.color = Color.red;

		yield return new WaitForSeconds (m_TimeDelayBeforeGameRestart);

		iTween.FadeTo (m_CameraFadePanel, 1f, m_CameraFadeDuration);
		yield return new WaitForSeconds (m_CameraFadeDuration + 0.5f);

		m_PlayerNormal.SetActive (true);
		m_PlayerFall.SetActive (false);

		m_MinigameUI.GetComponent<Image> ().color = Color.white * 150f/255f;
		m_PromptText.text = "Use 'A' and 'D' keys to bring the indicator to the center for at least 10 seconds!";
		m_PromptText.color = Color.white;
		m_Timer.SetActive (false);
		ResetMinigameModel ();

		iTween.FadeTo (m_CameraFadePanel, 0f, m_CameraFadeDuration);
		yield return new WaitForSeconds (m_CameraFadeDuration + m_TimeDelayAfterCountdown);

		m_Timer.SetActive (true);
		toUpdateTimer = true;
		m_TimerAudioSource.Play ();
	}


	IEnumerator ReturnToMainGameCoroutine() {
		m_IsInPlay = false;
		toUpdateTimer = false;
		m_TimerAudioSource.Stop ();
		m_AudioSource.PlayOneShot (m_SuccessClip);
		m_MinigameUI.GetComponent<Image> ().color = Color.green;
		m_PromptText.text = "You kept your balance!";
		m_PromptText.color = Color.green;

		yield return new WaitForSeconds (m_TimeDelayBeforeTransitToGameEnd);

		iTween.FadeTo (m_CameraFadePanel, 1f, m_CameraFadeDuration);
		yield return new WaitForSeconds (m_CameraFadeDuration + 0.5f);

		m_GamePanel.SetActive (false);

		callbackToGameState ();
	}


	void RotatePlankAnticlockwise() {
		if (m_PlankRigidbody.rotation < m_RotationThresholds [0]) {
			m_PlankRigidbody.MoveRotation (m_PlankRigidbody.rotation + m_RotationIncrement);
		}
	}


	void RotatePlankClockwise() {
		if (m_PlankRigidbody.rotation > m_RotationThresholds [1]) {
			m_PlankRigidbody.MoveRotation (m_PlankRigidbody.rotation - m_RotationIncrement);
		}
	}


	void ResetMinigameModel() {
		m_Ball.position = m_BallInitialPosition;
		m_PlankRigidbody.gravityScale = 0;
		m_PlankRigidbody.rotation = m_InitialRotation;
		m_StabilitySlider.value = 0.5f;
	}


	void UpdateSliderValue() {
		int directionMultiplier = (m_Ball.position.x > 0) ? 1 : -1;
		float clampedDistanceFromCenter = (float) System.Math.Round((m_Ball.position - (Vector2)m_PlankCenter.position).magnitude, 2) * directionMultiplier / 12f;
		m_StabilitySlider.value = 0.5f + clampedDistanceFromCenter;
	}
}
