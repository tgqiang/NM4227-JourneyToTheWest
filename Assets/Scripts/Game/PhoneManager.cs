using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.PostProcessing;
using UnityEngine.UI;

public class PhoneManager : MonoBehaviour {

	[Header("Starting Text Prompt")]
	public GameObject m_PromptTexts;
	bool isCurrentlyAtTutorial;

	[Header("Messages and Replies")]
	public string[] m_GirlfriendMessages;
	public string[][] m_PlayerReplies;
	public int m_NumberOfReplyOptions;
	public int m_MaxResponsesNeeded;
	public GameObject m_GirlfriendMessageListRoot;
	public GameObject m_GirlfriendMessagePrefab;
	public GameObject m_PlayerReplyListRoot;
	public GameObject m_PlayerReplyPrefab;

	int[] answer = {0, 1, 0, 0, 1, 2, 1, 0, 2, 2, 1, 1};
	int score = 0;

	public int Score {
		get {
			return score;
		}
	}

	[Header("Phone Interface")]
	public GameObject m_PhoneInterface;
	public GameObject m_PhoneButton;
	public Vector2 m_PhoneActiveButtonSize;
	public Vector2 m_PhoneInactiveButtonSize;
	public Vector3 m_ActivePosition;
	public Vector3 m_InactivePosition;
	public Sprite m_PhoneActiveSprite;
	public Sprite m_PhoneInactiveSprite;
	public float m_PhoneAnimationDuration;
	Color[] m_FlashingColor;
	bool m_IsMobilePhoneActive;
	bool isCutscenePlaying;

	[Header("Phone Texting Interface")]
	public GameObject m_ReplyOptionsRoot;
	public Text[] m_ReplyOptionTexts;
	AudioSource m_PhoneNotification;
	public AudioClip m_MessageReceiveSound;
	public AudioClip m_ReplySentSound;
	public Image m_HeaderBackground;
	public Color m_MessageReceivedColor;
	public Color m_ReplySentColor;
	public GameObject m_HeaderNeutralText;
	public GameObject m_HeaderNotificationText;

	[Header("Relationship Meter and Vignette Effects")]
	public Slider m_RelationshipMeter;
	public float m_MaxMessageIgnoreDuration;	// used for triggering the vignette effect
	public float m_MeterDepletionPerSecond;
	public PostProcessingProfile m_VignetteProfile;

	public float m_VignetteMaxOpacity;				// the maximum opacity the vignette effect can have,
													// so that the player can still see something in the game
	bool toReplyMessage;
	float timeTillResponse;
	float vignetteOffset;

	public bool ToReplyMessage {
		get {
			return toReplyMessage;
		}
	}

	[Header("Pending Message Receiving")]
	public int m_NumPendingIncomingMessages = 0;	// for handling pending messages from GF
	public float m_TimeBeforeReceivingNextPendingMessage;
	bool hasReachedEnding;

	[Header("Element Offsets")]
	public Vector2 m_MessageOffset = new Vector2(0, 200f);
	public Vector2 m_MessageListRootOffset = new Vector2 (0, 100f);
	public int m_TotalMessagesToDisplayOnScreen;
	public int m_MessageIndexOffset;

	bool canReceiveMessage = true;
	bool isMessageFromGF = true;
	int totalMessageCount = 0;
	int girlfriendMessageIndex = 0;
	int playerReplyIndex = 0;
	int numResponses;

	List<GameObject> m_GirlfriendMessageList;
	List<GameObject> m_PlayerReplyList;

	// Callback for resuming game after 1st message is replied
	public delegate void ClearFirstMessage();
	ClearFirstMessage callbackToClearFirstMessage;


	// Use this for initialization
	void Start () {
		vignetteOffset = m_MaxMessageIgnoreDuration * (m_MeterDepletionPerSecond);
		m_FlashingColor = new Color[] { Color.green, Color.blue };
		m_PhoneNotification = GetComponent<AudioSource> ();

		InitializePlayerReplies ();

		m_GirlfriendMessageList = new List<GameObject> ();
		m_PlayerReplyList = new List<GameObject> ();
	}


