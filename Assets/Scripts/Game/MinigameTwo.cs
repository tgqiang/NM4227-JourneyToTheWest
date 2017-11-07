using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MinigameTwo : MonoBehaviour {

	[Header("Targets")]
	public GameObject m_BagPivot;
	public Sprite m_BagNormalSprite;

	[Header("Text Trigger")]
	public PhoneManager m_PhoneScript;
	public float m_TimeToTriggerMessage;
	float timeElapsedSinceMinigameStart;

	[Header("Sprite references")]
	public Sprite[] m_KeySprites;			// 'A' & 'D' button sprites
	public Sprite m_ItemRetrievedSprite;

	[Header("Minigame Two Instructions Modal & Countdown")]
	public GameObject m_GameInstructionsModal;
	public GameObject m_GameInstructionsPanel;
	public GameObject m_CountdownText;
	public float m_TimeDelayAfterCountdown;

	[Header("Transition Elements")]
	public GameObject m_CameraFadePanel;
	public float m_TimeDelayBeforeCutscene;
	public float m_CutsceneDurationBeforeTransitToMainGame;
	public float m_CameraFadeDuration;

	[Header("Minigame Two UI Elements")]
	public GameObject m_TrainHUD;
	public GameObject m_GamePanel;
	public GameObject[] m_Passengers;
	public Image m_KeyImage;
	public GameObject m_SuccessPrompt;

	[Header("Minigame Two Attributes")]
	public const int MAX_PASSENGERS_IN_TRAIN = 8;
	public float m_DelayDurationTillMinigameStarts;
	public float m_MinimumTimeForExposingItem;
	public float m_MaximumTimeForExposingItem;
	public float m_MinimumDelayBeforePassengerLiftsFoot;
	public float m_MaximumDelayBeforePassengerLiftsFoot;	// the time delay between the moment a passenger puts
															// down his/her foot, and the moment another passenger
															// will lift up his/her foot
	[Header("Audio")]
	public AudioClip m_CountdownClip;
	public AudioClip m_MinigameStartClip;
	public AudioClip m_SuccessClip;
	public AudioClip m_FailureClip;

	AudioSource m_AudioSource;

	bool m_IsInPlay;
	bool[] isItemRetrievedFromSeat;
	bool isLiftingFoot = false;
	bool canAcceptInput;

	float accumulatedFootLiftDuration = 0;
	float footLiftTimeLimit;
	float accumulatedDelay = 0;
	float predeterminedDelayDuration;
	int currentPassengerLiftingFoot = -1;

	bool toResumeFlow = true;

	// Callback to GameState
	public delegate void ReturnFlowToGameState();
	ReturnFlowToGameState callbackToGameState;


	void Awake() {
		m_AudioSource = EventSystem.current.GetComponent<AudioSource> ();
	}


	// Use this for initialization
	void Start () {
		isItemRetrievedFromSeat = new bool[MAX_PASSENGERS_IN_TRAIN];
	}


	// Update is called once per frame
	void Update () {
		if (m_IsInPlay) {
			timeElapsedSinceMinigameStart += Time.deltaTime;

			if (timeElapsedSinceMinigameStart >= m_TimeToTriggerMessage &&
				timeElapsedSinceMinigameStart < m_TimeToTriggerMessage + Time.deltaTime) {
				m_PhoneScript.ReceiveTextFromGF ();
			}

			// Every now and then, randomize a passenger with an item beneath their seats,
			// then animate them to lift up their foot and reveal the item below.
			// After that, check if input matches the required key
			if (isLiftingFoot) {
				accumulatedFootLiftDuration += Time.deltaTime;

				if (accumulatedFootLiftDuration > footLiftTimeLimit) {
					if (toResumeFlow) {
						AnimatePassengerPuttingDownFoot ();
						predeterminedDelayDuration = Random.Range (m_MinimumDelayBeforePassengerLiftsFoot, m_MaximumDelayBeforePassengerLiftsFoot);

						accumulatedFootLiftDuration = 0;
						isLiftingFoot = false;
					}
				} else {
					CheckForInputMatch ();
				}
			} else {
				canAcceptInput = false;

				accumulatedDelay += Time.deltaTime;

				if (accumulatedDelay > predeterminedDelayDuration) {
					MakeRandomPassengerLiftFoot ();

					accumulatedDelay = 0;
					isLiftingFoot = true;
				}
			}

			if (HaveAllItemsBeenRetrieved ()) {
				EndGame ();
			}
		}
	}


	// ==================== MINIGAME 2 LOGIC / FUNCTIONS ==================== //

	/*
	 * 1. A random passenger will lift their foot and hold them in mid-air for some time,
	 *    then put his/her foot down.
	 * 2. Player must press 'A' or 'D' to retrieve the item underneath that passenger's foot,
	 *    depending on which passenger raised his/her foot.
	 *    a) If input is registered in time, the item is retrieved from under the passenger's seat
	 *    b) Else, player must wait for next trigger.
	 * 3. If player finally takes back all items from underneath every seat, he clears the game.
	 *    Otherwise, the game continues until the end state is reached.
	 */
	public void TriggerMinigame(ReturnFlowToGameState callback) {
		callbackToGameState = callback;
		m_GamePanel.SetActive (true);
	}


	public void StartMinigame() {
		StartCoroutine (AnimateCountdownCoroutine ());
	}


	// NOTE: might need revisions depending on "grading scheme"
	// e.g. whether to make passenger put foot down upon any wrong key,
	// or just ignore all wrong inputs and only consume the
	// correct input to retrieve item from below the passenger's feet
	void CheckForInputMatch() {
		char requiredKey = (currentPassengerLiftingFoot < 4) ? 'A' : 'D';	// 'A' key for left side (indices 0~3),
		// 'D' for right side (indices 4~7)
		bool isAKeyPressed = Input.GetKeyDown (KeyCode.A);
		bool isDKeyPressed = Input.GetKeyDown (KeyCode.D);

		switch (requiredKey) {
			case 'A':
				if (isAKeyPressed) {
					if (canAcceptInput) {
						canAcceptInput = false;
						RetrieveItemAtSeat ();
					}
				} else if (isDKeyPressed) {
					if (canAcceptInput) {
						canAcceptInput = false;
						FailRetrieveItemAtSeat ();
					}
				}
				break;

			case 'D':
				if (isDKeyPressed) {
					if (canAcceptInput) {
						canAcceptInput = false;
						RetrieveItemAtSeat ();
					}
				} else if (isAKeyPressed) {
					if (canAcceptInput) {
						canAcceptInput = false;
						FailRetrieveItemAtSeat ();
					}
				}
				break;

			default:
				// No game flow should end up here
				break;
		}
	}


	void MakeRandomPassengerLiftFoot() {
		currentPassengerLiftingFoot = Random.Range (0, MAX_PASSENGERS_IN_TRAIN);

		if (isItemRetrievedFromSeat [currentPassengerLiftingFoot]) {
			MakeRandomPassengerLiftFoot ();
		} else {
			footLiftTimeLimit = Random.Range (m_MinimumTimeForExposingItem, m_MaximumTimeForExposingItem);
			m_KeyImage.sprite = (currentPassengerLiftingFoot < 4) ? m_KeySprites [0] : m_KeySprites [1];

			AnimatePassengerLiftingFoot ();
		}
	}


	void AnimatePassengerLiftingFoot() {
		canAcceptInput = true;
		m_Passengers [currentPassengerLiftingFoot].transform.Find ("Item").gameObject.SetActive (true);
		m_Passengers [currentPassengerLiftingFoot].gameObject.GetComponent<Image> ().enabled = false;
	}


	void AnimatePassengerPuttingDownFoot() {
		canAcceptInput = false;
		m_Passengers [currentPassengerLiftingFoot].transform.Find ("Item").gameObject.SetActive (false);
		m_Passengers [currentPassengerLiftingFoot].transform.Find ("Item").Find ("Key").gameObject.GetComponent<Image> ().color = Color.white;
		m_Passengers [currentPassengerLiftingFoot].gameObject.GetComponent<Image> ().enabled = true;
	}


	void RetrieveItemAtSeat() {
		StartCoroutine (RetrieveItemSuccessCoroutine ());
	}


	void FailRetrieveItemAtSeat() {
		StartCoroutine (RetrieveItemFailCoroutine ());
	}


	bool HaveAllItemsBeenRetrieved() {
		for (int i = 0; i < MAX_PASSENGERS_IN_TRAIN; i++) {
			if (!isItemRetrievedFromSeat [i]) {
				return false;
			}
		}

		return true;
	}


	void EndGame() {
		StartCoroutine (AnimateCutsceneCoroutine ());
	}


	IEnumerator RetrieveItemSuccessCoroutine() {
		canAcceptInput = false;
		toResumeFlow = false;

		m_AudioSource.PlayOneShot (m_SuccessClip);
		isItemRetrievedFromSeat [currentPassengerLiftingFoot] = true;
		m_Passengers [currentPassengerLiftingFoot].transform.Find ("Item").gameObject.GetComponent<Image> ().sprite = m_ItemRetrievedSprite;
		m_Passengers [currentPassengerLiftingFoot].transform.Find ("Item").gameObject.GetComponent<Image> ().color = new Color(1f, 1f, 1f, .5f);
		m_Passengers [currentPassengerLiftingFoot].GetComponent<Image> ().color = new Color(1f, 1f, 1f, .5f);
		m_Passengers [currentPassengerLiftingFoot].transform.Find ("Item").Find ("Key").gameObject.GetComponent<Image> ().color = Color.green;
		iTween.PunchPosition (m_Passengers [currentPassengerLiftingFoot].transform.Find ("Item").Find ("Key").gameObject, new Vector2 (0f, 5f), .5f);

		yield return new WaitForSeconds (.8f);

		toResumeFlow = true;
	}


	IEnumerator RetrieveItemFailCoroutine() {
		canAcceptInput = false;
		toResumeFlow = false;

		m_AudioSource.PlayOneShot (m_FailureClip);
		m_Passengers [currentPassengerLiftingFoot].transform.Find ("Item").Find ("Key").gameObject.GetComponent<Image> ().color = Color.red;
		iTween.PunchPosition (m_Passengers [currentPassengerLiftingFoot].transform.Find ("Item").Find ("Key").gameObject, new Vector2 (0f, 5f), .5f);

		yield return new WaitForSeconds (.8f);
		AnimatePassengerPuttingDownFoot ();

		toResumeFlow = true;
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
	}


	IEnumerator AnimateCutsceneCoroutine() {
		m_IsInPlay = false;
		yield return new WaitForSeconds (1.2f);

		m_SuccessPrompt.SetActive (true);

		yield return new WaitForSeconds (m_TimeDelayBeforeCutscene);

		iTween.FadeTo (m_CameraFadePanel, 1f, m_CameraFadeDuration);
		yield return new WaitForSeconds (m_CameraFadeDuration + 0.5f);

		m_GamePanel.SetActive (false);
		(m_BagPivot.transform as RectTransform).localRotation = Quaternion.Euler (Vector3.zero);
		m_BagPivot.transform.Find ("Image").gameObject.GetComponent<Image> ().sprite = m_BagNormalSprite;

		callbackToGameState();

		iTween.FadeTo (m_CameraFadePanel, 0f, m_CameraFadeDuration);
		yield return new WaitForSeconds (m_CameraFadeDuration + 0.5f);
	}
}
