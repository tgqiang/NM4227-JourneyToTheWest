using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameState : MonoBehaviour {

	[Header("Postprocessing Profile")]
	public PostProcessingProfile m_PostProcessingProfile;

	[Header("Game HUD")]
	public GameObject m_HUD;
	public GameObject m_TrainHUD;
	public GameObject m_TrainSetting;
	public Image[] m_Indicators;
	public float m_DurationToNextStation;	// duration needed for next indicator to turn black
	float currentAccumulatedDuration = 0;
	bool isMovingToNextStation;
	Color COLOR_YELLOW = new Color (1f, 1f, 0f);

	[Header("In-Game Checkpoints")]
	public int m_MaxMinigames;
	int m_MinigamesCleared = 0;
	public int[] m_CheckpointIndices;	// used for triggering the minigames
	int m_CurrentCheckpoint = 0;

	bool m_IsOnTrain = true;			// a flag for firing on-train triggers
	bool m_IsPlayingMinigame;			// a flag used for overriding general event triggers,
										// when a minigame is being carried out

	[Header("Minigame Scripts")]
	public MinigameOne m_MinigameOneScript;
	public MinigameTwo m_MinigameTwoScript;
	public MinigameThree m_MinigameThreeScript;
	public int m_MinimumPassingScore;
	public PhoneManager m_PhoneScript;

	[Header("Cutscenes To Fire Outside Minigame")]
	public ItemSpillingCutscene m_PreMinigameTwoCutscene;
	public TrainFaultCutscene m_PreMinigameThreeCutscene;
	public CinematicManager m_CinematicScript;

	[Header("Transition to Bus Scenes")]
	public GameObject m_CameraFadePanel;
	public GameObject m_BusBoardingPanel;
	public GameObject m_BusAtBusStop;
	public GameObject m_InBusPanel;
	public GameObject m_PlayerAtBusStop;
	public Vector3 m_PlayerAtBusStopMoveToPosition;
	public float m_TransitionToBusStopDuration;
	public float m_BusArrivingDuration;
	public float m_BusBoardingCutsceneDuration;
	public float m_CameraFadeDuration;

	[Header("Ending Transitions")]
	public GameObject m_GirlfriendMeetingPanel;
	public GameObject m_EndingPanel;
	public GameObject m_Player;
	public GameObject m_PlayerSpeechBubble;
	public GameObject m_GirlfriendSpeechBubble;
	public float m_TimeDelayBetweenDialogueSwitch;
	public float m_DialogueSwapInterval;

	[Header("Message Mural")]
	public int m_MaxMessagesToDisplay;
	public GameObject[] m_MessagePairs;
	public GameObject m_PlayerMeetingGF;
	public GameObject m_Girlfriend;
	public Vector3 m_MessagePairMoveToPosition;
	public float m_MessagePairMoveToDuration;
	public Vector3 m_GFMoveToPosition;
	public float m_GFMoveToDuration;
	int numMessagesDisplayed = 0;

	[Header("Credits Panel")]
	public GameObject m_CreditsPanel;
	public float m_CreditsPersistenceDuration;

	[Header("Audio")]
	public BGMManager m_BGMPlayer;
	public AudioClip m_BusArrivalClip;
	public AudioClip m_PlayerFootstepsClip;
	public AudioClip m_GFDialogueClip;

	AudioSource m_AudioSource;


	void Awake() {
		m_AudioSource = EventSystem.current.GetComponent<AudioSource> ();
	}


	void Update() {
		if (m_IsOnTrain) {
			currentAccumulatedDuration += Time.deltaTime;

			TriggerMinigameAtCheckpoint ();

			if (!m_IsPlayingMinigame) {
				if (currentAccumulatedDuration > m_DurationToNextStation) {
					SetIndicatorToBlackAndIncrementCheckpointCounter ();
				}
			}

			if (!isMovingToNextStation) {
				if (Mathf.FloorToInt (currentAccumulatedDuration) % 2 == 0) {
					m_Indicators [m_CurrentCheckpoint].color = COLOR_YELLOW;
				} else {
					m_Indicators [m_CurrentCheckpoint].color = Color.black;
				}
			}
		}
	}


	// ==================== GENERIC GAME STATE FUNCTIONS APPLICABLE TO ALL CONTEXTS ==================== //

	public void HandleReturnFlowToGameState ()	{
		if (m_IsOnTrain) {
			m_TrainSetting.SetActive (true);
			SetIndicatorToBlackAndIncrementCheckpointCounter ();
		}
		m_IsPlayingMinigame = false;
		m_MinigamesCleared += 1;

		if (m_MinigamesCleared == 3) {
			m_PhoneScript.NotifyEndingReached ();
			SetupMessageMural ();
			StartCoroutine (ShowEndingCoroutine());
		}
	}


	// ==================== END GENERIC GAME STATE FUNCTIONS APPLICABLE TO ALL CONTEXTS ==================== //



	// ==================== PLAYER-IN-TRAIN GAME STATE FUNCTIONS ==================== //

	void SetIndicatorToBlackAndIncrementCheckpointCounter() {
		// We turn on this flag to halt the blinking effect of the current train map indicator light
		isMovingToNextStation = true;

		// we don't bother updating train map indicators at and beyond Clementi station,
		// since the player's gonna end up taking a bus due to a train fault
		// or something that will take place at this station.
		if (m_CurrentCheckpoint < 22) {
			currentAccumulatedDuration = 0;
			m_Indicators [m_CurrentCheckpoint].color = Color.black;
			m_CurrentCheckpoint += 1;
		}

		// [ Message triggers ]
		// One message will already have been triggered at main menu. (Msg #1)
		// One message will also have been triggered in minigame #1 (Pasir Ris station: Msg #2)
		// One message will also have been triggered in minigame #2. (Paya Lebar station: Msg #5)
		// One message will also have been triggered before minigame #3, after boarding the bus. (Msg #9)
		// One message will also have been triggered during the bus ride. (Msg #10)
		// Two messages will also have been triggered in minigame #3. (Msg #11, #12)

		if (m_CurrentCheckpoint == 3) {
			// We fire another message at Tanah Merah station (Msg #3)
			m_PhoneScript.ReceiveTextFromGF ();
		} else if (m_CurrentCheckpoint == 5) {
			// We fire another message at Kembangan station (Msg #4)
			m_PhoneScript.ReceiveTextFromGF ();
		} else if (m_CurrentCheckpoint == 12) {
			// We fire another message at City Hall station (Msg #6)
			m_PhoneScript.ReceiveTextFromGF ();
		} else if (m_CurrentCheckpoint == 16) {
			// We fire another message at Outram Park station (Msg #7)
			m_PhoneScript.ReceiveTextFromGF ();
		} else if (m_CurrentCheckpoint == 20) {
			// We fire another message at Buona Vista station (Msg #8)
			m_PhoneScript.ReceiveTextFromGF ();
		}

		// We turn off this flag now to let the blinking effect resume on next indicator light
		isMovingToNextStation = false;
	}


	void TriggerMinigameAtCheckpoint() {
		switch (m_CurrentCheckpoint) {
			case 0:		// Pasir Ris station
				if (!m_IsPlayingMinigame) {
					m_IsPlayingMinigame = true;
					m_MinigameOneScript.TriggerMinigame (HandleReturnFlowToGameState);
				}
				break;

			case 7:		// Paya Lebar station
				if (!m_IsPlayingMinigame) {
					m_IsPlayingMinigame = true;
					StartCoroutine (TriggerPreMinigameTwoCutsceneCoroutine ());
				}
				break;

			case 22:	// Clementi station: this should trigger a transference to the bus scenes
				if (!m_IsPlayingMinigame) {
					m_IsOnTrain = false;
					m_IsPlayingMinigame = true;
					m_MinigameThreeScript.PrepareMinigame (HandleReturnFlowToGameState);
					StartCoroutine (AnimateTransitionToBusCoroutine ());
				}
				break;

			default:
				m_IsPlayingMinigame = false;
				return;
		}
	}


	public void MoveOnToMinigameTwo() {
		StartCoroutine (TriggerLoadMinigameTwoCoroutine ());
	}


	// In this implementation, we show a limit of "bad replies", up to m_MaxMessagesToDisplay.
	void SetupMessageMural() {
		string[] gfMessageStrings = m_PhoneScript.GFMessageStrings;
		string[][] playerReplyStrings = m_PhoneScript.PlayerReplyStrings;
		int[] answerIndices = m_PhoneScript.AnswerIndices;
		int[] playerReplyIndices = m_PhoneScript.PlayerReplyIndices;

		for (int i = 0; i < playerReplyIndices.Length; i++) {
			// If the limit on number of messages to display has not been exceeded
			if (numMessagesDisplayed < m_MaxMessagesToDisplay) {
				// If player replied a particular message UNFAVORABLY, it will be shown on "message mural".
				if (playerReplyIndices [i] != -1) {
					if (playerReplyIndices [i] != answerIndices [i]) {
						m_MessagePairs [i].transform.Find ("GFMessage").Find ("Text").gameObject.GetComponent<Text> ().text = gfMessageStrings [i];
						m_MessagePairs [i].transform.Find ("PlayerMessage").Find ("Text").gameObject.GetComponent<Text> ().text = playerReplyStrings [i] [playerReplyIndices [i]];
						m_MessagePairs [i].SetActive (true);

						numMessagesDisplayed++;
					}
				}
			} else {
				break;
			}
		}
	}


	public void MoveToEndingAfterDialogue() {
		StartCoroutine (ShowEndingTrainSceneCoroutine ());
	}


	public void MoveToCredits() {
		StartCoroutine (ShowCreditsCoroutine ());
	}

	// ==================== END PLAYER-IN-TRAIN GAME STATE FUNCTIONS ==================== //



	// ==================== ANIMATIONS-TRANSITIONS FUNCTIONS ==================== //

	IEnumerator TriggerPreMinigameTwoCutsceneCoroutine() {
		yield return m_CinematicScript.ActivateCinematicEffectCoroutine ();

		yield return m_PreMinigameTwoCutscene.CutsceneCoroutine ();

		m_CinematicScript.ShowNextButton (MoveOnToMinigameTwo);
	}


	IEnumerator TriggerLoadMinigameTwoCoroutine() {
		yield return m_CinematicScript.DeactivateCinematicEffectCoroutine ();

		m_TrainSetting.SetActive (false);
		m_MinigameTwoScript.TriggerMinigame (HandleReturnFlowToGameState);
	}


	IEnumerator AnimateTransitionToBusCoroutine() {
		yield return m_PreMinigameThreeCutscene.CutsceneCoroutine ();
		yield return ExitTrainAndMoveToBusStopCoroutine ();
		yield return BoardBusCoroutine ();
	}


	IEnumerator ExitTrainAndMoveToBusStopCoroutine() {
		iTween.FadeTo (m_CameraFadePanel, 1f, m_CameraFadeDuration);
		yield return new WaitForSeconds(m_CameraFadeDuration + 0.5f);

		m_TrainSetting.SetActive (false);
		m_TrainHUD.SetActive (false);
		m_BusBoardingPanel.SetActive (true);

		iTween.FadeTo (m_CameraFadePanel, 0f, m_CameraFadeDuration);
		yield return new WaitForSeconds(m_CameraFadeDuration + 1.5f);
	}


	IEnumerator BoardBusCoroutine() {
		m_AudioSource.PlayOneShot (m_BusArrivalClip);
		iTween.ScaleTo (m_BusAtBusStop, Vector3.one, m_BusArrivingDuration);
		yield return new WaitForSeconds (m_BusArrivingDuration);

		m_PlayerAtBusStop.GetComponent<Animator> ().SetBool ("Walking", true);
		m_AudioSource.PlayOneShot (m_PlayerFootstepsClip);
		iTween.MoveTo (m_PlayerAtBusStop, iTween.Hash ("position", m_PlayerAtBusStopMoveToPosition, "time", m_BusBoardingCutsceneDuration, "easetype", iTween.EaseType.linear));
		yield return new WaitForSeconds (m_BusBoardingCutsceneDuration + 0.1f);
		m_PlayerAtBusStop.GetComponent<Animator> ().SetBool ("Walking", false);

		iTween.FadeTo (m_CameraFadePanel, 1f, m_CameraFadeDuration);
		yield return new WaitForSeconds(m_CameraFadeDuration + 0.5f);

		m_BusBoardingPanel.SetActive (false);
		m_InBusPanel.SetActive (true);
		m_BGMPlayer.PlayBusAmbient ();

		iTween.FadeTo (m_CameraFadePanel, 0f, m_CameraFadeDuration);
		yield return new WaitForSeconds(m_CameraFadeDuration + 1.5f);

		m_MinigameThreeScript.TriggerMinigame ();
	}

	
	IEnumerator ShowEndingCoroutine() {
		m_BGMPlayer.PlayWalkingAmbient ();

		yield return m_CinematicScript.ActivateCinematicEffectCoroutine (true);

		m_HUD.SetActive (false);
		m_GirlfriendMeetingPanel.SetActive (true);
		m_Player.GetComponent<AudioSource> ().Play ();
		m_Player.GetComponent<Animator> ().SetBool ("Walking", true);

		// TODO: probably will need to find a way to loop the footsteps SFX, since a separate BGM will be playing alongside.
		//m_AudioSource.PlayOneShot (m_PlayerFootstepsClip);

		iTween.FadeTo (m_CameraFadePanel, 0f, m_CameraFadeDuration);
		yield return new WaitForSeconds (m_CameraFadeDuration + 2f);

		for (int i = 0; i < numMessagesDisplayed; i++) {
			iTween.MoveTo (m_MessagePairs[i], iTween.Hash ("position", m_MessagePairMoveToPosition, "time", m_MessagePairMoveToDuration, "easetype", iTween.EaseType.linear));
			yield return new WaitForSeconds (m_MessagePairMoveToDuration + 1f);
		}

		if (m_PhoneScript.Score == 0) {
			yield return new WaitForSeconds ((m_MessagePairMoveToDuration + 1f) * m_MaxMessagesToDisplay);
		}

		iTween.MoveTo (m_Girlfriend, iTween.Hash ("position", m_GFMoveToPosition, "time", m_GFMoveToDuration, "easetype", iTween.EaseType.linear));
		yield return new WaitForSeconds (m_GFMoveToDuration);

		m_Player.GetComponent<AudioSource> ().Stop ();
		m_Player.GetComponent<Animator> ().SetTrigger("MessagesStop");	// stop the player's walking

		yield return new WaitForSeconds (1f);
		yield return EmulateDialogueCoroutine ();
	}


	IEnumerator EmulateDialogueCoroutine() {
		// GF speaks
		if (m_PhoneScript.Score >= m_MinimumPassingScore) {
			UpdateGFSpeechBubble ("I'm so happy to see you! Thank you for being such a great boyfriend baby :)");
		} else if (m_PhoneScript.Score == 0) {
			UpdateGFSpeechBubble ("You never replied me at all!!! What is wrong with you?");
		} else {
			UpdateGFSpeechBubble ("Your SMS replies were rather rude... I hope we are going to be alright together babe...");
		}

		m_AudioSource.PlayOneShot (m_GFDialogueClip);
		m_GirlfriendSpeechBubble.SetActive (true);
		yield return new WaitForSeconds (m_DialogueSwapInterval);

		m_GirlfriendSpeechBubble.SetActive (false);
		yield return new WaitForSeconds (m_TimeDelayBetweenDialogueSwitch);

		// GF speaks
		if (m_PhoneScript.Score >= m_MinimumPassingScore) {
			UpdateGFSpeechBubble("Erm... do you... still love me...?");
		} else if (m_PhoneScript.Score == 0) {
			UpdateGFSpeechBubble ("Do you actually love me at all??");
		} else {
			UpdateGFSpeechBubble ("Do you really love me?");
		}

		m_AudioSource.PlayOneShot (m_GFDialogueClip);
		m_GirlfriendSpeechBubble.SetActive (true);
		yield return new WaitForSeconds (m_DialogueSwapInterval);

		m_GirlfriendSpeechBubble.SetActive (false);
		yield return new WaitForSeconds (m_TimeDelayBetweenDialogueSwitch);

		// Player speaks
		m_PlayerSpeechBubble.SetActive (true);
	}


	IEnumerator ShowEndingTrainSceneCoroutine() {
		iTween.FadeTo (m_CameraFadePanel, 1f, m_CameraFadeDuration);
		m_PlayerSpeechBubble.SetActive (false);
		yield return new WaitForSeconds(m_CameraFadeDuration + 0.5f);

		VignetteModel.Settings vignetteSettings = m_PostProcessingProfile.vignette.settings;
		vignetteSettings.intensity = 0f;
		m_PostProcessingProfile.vignette.settings = vignetteSettings;

		GrainModel grainModel = m_PostProcessingProfile.grain;
		grainModel.enabled = true;

		DepthOfFieldModel dofModel = m_PostProcessingProfile.depthOfField;
		dofModel.enabled = true;

		m_GirlfriendMeetingPanel.SetActive (false);
		m_EndingPanel.SetActive (true);
		m_BGMPlayer.PlayEndingSong ();

		iTween.FadeTo (m_CameraFadePanel, 0f, m_CameraFadeDuration);
		yield return new WaitForSeconds(m_CameraFadeDuration + 2f);

		m_CinematicScript.ShowNextButton (MoveToCredits);
	}


	IEnumerator ShowCreditsCoroutine() {
		yield return m_CinematicScript.DeactivateCinematicEffectCoroutine ();

		iTween.FadeTo (m_CameraFadePanel, 1f, m_CameraFadeDuration);
		GrainModel grainModel = m_PostProcessingProfile.grain;
		grainModel.enabled = false;

		yield return new WaitForSeconds(m_CameraFadeDuration + 0.5f);

		m_CreditsPanel.SetActive (true);

		iTween.FadeTo (m_CameraFadePanel, 0f, m_CameraFadeDuration);
		yield return new WaitForSeconds(m_CameraFadeDuration + 1.5f);

		yield return new WaitForSeconds (m_CreditsPersistenceDuration);

		iTween.FadeTo (m_CameraFadePanel, 1f, m_CameraFadeDuration);
		yield return new WaitForSeconds(m_CameraFadeDuration + 0.5f);

		SceneManager.LoadScene ("GameScene");
	}

	// ==================== END ANIMATIONS-TRANSITIONS FUNCTIONS ==================== //


	void UpdateGFSpeechBubble(string dialogue) {
		m_GirlfriendSpeechBubble.transform.Find("Text").gameObject.GetComponent<Text>().text = dialogue;
	}
}
