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

	bool hasAtLeastTwoReplies;

	public bool HasAtLeastTwoReplies {
		get {
			return hasAtLeastTwoReplies;
		}
	}

	int[] answer = {0, 1, 0, 0, 0, 0, 1, 0, 2, 2, 1, 1};
	int[] playerReplyIndices = new int[] {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1};
	int score = 0;

	public int Score {
		get {
			return score;
		}
	}

	public string[] GFMessageStrings {
		get {
			return m_GirlfriendMessages;
		}
	}

	public string[][] PlayerReplyStrings {
		get {
			return m_PlayerReplies;
		}
	}

	public int[] AnswerIndices {
		get {
			return answer;
		}
	}

	public int[] PlayerReplyIndices {
		get {
			return playerReplyIndices;
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
		m_FlashingColor = new Color[] { Color.white, Color.white };
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

					//m_RelationshipMeter.value = Mathf.Max (m_RelationshipMeter.value - m_MeterDepletionPerSecond * deltaTime, 0);

					if (timeTillResponse > m_MaxMessageIgnoreDuration) {
						//float ratio = ComputeRatioFromRelationshipMeter ();
						//UpdateVignetteEffect (ratio);
					}
				}
			}
		}
	}


	void InitializePlayerReplies() {
		m_PlayerReplies = new string[][] {
			new string[] {"Hey babe! Tons of things happened during BMT, but I'm okay. Tell you about it later.",
						  "Hey babe. A lot of things happened... BMT went quite badly actually.",
						  "Like that lor. Army still goes on right?? Life sucks."},
			new string[] {"Yeah, I didn't really want to talk about it over the phone. I thought you wouldn't understand.",
						  "Haha well, I'll tell you later. It was pretty hard to explain it over the phone",
						  "Say already you also cannot help right"},
			new string[] {"Yeah I guess I didn't want you to worry. Anyway, I feel better when I talk to you over the phone. It's like I can pretend I'm not in army for five minutes.",
						  "Can we not talk about this? I just really want to not think about anything and rest. Army's tough enough and you have to keep asking difficult questions.",
						  "What my dad told you?? How did you guys even meet lol."},
			new string[] {"Yeah I understand. Thanks babe :-) I feel like it's so difficult to find people who care in army.",
						  "Yeah can we just talk about something else?",
						  "Ok i get it"},
			new string[] {"I feel like it's hard to make friends here sometimes. But it's okay I guess, just need more time maybe!",
						  "You won't understand la, you also no need to go army.",
						  "lol whut, you k not?"},
			new string[] {"Me too!!!! :(",
						  "Yeah same. I totally get why people say being in army and having a girlfriend at the same time is tough.",
						  "You have friends in uni still lonely??"},
			new string[] {"Yeah, I guess so. Doesn't seem to change anything though. I still feel like we're living separate lives.",
						  "Yeah :-) That helps, surprisingly haha. Like I'm not alone in this. Our love will win right!",
						  "My wrist hurts.... And this mrt ride is taking forever omg why do you live so far"},
			new string[] {"Haha ok, don't worry ok? Anyway, I'm reaching soon!",
						  "Yeah...... But nothing will change still. I guess that's really what's important for me.",
						  "Actually maybe next time we could meet somewhere more central? Pasir Ris to Jookoon is just TOOOO FAR"},
			new string[] {"Nothing much to talk about la. Just like that lor.",
						  "Ya ok.",
						  "Yeah I really need a good talk and a listening ear :'("},
			new string[] {"Maybe you should find another boyfriend who's not in army then. haha kidding........?",
						  "Nah. What's for dinner??",
						  "Yeah, but I feel better when I talk to you!!"},
			new string[] {"You better make sure the food is warm when I arrive",
						  "Yes! So hungry.. Can't wait to see you too!",
						  "Not really hungry actually."},
			new string[] {"Coming. Stop rushing me lah",
						  "Racing to your place at the speed of light!!! I'm so excited to see your fam too",
						  "I'm in a constant state of despair lol"}
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

		playerReplyIndices[playerReplyIndex] = option;	// to track player's conversation history

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
		if (numResponses >= 2) {
			hasAtLeastTwoReplies = true;
		}

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