	void Update () {
		if (!hasReachedEnding) {
			if (isCutscenePlaying) {
				// do nothing
			} else {
				if (toReplyMessage) {
					float deltaTime = Time.deltaTime;
					timeTillResponse += deltaTime;

					if (!m_IsMobilePhoneActive) {
						m_HeaderBackground.color = m_FlashingColor [Mathf.FloorToInt (timeTillResponse % 2)];
					} else {
						m_HeaderBackground.color = m_FlashingColor [0];
					}

					m_RelationshipMeter.value = Mathf.Max (m_RelationshipMeter.value - m_MeterDepletionPerSecond * deltaTime, 0);

					if (timeTillResponse > m_MaxMessageIgnoreDuration) {
						float ratio = ComputeRatioFromRelationshipMeter ();
						UpdateVignetteEffect (ratio);
					}
				}
			}
		}
	}


	void InitializePlayerReplies() {
		m_PlayerReplies = new string[][] {
			new string[] {"Hey, can't wait to see you too! I'm on the mrt at Pasir Ris now.",
						  "What you mean? We just talk yesterday??",
						  "You want McSpicy? I heard got offer leh"},
			new string[] {"Lol okay",
						  "I've missed you too!... Your love and support has helped me cope babe.",
						  "You are so lame haha"},
			new string[] {"Yes I'm hurrying!! That's so sweet, tell her I'm coming",
						  "Uh u know how SMRT is, can't get any faster than this",
						  "Huh, I don't like her cooking leh :(..."},
			new string[] {"Didn't get to see you these few weeks, makes me very sad!! Not fun at all",
						  "Super shag... Somemore you stay so far...",
						  "Like BMT lor"},
			new string[] {"A car maybe? lol driving much faster than mrt. you live v far leh",
						  "Ooo I dont know! Either way, I know I'll love it since it's from you ;)",
						  "lol what"},
			new string[] {"lol ok, I too busy to miss you leh",
						  "What to do... BMT is lidat one",
						  "I'm so sorry babe. I really really wish I could be there for you!"},
			new string[] {"Too busy lah",
						  "oh babe I'm so sorry I was busy. Hope you are doing well, I can't wait to catch up with you my love.",
						  "I thought we talked ytd? Still got things to say meh?"},
			new string[] {"It really hurts. The balm I used in camp cannot beat your daily massages!",
						  "It really hurts. Ltr massage for me ok",
						  "Nothing one la"},
			new string[] {"You won't understand one la...",
						  "Didn't really wanna hear your nagging. Like my mother leh you",
						  "I really wanted to call you but it was too late by then. I'll tell you more later"},
			new string[] {"Maybe you should find another boyfriend who's not in army then. lol",
						  "Seeing you too much makes me annoyed anyway lol!",
						  "I have no choice baby, let's get through this tough period together!"},
			new string[] {"You better make sure the food is warm when I arrive",
						  "Yes I am! I'm so sorry to keep your family waiting, I am reaching soon babe.",
						  "I don't really want to eat anymore urgh."},
			new string[] {"Coming. Stop rushing me lah",
						  "Racing to your place at the speed of light!!! I'm so excited to see your fam too",
						  "I'm in army lol"}
		};
	}


	public void InitializeFirstMessageCallback(ClearFirstMessage callback) {
		callbackToClearFirstMessage = callback;
	}


	public void SetIsCutscenePlaying() {
		isCutscenePlaying = true;
		UpdateVignetteEffect (0);
		m_PhoneInterface.SetActive (false);
	}

	// ==================== PHONE ACTIVATION TRANSITION FUNCTIONS ==================== //

	public void ToggleMobilePhoneInterface() {
		EventSystem.current.SetSelectedGameObject (null);
		StartCoroutine (AnimatePhoneInterfaceCoroutine ());
	}


	public void ManualHideMobilePhoneInterface() {
		if (m_IsMobilePhoneActive) {
			ToggleMobilePhoneInterface ();
		}
	}


