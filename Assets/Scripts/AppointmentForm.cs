using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using System;

public class AppointmentForm : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField firstNameInput;
    public TMP_InputField middleNameInput;
    public TMP_InputField lastNameInput;
    public TMP_InputField contactInput;
    public TMP_InputField emailInput;
    public TMP_InputField propertyInput;
    public TMP_InputField dateInput;
    public TMP_InputField timeInput;
    public TMP_InputField messageInput;
    public Button submitButton;
    public GameObject blockerPanel; // Optional UI blocker until inquiry is accepted
    public TextMeshProUGUI statusText;

    private DatabaseReference dbRef;
    private FirebaseAuth auth;
    private string userUid;

    void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        auth = FirebaseAuth.DefaultInstance;
        userUid = auth.CurrentUser != null ? auth.CurrentUser.UserId : null;

        if (submitButton != null)
            submitButton.onClick.AddListener(OnSubmit);

        // Watch if inquiry is accepted for the property
        if (!string.IsNullOrEmpty(userUid))
            ListenForInquiryAcceptance();
    }

    private void ListenForInquiryAcceptance()
    {
        DatabaseReference inquiriesRef = FirebaseDatabase.DefaultInstance.GetReference("inquiries");

        inquiriesRef.ValueChanged += (sender, args) =>
        {
            if (args.DatabaseError != null) return;

            bool canProceed = false;

            if (args.Snapshot.Exists)
            {
                foreach (var propertyNode in args.Snapshot.Children)
                {
                    foreach (var inquiryNode in propertyNode.Children)
                    {
                        var inquiry = inquiryNode.Value as System.Collections.IDictionary;
                        if (inquiry == null) continue;

                        string inquiryUserUid = inquiry.Contains("userUid") ? inquiry["userUid"].ToString() : "";
                        string status = inquiry.Contains("status") ? inquiry["status"].ToString() : "";
                        string assignedAgentUid = inquiry.Contains("assignedAgentUid") ? inquiry["assignedAgentUid"].ToString() : "";

                        if (inquiryUserUid == userUid && status == "accepted" && !string.IsNullOrEmpty(assignedAgentUid))
                        {
                            canProceed = true;
                            break;
                        }
                    }
                }
            }

            if (blockerPanel != null)
                blockerPanel.SetActive(!canProceed);

            if (statusText != null)
                statusText.text = canProceed
                    ? "You can now book an appointment."
                    : "You cannot proceed to appointment until your inquiry is accepted.";
        };
    }

    private void OnSubmit()
    {
        if (string.IsNullOrEmpty(userUid))
        {
            Debug.LogError("User not logged in.");
            return;
        }

        string firstName = firstNameInput.text.Trim();
        string middleName = middleNameInput.text.Trim();
        string lastName = lastNameInput.text.Trim();
        string contact = contactInput.text.Trim();
        string email = emailInput.text.Trim();
        string property = propertyInput.text.Trim();
        string date = dateInput.text.Trim();
        string time = timeInput.text.Trim();
        string message = messageInput.text.Trim();

        if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(contact) ||
            string.IsNullOrEmpty(email) || string.IsNullOrEmpty(property) || string.IsNullOrEmpty(date) || string.IsNullOrEmpty(time))
        {
            statusText.text = "Please fill in all required fields.";
            return;
        }

        string appointmentId = dbRef.Child("appointments").Push().Key;
        var appointmentData = new
        {
            fname = firstName,
            mname = middleName,
            lname = lastName,
            contact = contact,
            email = email,
            property = property,
            date = date,
            time = time,
            message = message,
            status = "pending",
            submittedAt = DateTime.UtcNow.ToString("o"),
            userUid = userUid,
            approvedBy = "",
            approvedByName = ""
        };

        dbRef.Child("appointments").Child(appointmentId).SetRawJsonValueAsync(JsonUtility.ToJson(appointmentData))
        .ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to submit appointment: " + task.Exception);
                statusText.text = "Failed to submit appointment. Try again.";
            }
            else
            {
                Debug.Log("Appointment submitted successfully.");
                statusText.text = "Appointment submitted successfully. Awaiting approval.";
            }
        });
    }
}
