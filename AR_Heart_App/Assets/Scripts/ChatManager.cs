
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Manages the UI and functionality of the interactive chatbot.
/// </summary>
public class ChatManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField]
    private InputField chatInputField;
    [SerializeField]
    private Button sendButton;
    [SerializeField]
    private Text chatHistoryText; // Simple solution using a single Text object
    [SerializeField]
    private ScrollRect chatScrollRect;

    [Header("Managers")]
    [SerializeField]
    private APIManager apiManager;

    private List<ChatMessage> messageHistory = new List<ChatMessage>();

    void Start()
    {
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnSendButtonPressed);
        }

        // Add a welcome message
        ReceiveMessage("Hello! Ask me about the different arrhythmia types.");
    }

    /// <summary>
    /// Called when the user clicks the 'Send' button.
    /// </summary>
    private void OnSendButtonPressed()
    {
        string messageText = chatInputField.text;
        if (!string.IsNullOrEmpty(messageText))
        {
            SendMessage(messageText);
            chatInputField.text = ""; // Clear the input field
        }
    }

    /// <summary>
    /// Adds the user's message to the history and sends it to the API.
    /// </summary>
    private void SendMessage(string messageText)
    {
        AddMessageToHistory("You", messageText);

        // Call the API Manager to get a response from the bot
        StartCoroutine(apiManager.GetChatbotResponse(messageText, (response) => {
            if (response != null && !string.IsNullOrEmpty(response.reply))
            {
                ReceiveMessage(response.reply);
            }
            else
            {
                ReceiveMessage("Sorry, I encountered an error. Please try again.");
            }
        }));
    }

    /// <summary>
    /// Called by the API manager's callback to display the bot's response.
    /// </summary>
    private void ReceiveMessage(string messageText)
    {
        AddMessageToHistory("Bot", messageText);
    }

    /// <summary>
    /// Adds a message to the internal list and updates the UI.
    /// </summary>
    private void AddMessageToHistory(string sender, string message)
    {
        ChatMessage newMessage = new ChatMessage { sender = sender, text = message };
        messageHistory.Add(newMessage);
        UpdateChatUI();
    }

    /// <summary>
    /// Re-renders the chat history text UI.
    /// </summary>
    private void UpdateChatUI()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var message in messageHistory)
        {
            sb.AppendLine($"<b>{message.sender}:</b> {message.text}");
        }
        chatHistoryText.text = sb.ToString();

        // Ensure the scroll view automatically scrolls to the bottom
        Canvas.ForceUpdateCanvases();
        chatScrollRect.verticalNormalizedPosition = 0f;
    }
}
