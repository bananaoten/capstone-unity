using UnityEngine;
using TMPro;
using Firebase.Database;
using Firebase.Auth;
using System;
using System.Text.RegularExpressions;
using Firebase.Extensions;
using System.Collections.Generic;

public class InquiryFormManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown inquiryTypeDropdown;
    public TMP_Dropdown propertyDropdown; // Lancris only, Treelane can ignore
    public TMP_InputField fullNameInput;
    public TMP_InputField emailInput;
    public TMP_InputField phoneInput;
    public TMP_InputField messageInput;
    public TMP_Text errorText;
    public GameObject exitButton;
    public GameObject resubmitButton;

    [Header("Messaging References")]
    public GameObject messagePagePanel;
    public GameObject messagingPanelPage;

    [Header("UI Prefabs")]
    public GameObject inquiryFormPrefab;   // Inquiry form prefab

    // ✅ Appointment system references
    [Header("Appointment Prefabs - Treelane")]
    public GameObject[] treelaneAppointmentForms;
    public GameObject[] treelaneBlockers;

    [Header("Appointment Prefabs - Lancris")]
    public GameObject[] lancrisAppointmentForms;
    public GameObject[] lancrisBlockers;

    // ✅ Reservation system
    [Header("Reservation Blocker")]
    public GameObject reservationBlocker; // assign in Inspector

    [Header("Settings (Auto-Detected)")]
    [SerializeField] private string subdivision = "default"; 
    [SerializeField] private bool usePropertyId = false;      

    private DatabaseReference dbRef;
    private FirebaseAuth auth;

    private Dictionary<string, InquiryState> inquiryStates = new Dictionary<string, InquiryState>();

    void Awake()
    {
        if (exitButton != null)
            exitButton.SetActive(true);

        if (resubmitButton != null)
            resubmitButton.SetActive(false);

        DetectSubdivision();
    }

    void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        auth = FirebaseAuth.DefaultInstance;

        if (auth.CurrentUser != null)
        {
            emailInput.text = auth.CurrentUser.Email ?? "";
            fullNameInput.text = auth.CurrentUser.DisplayName ?? "";

            // ✅ Listen to both subdivisions
            ListenToUserInquiry(auth.CurrentUser.UserId, "treelane");
            ListenToUserInquiry(auth.CurrentUser.UserId, "lancris");
        }
        else
        {
            if (inquiryFormPrefab != null)
                inquiryFormPrefab.SetActive(true);
        }
    }

    private void DetectSubdivision()
    {
        if (inquiryFormPrefab != null)
        {
            string prefabName = inquiryFormPrefab.name.ToLower();

            if (prefabName.Contains("lancris"))
            {
                subdivision = "lancris";
                usePropertyId = true;
            }
            else if (prefabName.Contains("treelane"))
            {
                subdivision = "treelane";
                usePropertyId = false;
            }
            else
            {
                subdivision = "default";
                usePropertyId = false;
            }
        }

        Debug.Log($"[InquiryFormManager] Auto-detected subdivision={subdivision}, usePropertyId={usePropertyId}");
    }

    void ListenToUserInquiry(string userId, string subdivisionKey)
    {
        var userInquiryRef = dbRef.Child("user_inquiries").Child(userId).Child(subdivisionKey);

        userInquiryRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted) return;

            if (!task.Result.Exists)
            {
                if (inquiryFormPrefab != null)
                    inquiryFormPrefab.SetActive(true);
                return;
            }

            string inquiryId = task.Result.Value.ToString();
            var inquiryRef = dbRef.Child("inquiries").Child(subdivisionKey).Child(inquiryId);

            InquiryState state = new InquiryState
            {
                inquiryId = inquiryId,
                inquiryRef = inquiryRef,
                isAccepted = false,
                isDeclined = false
            };

            inquiryStates[subdivisionKey] = state;

            inquiryRef.ValueChanged += (sender, e) =>
            {
                if (!e.Snapshot.Exists) return;
                HandleInquirySnapshot(subdivisionKey, e.Snapshot);
            };

            inquiryRef.GetValueAsync().ContinueWithOnMainThread(t =>
            {
                if (t.IsFaulted || !t.Result.Exists) return;
                HandleInquirySnapshot(subdivisionKey, t.Result, initialCheck: true);
            });
        });
    }

    private void HandleInquirySnapshot(string subdivisionKey, DataSnapshot snapshot, bool initialCheck = false)
    {
        string status = snapshot.Child("status").Value?.ToString();
        string agentUid = snapshot.Child("assignedAgentUid").Value?.ToString();

        Debug.Log($"[{subdivisionKey}] status={status}, agentUid={agentUid}");

        if (!inquiryStates.ContainsKey(subdivisionKey))
            return;

        var state = inquiryStates[subdivisionKey];

        if (status == "accepted" && !string.IsNullOrEmpty(agentUid))
        {
            state.isAccepted = true;
            state.isDeclined = false;
            state.assignedAgentUid = agentUid;

            if (inquiryFormPrefab != null)
                inquiryFormPrefab.SetActive(false);

            if (messagingPanelPage != null)
                messagingPanelPage.SetActive(true);

            if (resubmitButton != null)
                resubmitButton.SetActive(false);

            // ✅ Appointment logic
            ToggleAppointments(subdivisionKey, true);

            // ✅ Reservation logic
            CheckReservationAccess();

            OpenChatWithAgent(agentUid);
        }
        else if (status == "pending")
        {
            state.isAccepted = false;
            state.isDeclined = false;
            state.assignedAgentUid = null;

            if (inquiryFormPrefab != null)
                inquiryFormPrefab.SetActive(true);

            if (resubmitButton != null)
                resubmitButton.SetActive(false);

            // ✅ Appointment logic
            ToggleAppointments(subdivisionKey, false);

            // ✅ Reservation logic
            CheckReservationAccess();

            if (!initialCheck)
                ShowError($"⏳ Your {subdivisionKey} inquiry is pending. Waiting for agent...");
        }
        else if (status == "declined")
        {
            state.isAccepted = false;
            state.isDeclined = true;
            state.assignedAgentUid = null;

            if (inquiryFormPrefab != null)
                inquiryFormPrefab.SetActive(true);

            if (resubmitButton != null)
                resubmitButton.SetActive(true);

            // ✅ Appointment logic
            ToggleAppointments(subdivisionKey, false);

            // ✅ Reservation logic
            CheckReservationAccess();

            if (!initialCheck)
                ShowError($"❌ Your {subdivisionKey} inquiry was declined. Please resubmit.");
        }

        inquiryStates[subdivisionKey] = state;
    }

    private void ToggleAppointments(string subdivisionKey, bool accepted)
    {
        if (subdivisionKey == "treelane")
        {
            foreach (var form in treelaneAppointmentForms)
                if (form != null) form.SetActive(accepted);

            foreach (var blocker in treelaneBlockers)
                if (blocker != null) blocker.SetActive(!accepted);
        }
        else if (subdivisionKey == "lancris")
        {
            foreach (var form in lancrisAppointmentForms)
                if (form != null) form.SetActive(accepted);

            foreach (var blocker in lancrisBlockers)
                if (blocker != null) blocker.SetActive(!accepted);
        }
    }

    // ✅ Reservation check: hide blocker if ANY inquiry is accepted
    private void CheckReservationAccess()
    {
        bool hasAccepted = false;
        foreach (var state in inquiryStates.Values)
        {
            if (state.isAccepted)
            {
                hasAccepted = true;
                break;
            }
        }

        if (reservationBlocker != null)
            reservationBlocker.SetActive(!hasAccepted);
    }

    public void SubmitInquiry()
    {
        errorText.text = "";

        string fullName = fullNameInput.text.Trim();
        string email = emailInput.text.Trim();
        string phone = phoneInput.text.Trim();
        string message = messageInput.text.Trim();
        string inquiryType = inquiryTypeDropdown.options[inquiryTypeDropdown.value].text;
        string propertyId = usePropertyId ? propertyDropdown.options[propertyDropdown.value].text.ToLower() : null;

        if (string.IsNullOrEmpty(fullName)) { ShowError("Full name is required."); return; }
        if (string.IsNullOrEmpty(email) || !IsValidEmail(email)) { ShowError("Please enter a valid email address."); return; }
        if (string.IsNullOrEmpty(phone) || !Regex.IsMatch(phone, @"^[0-9]+$")) { ShowError("Phone must contain numbers only."); return; }
        if (string.IsNullOrEmpty(message)) { ShowError("Additional message is required."); return; }
        if (auth.CurrentUser == null) { ShowError("You must be logged in to submit an inquiry."); return; }

        string inquiryId = dbRef.Child("inquiries").Child(subdivision).Push().Key;

        InquiryData inquiry = new InquiryData
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
                    dbRef.Child("user_inquiries").Child(auth.CurrentUser.UserId).Child(subdivision).SetValueAsync(inquiryId);

                    ShowError("✅ Inquiry submitted successfully!", success: true);
                    ClearForm();

                    ListenToUserInquiry(auth.CurrentUser.UserId, subdivision);
                }
                else
                {
                    ShowError("❌ Error submitting inquiry.");
                }
            });
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

    private void OpenChatWithAgent(string agentUid)
    {
        Debug.Log("Opening chat with agent: " + agentUid);
        // TODO: connect to ChatManager
    }

    private class InquiryState
    {
        public string inquiryId;
        public DatabaseReference inquiryRef;
        public bool isAccepted;
        public bool isDeclined;
        public string assignedAgentUid;
    }
}

[Serializable]
public class InquiryData
{
    public string fullName;
    public string email;
    public string phone;
    public string message;
    public string inquiryType;
    public string propertyId;
    public string propertyType;
    public string consent;
    public string status;
    public string assignedAgentUid;
    public long timestamp;
}
