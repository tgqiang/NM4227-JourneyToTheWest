using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MinigameOne : MonoBehaviour {

	static string[] components = { "A", "D" };

	[Header("Text Trigger")]
	public PhoneManager m_PhoneScript;
	public float m_TimeToTriggerMessage;
	float timeElapsedSinceMinigameStart;

	[Header("Combo Key Sprite references")]
	public Sprite[] m_KeySprites;

	[Header("Minigame One Instructions Modal & Countdown")]
	public GameObject m_GameInstructionsModal;
	public GameObject m_GameInstructionsPanel;
	public GameObject m_CountdownText;

	[Header("Transition Elements")]
	public TrainArgumentCutscene m_CutsceneScript;
	public CinematicManager m_CinematicScript;
	public GameObject m_CameraFadePanel;
	public float m_TimeDelayBeforeShowingInstructions;
	public float m_TimeDelayBeforeCutscene;
	public float m_CutsceneDurationBeforeTransitToMainGame;
	public float m_TimeDelayBeforeTransitionToNextCarriage;
	public float m_CameraFadeDuration;

	[Header("Minigame One UI Elements")]
	public GameObject m_GamePanel;
	public GameObject m_ComboKeysPanel;
	public Image[] m_ComboKeys;
	public Image m_ComboKeysBG;
	public Text m_FeedbackText;
	public GameObject m_Player;
	public GameObject m_CompetingPassengerMoving;
	public GameObject m_CompetingPassenger;
	public GameObject m_VacantSeat;
	public Vector2 m_PlayerInitialPosition;
	public Vector2 m_CompetingPassengerInitialPosition;
	public float m_NumStepsForCompetingPassengerToReachSeats;
	Vector2 stepDistance;
	Vector2 competingStepDistance;

	[Header("Minigame One Attributes")]
	public int m_NumSeats;

	bool m_IsInPlay;
	bool canReceiveInput;
	string comboString = "";
	int comboStringIndex = 0;
	int numCorrectAttempts = 0;				// need to clear sequence of m_NumSeats x combo strings to clear game

	[Header("Audio")]
	public AudioClip m_CountdownClip;
	public AudioClip m_MinigameStartClip;
	public AudioClip m_SuccessClip;
	public AudioClip m_FailureClip;

	AudioSource m_AudioSource;

	// Callback to GameState
	public delegate void ReturnFlowToGameState();
	ReturnFlowToGameState callbackToGameState;


	void Awake() {
		m_AudioSource = EventSystem.current.GetComponent<AudioSource> ();
	}


	void Start() {
		stepDistance = ((m_VacantSeat.transform as RectTransform).anchoredPosition - (m_Player.transform as RectTransform).anchoredPosition) / m_NumSeats;
		stepDistance.y = 0;
		competingStepDistance = stepDistance * m_NumSeats / m_NumStepsForCompetingPassengerToReachSeats;
	}


	void Update() {
		// Update loop logic for handling minigame 1
		if (m_IsInPlay) {
			timeElapsedSinceMinigameStart += Time.deltaTime;

			if (timeElapsedSinceMinigameStart >= m_TimeToTriggerMessage &&
			    timeElapsedSinceMinigameStart < m_TimeToTriggerMessage + Time.deltaTime) {
				m_PhoneScript.ReceiveTextFromGF ();
			}

			if (canReceiveInput) {
				(m_CompetingPassengerMoving.transform as RectTransform).anchoredPosition += (competingStepDistance * Time.deltaTime);

				if (HasPassengerReachedSeatFirst ()) {
					MovePlayerToNextCarriage ();
				}
			}

			if (Input.anyKeyDown) {
				if (!Input.GetKeyDown(KeyCode.Mouse0)) {
					CheckForInputMatchWithComboString ();
				}
			}
		}
	}


	// ==================== MINIGAME 1 LOGIC / FUNCTIONS ==================== //

	/*
	 * 1. Generate combo string
	 * 2. Register each input and check for match (done in Update() loop)
	 * 3. a) If correct, move on to next input
	 *    b) Else, player moves to next carriage and repeats this
	 * 4. a) If player clears the combo string, he gets the seat and go straights to cutscene
	 *    b) Otherwise, when player uses up all his attempts, he will also be directed to cutscene
	 */
	public void TriggerMinigame(ReturnFlowToGameState callback) {
		GenerateComboString (numCorrectAttempts + 1);
		callbackToGameState = callback;
		m_GamePanel.SetActive (true);
		StartCoroutine (ShowInstructionsCoroutine ());
	}


	public void StartMinigame() {
		StartCoroutine (AnimateCountdownCoroutine ());
	}


	void GenerateComboString(int comboStringLength) {
		// Reset the combo key feedback view
		m_FeedbackText.text = "Press the keys from left to right to get to the empty seat!";
		m_ComboKeysBG.color = Color.white * 0.6f;

		comboString = "";		// we reset the combo string first, in case the player fails any previous attempts

		for (int i = 0; i < comboStringLength; i++) {
			int option = Random.Range (0, 2);
			m_ComboKeys [i].sprite = m_KeySprites [option];
			m_ComboKeys [i].color = Color.white;
			comboString += components [option];
		}
	}


	void CheckForInputMatchWithComboString() {
		if (canReceiveInput) {
			char requiredKey = comboString [comboStringIndex];
			bool isAKeyPressed = Input.GetKeyDown (KeyCode.A);
			bool isDKeyPressed = Input.GetKeyDown (KeyCode.D);

			switch (requiredKey) {
				case 'A':
					if (isAKeyPressed) {
						ClearCurrentComboKey ();
					} else {
						MovePlayerToNextCarriage ();
					}
					break;
			
				case 'D':
					if (isDKeyPressed) {
						ClearCurrentComboKey ();
					} else {
						MovePlayerToNextCarriage ();
					}
					break;

				default:
				// No game flow should end up here
					break;
			}
		}
	}


	bool HasPassengerReachedSeatFirst() {
		return m_CompetingPassengerMoving.activeSelf && (m_CompetingPassengerMoving.transform as RectTransform).anchoredPosition.x >= 550f;
	}


	void ClearCurrentComboKey() {
		m_ComboKeys [comboStringIndex].color = Color.green;
		comboStringIndex += 1;

		if ((comboStringIndex == numCorrectAttempts + 1) && numCorrectAttempts < m_NumSeats) {
			canReceiveInput = false;
			StartCoroutine (GenerateComboKeysForNextStageCoroutine ());
		}
	}


	void ResetComboKeysDisplay() {
		for (int i = 0; i < m_NumSeats; i++) {
			m_ComboKeys [i].sprite = null;
			m_ComboKeys [i].color = Color.white;
		}
	}


	void RestartMinigame() {
		ResetComboKeysDisplay ();
		numCorrectAttempts = 0;
		GenerateComboString (numCorrectAttempts + 1);
		comboStringIndex = 0;
	}


	void MovePlayerToNextCarriage() {
		StartCoroutine (TransitToNextTrainCarriageCoroutine ());
	}


	void EndGame() {
		StartCoroutine (AnimateCutsceneCoroutine (true));
	}

	// ==================== END MINIGAME 1 LOGIC / FUNCTIONS ==================== //



	// ==================== ANIMATIONS-CUTSCENES FUNCTIONS ==================== //
	IEnumerator ShowInstructionsCoroutine() {
		yield return new WaitForSeconds (m_TimeDelayBeforeShowingInstructions);
		m_GameInstructionsModal.SetActive (true);
		m_ComboKeysPanel.SetActive (true);
	}


	IEnumerator GenerateComboKeysForNextStageCoroutine() {
		comboStringIndex = 0;
		numCorrectAttempts += 1;
		(m_Player.transform as RectTransform).anchoredPosition += stepDistance;
		m_AudioSource.PlayOneShot (m_SuccessClip);
		m_FeedbackText.text = "Clear!";
		iTween.ShakePosition (m_FeedbackText.gameObject, new Vector3(0f, 50f, 0f), 0.2f);

		yield return new WaitForSeconds (0.3f);

		if (numCorrectAttempts == m_NumSeats) {
			EndGame ();
		} else {
			m_FeedbackText.text = "Press the keys from left to right to get to the empty seat!";
			GenerateComboString (numCorrectAttempts + 1);
			canReceiveInput = true;
		}
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

		m_IsInPlay = true;
		canReceiveInput = true;
	}


	IEnumerator TransitToNextTrainCarriageCoroutine() {
		yield return ShowWrongInputFeedbackCoroutine ();
		yield return AnimateCarriageTransitionCoroutine ();
	}


	IEnumerator ShowWrongInputFeedbackCoroutine() {
		canReceiveInput = false;
		m_AudioSource.PlayOneShot (m_FailureClip);
		m_CompetingPassengerMoving.SetActive (false);
		m_CompetingPassenger.SetActive (true);
		m_FeedbackText.text = "Oh no, someone has taken the seat! Moving to the next carriage to find a seat...";
		m_ComboKeysBG.color = Color.red;
		m_ComboKeys [comboStringIndex].color = Color.red;

		yield return new WaitForSeconds (m_TimeDelayBeforeTransitionToNextCarriage);
	}


	IEnumerator AnimateCarriageTransitionCoroutine() {
		iTween.FadeTo (m_CameraFadePanel, 1f, m_CameraFadeDuration);

		yield return new WaitForSeconds (m_CameraFadeDuration + 0.5f);

		RestartMinigame ();

		(m_Player.transform as RectTransform).anchoredPosition = m_PlayerInitialPosition;
		m_CompetingPassenger.SetActive (false);

		m_CompetingPassengerMoving.SetActive (true);
		(m_CompetingPassengerMoving.transform as RectTransform).anchoredPosition = m_CompetingPassengerInitialPosition;

		m_ComboKeysPanel.SetActive (false);

		iTween.FadeTo (m_CameraFadePanel, 0f, m_CameraFadeDuration);
		yield return new WaitForSeconds (m_CameraFadeDuration + 1f);

		m_ComboKeysPanel.SetActive (true);
		canReceiveInput = true;
	}


	IEnumerator AnimateCutsceneCoroutine(bool hasPassedMinigame) {
		m_IsInPlay = false;

		if (hasPassedMinigame) {
			m_FeedbackText.text = "You got the seat!";
			m_ComboKeysBG.color = Color.green;
		}

		yield return new WaitForSeconds (m_TimeDelayBeforeCutscene);

		iTween.FadeTo (m_CameraFadePanel, 1f, m_CameraFadeDuration);
		yield return new WaitForSeconds (m_CameraFadeDuration + 0.5f);

		m_GamePanel.SetActive (false);

		yield return m_CinematicScript.ActivateCinematicEffectCoroutine();
		yield return m_CutsceneScript.CutsceneCoroutine ();

		m_CinematicScript.ShowNextButton (MoveBackToMainGame);
	}


	public void MoveBackToMainGame() {
		StartCoroutine (MoveBackToMainGameCoroutine ());
	}


	IEnumerator MoveBackToMainGameCoroutine() {
		yield return m_CinematicScript.DeactivateCinematicEffectCoroutine ();
		callbackToGameState();
	}

	// ==================== ANIMATIONS-CUTSCENES FUNCTIONS ==================== //
}
