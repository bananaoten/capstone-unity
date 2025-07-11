using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System;

public class AppointmentForm : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField lastNameInput;
    public TMP_InputField firstNameInput;
    public TMP_InputField middleNameInput;
    public TMP_InputField contactNumberInput;
    public TMP_InputField emailAddressInput;
    public TMP_Dropdown propertyDropdown;
    public TMP_InputField preferredDateInput;  // Format: YYYY-MM-DD
    public TMP_InputField preferredTimeInput;  // Format: HH:mm
    public TMP_InputField additionalMessageInput;

    [Header("Message Display")]
    public TMP_Text messageText; // Assign this in the inspector to show messages

    private DatabaseReference dbRef;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                dbRef = FirebaseDatabase.DefaultInstance.RootReference;
            }
            else
            {
                Debug.LogError("Could not resolve Firebase dependencies: " + task.Result);
                ShowMessage("Firebase init error: " + task.Result, true);
            }
        });
    }

    public void SubmitAppointment()
    {
        string validationMessage = ValidateForm();

        if (validationMessage != "")
        {
            ShowMessage(validationMessage, true);
            return;
        }

        string key = dbRef.Child("appointments").Push().Key;

        AppointmentData appointment = new AppointmentData
        {
            lastName = lastNameInput.text.Trim(),
            firstName = firstNameInput.text.Trim(),
            middleName = middleNameInput.text.Trim(),
            contactNumber = contactNumberInput.text.Trim(),
            emailAddress = emailAddressInput.text.Trim(),
            propertyToVisit = propertyDropdown.options[propertyDropdown.value].text,
            preferredDate = preferredDateInput.text.Trim(),
            preferredTime = preferredTimeInput.text.Trim(),
            additionalMessage = additionalMessageInput.text.Trim(),
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };

        string json = JsonUtility.ToJson(appointment);

        dbRef.Child("appointments").Child(key).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                ShowMessage("Appointment submitted successfully!", false);
                ClearForm();
            }
            else
            {
                Debug.LogError("Failed to submit appointment: " + task.Exception);
                ShowMessage("Failed to submit. Please try again.", true);
            }
        });
    }

    private string ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(firstNameInput.text)) return "First Name is required";
        if (string.IsNullOrWhiteSpace(lastNameInput.text)) return "Last Name is required";
        if (string.IsNullOrWhiteSpace(middleNameInput.text)) return "Middle Name is required";
        if (string.IsNullOrWhiteSpace(contactNumberInput.text)) return "Contact Number is required";
        if (string.IsNullOrWhiteSpace(emailAddressInput.text)) return "Email Address is required";
        if (string.IsNullOrWhiteSpace(preferredDateInput.text)) return "Preferred Date is required";
        if (string.IsNullOrWhiteSpace(preferredTimeInput.text)) return "Preferred Time is required";
        return "";
    }

    private void ShowMessage(string message, bool isError)
    {
        if (messageText != null)
        {
            messageText.text = message;
            messageText.color = isError ? Color.red : Color.green;
        }
    }

    private void ClearForm()
    {
        lastNameInput.text = "";
        firstNameInput.text = "";
        middleNameInput.text = "";
        contactNumberInput.text = "";
        emailAddressInput.text = "";
        propertyDropdown.value = 0;
        preferredDateInput.text = "";
        preferredTimeInput.text = "";
        additionalMessageInput.text = "";
    }

    [Serializable]
    public class AppointmentData
    {
        public string lastName;
        public string firstName;
        public string middleName;
        public string contactNumber;
        public string emailAddress;
        public string propertyToVisit;
        public string preferredDate;
        public string preferredTime;
        public string additionalMessage;
        public string timestamp;
    }
}
