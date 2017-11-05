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

	[Header("Train Animations")]
	public GameObject m_TrainDoorLeft;
	public GameObject m_TrainDoorRight;
	public Vector3 m_DoorOpenPosition;
	public float m_TrainDoorsOpenDuration;

	[Header("Game-Start Transition Elements")]
	public PhoneManager m_PhoneScript;
	public CinematicManager m_CinematicScript;

	public GameObject m_GameMission;
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

	[Header("Audio")]
	public AudioManager m_AudioPlayer;


	void Awake () {
		VignetteModel.Settings vignetteSettings = m_PostProcessProfile.vignette.settings;
		vignetteSettings.intensity = 0f;
		m_PostProcessProfile.vignette.settings = vignetteSettings;

		GrainModel grainModel = m_PostProcessProfile.grain;
		grainModel.enabled = false;

		DepthOfFieldModel dofModel = m_PostProcessProfile.depthOfField;
		dofModel.enabled = false;
	}


	void Update() {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			OpenExitPrompt ();
		}
	}


	// Main Menu options //
	public void ViewInstructions() {
		//m_AudioPlayer.PlayDoorOpenSound ();
		StartCoroutine (AnimateDoorsCoroutine ());
	}


	public void QuitGame() {
		Application.Quit ();
	}


	// Instruction Screen options //
	public void CloseInstructions() {
		StartCoroutine (TransitToGameCoroutine ());
	}


	// Game Mission Panel options //
	public void CloseGameMission() {
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
		//m_PhoneScript.ManualHideMobilePhoneInterface ();
	}


	// Coroutines //

	IEnumerator AnimateDoorsCoroutine() {
		m_MainMenuPanel.SetActive (false);

		Hashtable hashtableLeftDoor = iTween.Hash ("position", -1 * m_DoorOpenPosition,
			                              "time", m_TrainDoorsOpenDuration,
			                              "easetype", "easeInOutQuart");
		Hashtable hashtableRightDoor = iTween.Hash ("position", m_DoorOpenPosition,
			                               "time", m_TrainDoorsOpenDuration,
			                               "easetype", "easeInOutQuart");

		iTween.MoveTo (m_TrainDoorLeft, hashtableLeftDoor);
		iTween.MoveTo (m_TrainDoorRight, hashtableRightDoor);

		yield return new WaitForSeconds (m_TrainDoorsOpenDuration + 0.5f);
		m_InstructionPanel.SetActive (true);
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
		//yield return CameraFadeCoroutine ();
	}


	IEnumerator DismissMissionCoroutine() {
		iTween.FadeTo (m_GameMission, 0, m_GameMissionFadeDuration);
		for (int i = 0; i < m_GameMissionTexts.Length; i++) {
			iTween.FadeTo (m_GameMissionTexts[i], 0, m_GameMissionFadeDuration);
		}

		yield return new WaitForSeconds (m_GameMissionFadeDuration);
	}


	IEnumerator PlayerEnterTrainCoroutine() {
		m_Player.SetActive (true);

		iTween.ScaleTo (m_Player, m_PlayerScale, m_PlayerMoveIntoTrainDuration);
		yield return new WaitForSeconds (m_PlayerMoveIntoTrainDuration + 0.5f);

		iTween.FadeTo (m_Player, 0f, m_PlayerFadeDuration);
		yield return new WaitForSeconds (m_PlayerFadeDuration + 0.5f);

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
		iTween.FadeTo (m_CameraFadePanel, 0f, m_CameraFadeDuration);
		yield return new WaitForSeconds (m_CameraFadeDuration + 1f);
	}
}