	public void ManualShowMobilePhoneInterface() {
		if (!m_IsMobilePhoneActive) {
			ToggleMobilePhoneInterface ();
		}
	}


	IEnumerator AnimatePhoneInterfaceCoroutine() {
		if (m_IsMobilePhoneActive) {

			if (isCurrentlyAtTutorial) {
				isCurrentlyAtTutorial = false;
				m_PromptTexts.SetActive (false);
			}

			m_PhoneButton.GetComponent<Image> ().sprite = m_PhoneInactiveSprite;
			(m_PhoneButton.transform as RectTransform).sizeDelta = m_PhoneInactiveButtonSize;
			m_IsMobilePhoneActive = false;
			iTween.MoveTo (m_PhoneInterface, m_InactivePosition, m_PhoneAnimationDuration);
		} else {
			
			if (isCurrentlyAtTutorial) {
				m_PromptTexts.SetActive (true);
			}

			m_PhoneButton.GetComponent<Image> ().sprite = m_PhoneActiveSprite;
			(m_PhoneButton.transform as RectTransform).sizeDelta = m_PhoneActiveButtonSize;
			m_IsMobilePhoneActive = true;
			iTween.MoveTo (m_PhoneInterface, m_ActivePosition, m_PhoneAnimationDuration);
		}

		yield return new WaitForSeconds (m_PhoneAnimationDuration);
	}

	// ==================== END PHONE ACTIVATION TRANSITION FUNCTIONS ==================== //



	// ==================== PHONE TEXTING INTERFACE FUNCTIONS ==================== //

	void UpdateAndShowReplyOptions() {
		for (int i = 0; i < m_NumberOfReplyOptions; i++) {
			m_ReplyOptionTexts [i].text = m_PlayerReplies [totalMessageCount / 2] [i];
		}

		m_ReplyOptionsRoot.SetActive (true);
	}


	void HideReplyOptions() {
		m_ReplyOptionsRoot.SetActive (false);
	}


	void UpdateMessageReceivedOnInterface() {
		m_HeaderBackground.color = m_MessageReceivedColor;
		m_HeaderNeutralText.SetActive (false);
		m_HeaderNotificationText.SetActive (true);
		m_PhoneNotification.clip = m_MessageReceiveSound;
		m_PhoneNotification.Play ();

		// Create the message object (that is initially invisible), update its attributes, and update the list
		GameObject incomingText = Instantiate (m_GirlfriendMessagePrefab, m_GirlfriendMessageListRoot.transform);
		incomingText.transform.Find ("Text").gameObject.GetComponent<Text> ().text = m_GirlfriendMessages [girlfriendMessageIndex];
		(incomingText.transform as RectTransform).anchoredPosition -= m_MessageOffset * girlfriendMessageIndex;
		m_GirlfriendMessageList.Add (incomingText);

		// Update important indices and counts
		totalMessageCount += 1;
		girlfriendMessageIndex += 1;

		// Invoke this to reposition all message elements
		RepositionMessagesInScreen ();

		// Finally, reveal the added message
		incomingText.SetActive (true);

		UpdateAndShowReplyOptions ();
	}


	void UpdateMessageSentOnInterface(int option) {
		// Hide the reply options to prevent player from spamming
		HideReplyOptions ();

		if (option == answer [playerReplyIndex]) {
			score += 1;
			//Debug.Log ("Correct reply - score: [" + score + "]");
		}

		m_HeaderBackground.color = m_ReplySentColor;
		m_HeaderNotificationText.SetActive (false);
		m_HeaderNeutralText.SetActive (true);
		m_PhoneNotification.clip = m_ReplySentSound;
		m_PhoneNotification.Play ();

		// Create the message object (that is initially invisible), update its attributes, and update the list
		GameObject incomingText = Instantiate (m_PlayerReplyPrefab, m_PlayerReplyListRoot.transform);
		incomingText.transform.Find("Text").gameObject.GetComponent<Text> ().text = m_PlayerReplies [playerReplyIndex][option];
		(incomingText.transform as RectTransform).anchoredPosition -= m_MessageOffset * playerReplyIndex;
		m_PlayerReplyList.Add (incomingText);

		// Update important indices and counts
		totalMessageCount += 1;
		playerReplyIndex += 1;
		numResponses += 1;

		//Debug.Log ("Number of replies so far: " + numResponses);

		// Invoke this to reposition all message elements
		RepositionMessagesInScreen ();

		// Finally, reveal the added message
		incomingText.SetActive (true);
	}


