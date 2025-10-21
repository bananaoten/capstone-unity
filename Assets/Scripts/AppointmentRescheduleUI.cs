using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using System;
using System.Collections.Generic;

public class AppointmentRescheduleUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField fullNameInput;
    public TMP_InputField contactNumberInput;
    public TMP_InputField emailInput;
    public TMP_InputField preferredDateInput;
    public TMP_InputField preferredTimeInput; // Changed from Dropdown to InputField
    public TMP_InputField propertyInput;
    public TMP_InputField messageInput;
    public Button submitButton;
    public Button cancelButton;
    public TextMeshProUGUI statusText;

    [Header("Reschedule UI References")]
    public TMP_Dropdown rescheduleTimeDropdown;
    public TMP_InputField rescheduleDateInput;
    public TMP_InputField rescheduleMessageInput;

    private DatabaseReference dbRef;
    private FirebaseAuth auth;
    private string appointmentId;
    private string userUid;
    private bool suppressDateCallback = false;

    private void Awake()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        auth = FirebaseAuth.DefaultInstance;
        userUid = auth.CurrentUser != null ? auth.CurrentUser.UserId : null;

        SetupRescheduleTimeDropdown();

        if (preferredDateInput != null)
            preferredDateInput.onValueChanged.AddListener(OnPreferredDateChanged);

        if (rescheduleDateInput != null)
            rescheduleDateInput.onValueChanged.AddListener(OnRescheduleDateChanged);

        if (contactNumberInput != null)
        {
            contactNumberInput.contentType = TMP_InputField.ContentType.IntegerNumber;
            contactNumberInput.characterLimit = 11;
            contactNumberInput.onValueChanged.AddListener(OnContactChanged);
        }

        if (submitButton != null)
            submitButton.onClick.AddListener(OnSubmitReschedule);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(Hide);

        gameObject.SetActive(false); // Hide initially
    }

    public void Show()
    {
        gameObject.SetActive(true); // Show popup
        transform.SetAsLastSibling(); // Bring to front
    }

    private void OnContactChanged(string value)
    {
        contactNumberInput.text = new string(Array.FindAll(value.ToCharArray(), char.IsDigit));
        if (contactNumberInput.text.Length > 11)
            contactNumberInput.text = contactNumberInput.text.Substring(0, 11);
    }

    private void SetupRescheduleTimeDropdown()
    {
        if (rescheduleTimeDropdown != null)
        {
            rescheduleTimeDropdown.ClearOptions();
            rescheduleTimeDropdown.AddOptions(new List<string>
            {
                "Select Time",
                "07:00am",
                "07:30am",
                "08:00am",
                "08:30am",
                "09:00am",
                "09:30am",
                "10:00am",
                "10:30am",
                "11:00am",
                "01:00pm",
                "01:30pm",
                "02:00pm",
                "02:30pm",
                "03:00pm",
                "03:30pm",
                "04:00pm",
                "04:30pm",
                "05:00pm"
            });
        }
    }

    private void OnPreferredDateChanged(string input)
    {
        if (suppressDateCallback) return;

        string digits = "";
        foreach (char c in input)
            if (char.IsDigit(c)) digits += c;

        string formatted = "";

        if (digits.Length >= 2)
            formatted = digits.Substring(0, 2);
        else if (digits.Length > 0)
            formatted = digits;

        if (digits.Length >= 4)
            formatted += "/" + digits.Substring(2, 2);
        else if (digits.Length > 2)
            formatted += "/" + digits.Substring(2);

        if (digits.Length > 4)
            formatted += "/" + digits.Substring(4, Math.Min(4, digits.Length - 4));

        suppressDateCallback = true;
        preferredDateInput.text = formatted;
        suppressDateCallback = false;

        preferredDateInput.caretPosition = preferredDateInput.text.Length;
    }

    private void OnRescheduleDateChanged(string input)
    {
        if (suppressDateCallback) return;

        string digits = "";
        foreach (char c in input)
            if (char.IsDigit(c)) digits += c;

        string formatted = "";

        if (digits.Length >= 2)
            formatted = digits.Substring(0, 2);
        else if (digits.Length > 0)
            formatted = digits;

        if (digits.Length >= 4)
            formatted += "/" + digits.Substring(2, 2);
        else if (digits.Length > 2)
            formatted += "/" + digits.Substring(2);

        if (digits.Length > 4)
            formatted += "/" + digits.Substring(4, Math.Min(4, digits.Length - 4));

        suppressDateCallback = true;
        rescheduleDateInput.text = formatted;
        suppressDateCallback = false;

        rescheduleDateInput.caretPosition = rescheduleDateInput.text.Length;
    }

    public void LoadAppointment(string appointmentKey)
    {
        Debug.Log("[AppointmentRescheduleUI] Received appointmentId: " + appointmentKey);
        appointmentId = appointmentKey;
        Show();

        Debug.Log("Loading appointment: " + appointmentId);
        dbRef.Child("appointments").Child(appointmentId).GetValueAsync()
        .ContinueWithOnMainThread(task =>
        {
            Debug.Log("Snapshot raw data: " + (task.Result != null ? task.Result.GetRawJsonValue() : "null"));
            if (!task.IsCompleted || task.IsFaulted || !task.Result.Exists)
            {
                SetStatus("Appointment not found.", Color.red);
                return;
            }

            var snapshot = task.Result;

            string fname = snapshot.Child("fname").Value?.ToString() ?? "";
            string mname = snapshot.Child("mname").Value?.ToString() ?? "";
            string lname = snapshot.Child("lname").Value?.ToString() ?? "";
            string contact = snapshot.Child("contact").Value?.ToString() ?? "";
            string email = snapshot.Child("email").Value?.ToString() ?? "";
            string property = snapshot.Child("property").Value?.ToString() ?? "";
            string date = snapshot.Child("date").Value?.ToString() ?? "";
            string time = snapshot.Child("time").Value?.ToString() ?? "";
            string message = snapshot.Child("message").Value?.ToString() ?? "";
            string status = snapshot.Child("status").Value?.ToString() ?? "";

            string fullName = $"{fname} {mname} {lname}".Replace("  ", " ").Trim();

            // Set read-only fields
            fullNameInput.text = fullName;
            fullNameInput.interactable = false;

            contactNumberInput.text = contact;
            contactNumberInput.interactable = false;

            emailInput.text = email;
            emailInput.interactable = false;

            propertyInput.text = property;
            propertyInput.interactable = false;

            // Editable fields (original appointment)
            preferredDateInput.text = date;
            preferredDateInput.interactable = false;

            preferredTimeInput.text = time;
            preferredTimeInput.interactable = false;

            messageInput.text = message;
            messageInput.interactable = false;

            // Clear reschedule fields
            if (rescheduleDateInput != null) rescheduleDateInput.text = "";
            if (rescheduleTimeDropdown != null) rescheduleTimeDropdown.value = 0;
            if (rescheduleMessageInput != null) rescheduleMessageInput.text = "";

            // Status text: Approved (green)
            if (statusText != null)
                SetStatus("Current Status: Approved", new Color(0.2f, 0.8f, 0.2f)); // Green
        });
    }

    private void OnSubmitReschedule()
    {
        string newDate = rescheduleDateInput.text.Trim();
        string newTime = rescheduleTimeDropdown.value > 0 ? rescheduleTimeDropdown.options[rescheduleTimeDropdown.value].text : "";
        string newMessage = rescheduleMessageInput.text.Trim();

        if (string.IsNullOrEmpty(newDate) || string.IsNullOrEmpty(newTime))
        {
            SetStatus("Please select date and time.", Color.red);
            return;
        }

        dbRef.Child("appointments").Child(appointmentId).Child("date").SetValueAsync(newDate);
        dbRef.Child("appointments").Child(appointmentId).Child("time").SetValueAsync(newTime);
        dbRef.Child("appointments").Child(appointmentId).Child("message").SetValueAsync(newMessage);
        dbRef.Child("appointments").Child(appointmentId).Child("status").SetValueAsync("pending");

        // Status text: Pending Reschedule (orange-yellow)
        SetStatus("Current Status: Pending Reschedule", new Color(1f, 0.65f, 0f)); // Orange-yellow
        Invoke(nameof(Hide), 1.2f);
    }

    private void SetStatus(string message, Color color)
    {
        if (statusText)
        {
            statusText.text = message;
            statusText.color = color;
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}