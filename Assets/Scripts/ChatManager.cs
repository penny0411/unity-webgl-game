using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

public class ChatManager : MonoBehaviour, IPointerClickHandler
{
    [Header("Player Info UI")]
    public TMP_Text playerIdText;
    public TMP_Text creditsText;

    [Header("Input Field")]
    public TMP_InputField chatInput;

    [Header("Text RectTransform")]
    public RectTransform textRect;

    [Header("Send Button")]
    public Button sendButton;

    [Header("RPG Response Bubble")]
    public RPGResponseBubble rpgBubble;

    [Header("Scroll Settings")]
    public ScrollRect scrollRect;

    [Header("Typewriter Settings")]
    public float typingSpeed = 0.03f;

    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private string fullMessage;

    void Start()
    {
        StartCoroutine(ActivateInputNextFrame());

        chatInput.onValueChanged.AddListener(OnInputChanged);
        sendButton.onClick.AddListener(OnSendClicked);

        if (playerIdText != null)
            playerIdText.text = "ID: " + GameDataManager.Instance.playerId;

        if (creditsText != null)
            creditsText.text = "Credits: 20";
    }

    IEnumerator ActivateInputNextFrame()
    {
        yield return null;
        chatInput.ActivateInputField();
        chatInput.Select();
    }

    void OnInputChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
            Canvas.ForceUpdateCanvases();
    }

    void Update()
    {
        if (chatInput.isFocused && !isTyping)
        {
            bool enterPressed = false;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
                enterPressed = true;
#else
            if (Input.GetKeyDown(KeyCode.Return))
                enterPressed = true;
#endif

            if (enterPressed)
                OnSendClicked();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        chatInput.ActivateInputField();
        chatInput.Select();
    }

    void OnSendClicked()
    {
        if (isTyping) return;

        string playerMessage = chatInput.text.Trim();
        if (string.IsNullOrEmpty(playerMessage)) return;

        chatInput.text = "";

        StartCoroutine(OpenAIManager.Instance.SendMessageToAI(playerMessage, OnAIResponse));
    }

    void OnAIResponse(string aiReply, int credits)
    {
        if (creditsText != null)
            creditsText.text = "Credits: " + credits;

        StartTyping(aiReply);
    }

    void StartTyping(string message)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        rpgBubble.gameObject.SetActive(true);
        fullMessage = message;

        typingCoroutine = StartCoroutine(TypeText(message));
    }

    IEnumerator TypeText(string message)
    {
        isTyping = true;

        rpgBubble.responseText.text = "";

        for (int i = 0; i < message.Length; i++)
        {
            rpgBubble.responseText.text += message[i];
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }
}