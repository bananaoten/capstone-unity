using UnityEngine;
using TMPro;
using Firebase.Database;
using Firebase.Auth;
using System;
using System.Text.RegularExpressions;
using Firebase.Extensions;

public class InquiryFormManagerLancris : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown inquiryTypeDropdown;
    public TMP_Dropdown propertyDropdown;
    public TMP_InputField fullNameInput;
    public TMP_InputField emailInput;
    public TMP_InputField phoneInput;
    public TMP_InputField messageInput;
    public TMP_Text errorText;
    public GameObject exitButton;

    [Header("Messaging References")]
    public GameObject messagePagePanel;      // All conversations / agents
    public GameObject messagingPanelPage;    // Chat with the agent

    [Header("UI Prefabs")]
    public GameObject inquiryFormPrefab;     // Drag your inquiry form prefab here in the inspector

    [Header("Settings")]
    public string subdivision = "lancris";

    private DatabaseReference dbRef;
    private FirebaseAuth auth;
    private string currentInquiryId;
    private bool isAccepted = false;
    private string assignedAgentUid = null;
    private DatabaseReference inquiryRef;

    void Awake()
    {
        if (exitButton != null)
            exitButton.SetActive(true);
    }

    void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        auth = FirebaseAuth.DefaultInstance;

        if (auth.CurrentUser != null)
        {
            emailInput.text = auth.CurrentUser.Email ?? "";
            fullNameInput.text = auth.CurrentUser.DisplayName ?? "";

            ListenToUserInquiry(auth.CurrentUser.UserId);
        }
        else
        {
            // Show inquiry form for new users not logged in
            if (inquiryFormPrefab != null)
                inquiryFormPrefab.SetActive(true);
        }
    }

    void ListenToUserInquiry(string userId)
    {
        var userInquiryRef = dbRef.Child("user_inquiries").Child(userId);

        userInquiryRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted) return;

            if (!task.Result.Exists)
            {
                // No inquiry exists → show form for new user
                if (inquiryFormPrefab != null)
                    inquiryFormPrefab.SetActive(true);
                return;
            }

            currentInquiryId = task.Result.Value.ToString();
            inquiryRef = dbRef.Child("inquiries").Child(subdivision).Child(currentInquiryId);

            // Attach listener
            inquiryRef.ValueChanged += OnInquiryStatusChanged;

            // Check initial status
            inquiryRef.GetValueAsync().ContinueWithOnMainThread(t =>
            {
                if (t.IsFaulted || !t.Result.Exists) return;
                HandleInquirySnapshot(t.Result);
            });
        });
    }

    private void OnInquiryStatusChanged(object sender, ValueChangedEventArgs e)
    {
        if (!e.Snapshot.Exists) return;
        HandleInquirySnapshot(e.Snapshot);
    }

   private void HandleInquirySnapshot(DataSnapshot snapshot)
{
    string status = snapshot.Child("status").Value?.ToString();
    string agentUid = snapshot.Child("assignedAgentUid").Value?.ToString();

    if (string.IsNullOrEmpty(status)) return;

    if (status == "accepted" && !string.IsNullOrEmpty(agentUid))
    {
        isAccepted = true;
        assignedAgentUid = agentUid;

        // Hide inquiry form and show messaging panel
        if (inquiryFormPrefab != null)
            inquiryFormPrefab.SetActive(false);

        if (messagingPanelPage != null)
            messagingPanelPage.SetActive(true);

        OpenChatWithAgent(agentUid);

        // Unsubscribe immediately so further changes don't trigger this again
        if (inquiryRef != null)
            inquiryRef.ValueChanged -= OnInquiryStatusChanged;
    }
    else if (status == "pending")
    {
        // Only show the inquiry form if NOT already accepted
        if (!isAccepted)
        {
            isAccepted = false;
            assignedAgentUid = null;

            if (inquiryFormPrefab != null)
                inquiryFormPrefab.SetActive(true);

            ShowError("⏳ Your inquiry is pending. Waiting for agent...");
        }
    }
    else
    {
        // For any other status, you may want to handle accordingly
        // For example, if rejected, you could show a message or keep the form hidden
    }
}

    public void SubmitInquiry()
    {
        errorText.text = "";

        string fullName = fullNameInput.text.Trim();
        string email = emailInput.text.Trim();
        string phone = phoneInput.text.Trim();
        string message = messageInput.text.Trim();
        string inquiryType = inquiryTypeDropdown.options[inquiryTypeDropdown.value].text;
        string propertyId = propertyDropdown.options[propertyDropdown.value].text.ToLower();

        if (string.IsNullOrEmpty(fullName)) { ShowError("Full name is required."); return; }
        if (string.IsNullOrEmpty(email) || !IsValidEmail(email)) { ShowError("Please enter a valid email address."); return; }
        if (string.IsNullOrEmpty(phone) || !Regex.IsMatch(phone, @"^[0-9]+$")) { ShowError("Phone must contain numbers only."); return; }
        if (string.IsNullOrEmpty(message)) { ShowError("Additional message is required."); return; }
        if (auth.CurrentUser == null) { ShowError("You must be logged in to submit an inquiry."); return; }

        string inquiryId = dbRef.Child("inquiries").Child(subdivision).Push().Key;
        currentInquiryId = inquiryId;

        InquiryDataLancris inquiry = new InquiryDataLancris
        {
            fullName = fullName,
            email = email,
            phone = phone,
            message = message,
            inquiryType = inquiryType,
            propertyId = propertyId,
            propertyType = propertyId,
            consent = "agree",
            status = "pending",
            assignedAgentUid = null,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        string json = JsonUtility.ToJson(inquiry);

        dbRef.Child("inquiries").Child(subdivision).Child(inquiryId)
            .SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    dbRef.Child("user_inquiries").Child(auth.CurrentUser.UserId).SetValueAsync(inquiryId);

                    ShowError("✅ Inquiry submitted successfully!", success: true);
                    ClearForm();
                }
                else
                {
                    ShowError("❌ Error submitting inquiry.");
                }
            });
    }

   public void CloseForm()
{
    if (isAccepted && !string.IsNullOrEmpty(assignedAgentUid))
    {
        if (inquiryFormPrefab != null)
            inquiryFormPrefab.SetActive(false);

        if (messagingPanelPage != null)
            messagingPanelPage.SetActive(true);

        if (exitButton != null)
            exitButton.SetActive(false);  // 👈 hide exit button

        OpenChatWithAgent(assignedAgentUid);
    }
    else
    {
        if (messagePagePanel != null)
            messagePagePanel.SetActive(true);

        if (inquiryFormPrefab != null)
            inquiryFormPrefab.SetActive(true);

        if (exitButton != null)
            exitButton.SetActive(true);  // 👈 keep visible for pending/declined

        ShowError("⚠ You cannot proceed to chat until your inquiry is accepted.");
    }
}


    private void ShowError(string msg, bool success = false)
    {
        if (errorText != null)
        {
            errorText.color = success ? Color.green : Color.red;
            errorText.text = msg;
        }
    }

    private void ClearForm()
    {
        fullNameInput.text = "";
        emailInput.text = auth.CurrentUser != null ? auth.CurrentUser.Email ?? "" : "";
        phoneInput.text = "";
        messageInput.text = "";
        inquiryTypeDropdown.value = 0;
        propertyDropdown.value = 0;
    }

    private bool IsValidEmail(string email)
    {
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    // === CHAT HANDLER PLACEHOLDER ===
    private void OpenChatWithAgent(string agentUid)
    {
        Debug.Log("Opening chat with agent: " + agentUid);
        // TODO: Call your ChatManager here to connect user <-> agent
    }

public void OnExitButtonClicked()
{
    if (messagingPanelPage != null)
        messagingPanelPage.SetActive(false);
}

}

[Serializable]
public class InquiryDataLancris
{
    public string fullName;
    public string email;
    public string phone;
    public string message;
    public string inquiryType;
    public string propertyId;
    public string propertyType;
    public string consent;
    public string status; // pending, accepted, rejected
    public string assignedAgentUid;
    public long timestamp;
}