	void RepositionMessagesInScreen() {
		(m_GirlfriendMessageListRoot.transform as RectTransform).anchoredPosition += m_MessageListRootOffset;
		(m_PlayerReplyListRoot.transform as RectTransform).anchoredPosition += m_MessageListRootOffset;

		if (totalMessageCount >= m_TotalMessagesToDisplayOnScreen) {
			if (isMessageFromGF) {
				m_PlayerReplyList [playerReplyIndex - m_MessageIndexOffset].SetActive (false);
			} else {
				m_GirlfriendMessageList [girlfriendMessageIndex - m_MessageIndexOffset].SetActive (false);
			}
		}
	}

	// ==================== END PHONE TEXTING INTERFACE FUNCTIONS ==================== //



	// ==================== VIGNETTE FUNCTIONS ==================== //

	void UpdateVignetteEffect(float intensity) {
		VignetteModel.Settings vignetteSettings = m_VignetteProfile.vignette.settings;
		vignetteSettings.intensity = intensity;
		m_VignetteProfile.vignette.settings = vignetteSettings;
	}

	// ==================== END VIGNETTE FUNCTIONS ==================== //



	// ==================== PHONE BACKEND FUNCTIONS ==================== //

	void OverrideAllEffectsDuringCutscene() {
		m_PhoneInterface.SetActive (false);
		UpdateVignetteEffect (0);
	}


	public void NotifyCutsceneEnded() {
		isCutscenePlaying = false;
		m_PhoneInterface.SetActive (true);
	}


	public void NotifyEndingReached() {
		hasReachedEnding = true;
		UpdateVignetteEffect (0);
	}


	float ComputeRatioFromRelationshipMeter() {
		// This ratio requires an offset, since the vignette effect should ease in m_MaxMessageIgnoreDuration seconds later
		return (m_RelationshipMeter.maxValue - vignetteOffset - m_RelationshipMeter.value) / (m_RelationshipMeter.maxValue - vignetteOffset) * m_VignetteMaxOpacity;
	}


	public void ReceiveTextFromGF(bool isTutorial = false) {
		isCurrentlyAtTutorial = isTutorial;

		if (canReceiveMessage) {
			if (numResponses < m_MaxResponsesNeeded) {
				canReceiveMessage = false;
				toReplyMessage = true;

				// Set the flag to indicate that message is coming from GF
				isMessageFromGF = true;

				UpdateMessageReceivedOnInterface ();
				ManualShowMobilePhoneInterface ();
			}
		} else {
			m_NumPendingIncomingMessages += 1;
		}
	}


	public void SendReplyToGF(int option) {
		toReplyMessage = false;

		timeTillResponse = 0;
		m_HeaderBackground.color = m_ReplySentColor;
		UpdateVignetteEffect (0);
		m_RelationshipMeter.value = m_RelationshipMeter.maxValue;	// also reset the relationship meter

		canReceiveMessage = true;

		// Set the flag to indicate that message is coming from player
		isMessageFromGF = false;

		UpdateMessageSentOnInterface (option);

		if (numResponses == 1) {
			callbackToClearFirstMessage ();
		}

		StartCoroutine (ReceiveOnePendingMessageFromGF ());
	}


	IEnumerator ReceiveOnePendingMessageFromGF() {
		if (m_NumPendingIncomingMessages > 0 && !hasReachedEnding) {
			yield return new WaitForSeconds (m_TimeBeforeReceivingNextPendingMessage);
			ReceiveTextFromGF ();
			m_NumPendingIncomingMessages -= 1;
		}
	}

	// ==================== END PHONE BACKEND FUNCTIONS ==================== //
}
