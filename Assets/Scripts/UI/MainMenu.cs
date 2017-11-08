using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

	[Header("Postprocessing Profile")]
	public PostProcessingProfile m_PostProcessProfile;

	[Header("Menu Panels")]
	public GameObject m_MainMenuPanel;
	public GameObject m_InstructionPanel;
	public GameObject m_TrainPanel;
	public GameObject m_CameraFadePanel;
	public GameObject m_HUD;
	public GameObject m_TrainHUD;
	public GameObject m_ExitPanel;

	[Header("UI Buttons")]
	public Button m_MainMenuButton;
	public Button m_StartButton;
	public Button m_QuitButton;

	[Header("Train Animations")]
	public GameObject m_TrainDoorLeft;
	public GameObject m_TrainDoorRight;
	public Vector3 m_DoorOpenPosition;
	public Vector3 m_DoorClosePosition;
	public float m_TrainDoorsOpenCloseDuration;

	[Header("Game-Start Transition Elements")]
	public PhoneManager m_PhoneScript;
	public CinematicManager m_CinematicScript;

	public GameObject m_GameMission;
	public GameObject m_GameMissionButton;
	public float m_GameMissionPersistenceDuration;

	public GameObject[] m_GameMissionTexts;
	public float m_GameMissionFadeDuration;

	public float m_CameraFadeDuration;

	public GameObject m_GameState;

	[Header("Player Animation Sequence")]
	public GameObject m_Player;
	public Vector3 m_PlayerScale;
	public float m_PlayerMoveIntoTrainDuration;
	public float m_PlayerFadeDuration;

	[Header("Interface Audio")]
	public AudioClip m_DoorOpenClip;
	public AudioClip m_DoorCloseClip;
	public AudioClip m_ButtonHoverClip;
	public AudioClip m_GenericButtonClickClip;
	public AudioClip m_DigitalButtonClickClip;

	[Header("SFX")]
	public AudioClip m_PlayerFootsteps;

	[Header("BGM / Ambience")]
	public BGMManager m_BGMPlayer;

	AudioSource m_AudioSource;


	void Awake () {
		VignetteModel.Settings vignetteSettings = m_PostProcessProfile.vignette.settings;
		vignetteSettings.intensity = 0f;
		m_PostProcessProfile.vignette.settings = vignetteSettings;

		GrainModel grainModel = m_PostProcessProfile.grain;
		grainModel.enabled = false;

		DepthOfFieldModel dofModel = m_PostProcessProfile.depthOfField;
		dofModel.enabled = false;

		m_AudioSource = GetComponent<AudioSource> ();
	}


	void Update() {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			OpenExitPrompt ();
		}
	}


	public void OnButtonHover() {
		m_AudioSource.PlayOneShot (m_ButtonHoverClip);
	}


	public void OnDigitalButtonHover() {
		m_AudioSource.PlayOneShot (m_DigitalButtonClickClip);
	}


	// Main Menu options //
	public void ViewInstructions() {
		m_QuitButton.interactable = false;
		m_BGMPlayer.FadeOutIntroSong ();
		m_AudioSource.PlayOneShot (m_DoorOpenClip);
		StartCoroutine (AnimateDoorsCoroutine ());
	}


	public void QuitGame() {
		Application.Quit ();
	}


	// Instruction Screen options //
	public void CloseInstructions() {
		m_AudioSource.PlayOneShot (m_GenericButtonClickClip);
		StartCoroutine (TransitToGameCoroutine ());
	}


	// Game Mission Panel options //
	public void CloseGameMission() {
		m_AudioSource.PlayOneShot (m_GenericButtonClickClip);
		StartCoroutine (BeginMainGameCoroutine ());
	}


	// Exit Panel options //
	public void OpenExitPrompt() {
		m_ExitPanel.SetActive (true);
	}


	public void CloseExitPrompt() {
		m_ExitPanel.SetActive (false);
	}


	// Callback //
	void HandleClearFirstMessage () {
		m_GameMission.SetActive (true);
	}


	// Coroutines //

	IEnumerator AnimateDoorsCoroutine() {
		m_MainMenuPanel.SetActive (false);

		Vector3 leftDoorOpenPosition = m_DoorOpenPosition;
		leftDoorOpenPosition.x *= -1;

		Hashtable hashtableLeftDoor = iTween.Hash ("position", leftDoorOpenPosition,
			                              "time", m_TrainDoorsOpenCloseDuration,
			                              "easetype", "easeInOutQuart");
		Hashtable hashtableRightDoor = iTween.Hash ("position", m_DoorOpenPosition,
			                               "time", m_TrainDoorsOpenCloseDuration,
			                               "easetype", "easeInOutQuart");

		iTween.MoveTo (m_TrainDoorLeft, hashtableLeftDoor);
		iTween.MoveTo (m_TrainDoorRight, hashtableRightDoor);

		yield return new WaitForSeconds (m_TrainDoorsOpenCloseDuration + 0.5f);
		m_InstructionPanel.SetActive (true);
	}


	IEnumerator AnimateDoorsClosingCoroutine() {
		m_AudioSource.PlayOneShot (m_DoorCloseClip);
		// Delay door closing for awhile to match the beeping sound effect
		yield return new WaitForSeconds (2f);

		Vector3 leftDoorClosePosition = m_DoorClosePosition;
		leftDoorClosePosition.x *= -1;

		Hashtable hashtableLeftDoor = iTween.Hash ("position", leftDoorClosePosition,
												   "time", m_TrainDoorsOpenCloseDuration,
												   "easetype", "easeInOutQuart");
		Hashtable hashtableRightDoor = iTween.Hash ("position", m_DoorClosePosition,
													"time", m_TrainDoorsOpenCloseDuration,
													"easetype", "easeInOutQuart");

		iTween.MoveTo (m_TrainDoorLeft, hashtableLeftDoor);
		iTween.MoveTo (m_TrainDoorRight, hashtableRightDoor);

		yield return new WaitForSeconds (m_TrainDoorsOpenCloseDuration + 1.5f);
	}


	IEnumerator TransitToGameCoroutine() {
		m_StartButton.interactable = false;
		m_InstructionPanel.SetActive (false);
		m_HUD.SetActive (true);

		m_PhoneScript.InitializeFirstMessageCallback(HandleClearFirstMessage);

		yield return new WaitForSeconds (1f);
		m_PhoneScript.ReceiveTextFromGF (true);
	}


	IEnumerator BeginMainGameCoroutine() {
		yield return DismissMissionCoroutine ();
		yield return m_CinematicScript.ActivateCinematicEffectCoroutine ();
		yield return PlayerEnterTrainCoroutine ();
	}


	IEnumerator DismissMissionCoroutine() {
		m_GameMissionButton.SetActive (false);
		iTween.FadeTo (m_GameMission, 0, m_GameMissionFadeDuration);

		for (int i = 0; i < m_GameMissionTexts.Length; i++) {
			iTween.FadeTo (m_GameMissionTexts[i], 0, m_GameMissionFadeDuration);
		}

		yield return new WaitForSeconds (m_GameMissionFadeDuration);
	}


	IEnumerator PlayerEnterTrainCoroutine() {
		m_Player.SetActive (true);
		m_AudioSource.PlayOneShot (m_PlayerFootsteps);

		iTween.ScaleTo (m_Player, m_PlayerScale, m_PlayerMoveIntoTrainDuration);
		yield return new WaitForSeconds (m_PlayerMoveIntoTrainDuration + 0.5f);

		iTween.FadeTo (m_Player, 0f, m_PlayerFadeDuration);
		yield return new WaitForSeconds (m_PlayerFadeDuration + 0.5f);

		yield return AnimateDoorsClosingCoroutine ();

		m_CinematicScript.ShowNextButton (MoveToGameAction);
	}


	public void MoveToGameAction() {
		StartCoroutine (CameraFadeCoroutine ());
	}


	IEnumerator CameraFadeCoroutine() {
		iTween.FadeTo (m_CameraFadePanel, 1f, m_CameraFadeDuration);
		yield return new WaitForSeconds (m_CameraFadeDuration);
		m_TrainPanel.SetActive (false);
		yield return m_CinematicScript.DeactivateCinematicEffectCoroutine ();
		m_TrainHUD.SetActive (true);
		m_GameState.SetActive (true);
		m_BGMPlayer.PlayTrainAmbient ();
		iTween.FadeTo (m_CameraFadePanel, 0f, m_CameraFadeDuration);
		yield return new WaitForSeconds (m_CameraFadeDuration + 1f);
	}
}
