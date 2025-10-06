using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using System;
using System.Text.RegularExpressions;

public class AppointmentForm : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField firstNameInput;    // Auto-filled from Firebase Profile
    public TMP_InputField middleNameInput;   // Optional
    public TMP_InputField lastNameInput;     // Auto-filled from Firebase Profile
    public TMP_InputField contactInput;
    public TMP_InputField emailInput;        // Pre-filled from Firebase Auth, read-only
    public TMP_Dropdown propertyDropdown;
    public TMP_InputField dateInput;         // Auto-formatted MM/DD/YYYY
    public TMP_Dropdown timeDropdown;        // Dropdown for preferred time
    public TMP_InputField messageInput;
    public TextMeshProUGUI statusText;

    private DatabaseReference dbRef;
    private FirebaseAuth auth;
    private string userUid;

    private bool suppressDateCallback = false;

    [Serializable]
    public class AppointmentData
    {
        public string fname;
        public string mname;
        public string lname;
        public string contact;
        public string email;
        public string property;
        public string date;
        public string time;
        public string message;
        public string status;
        public string submittedAt;
        public string userUid;

        // Agent info
        public string assignedAgentUid;
        public string assignedAgentName;
    }

    void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        auth = FirebaseAuth.DefaultInstance;
        userUid = auth.CurrentUser != null ? auth.CurrentUser.UserId : null;

        if (auth.CurrentUser != null)
        {
            // ✅ Pre-fill email from Firebase Auth (read-only)
            emailInput.text = auth.CurrentUser.Email;
            emailInput.interactable = false;

            // ✅ Fetch user profile (FullName + Contact)
            dbRef.Child("users").Child(userUid).Child("profile").GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && task.Result.Exists)
                {
                    var snapshot = task.Result;
                    string fullName = snapshot.Child("FullName").Value?.ToString();
                    string contact = snapshot.Child("ContactNumber").Value?.ToString();

                    if (!string.IsNullOrEmpty(fullName))
                    {
                        string[] nameParts = fullName.Split(' ');
                        if (nameParts.Length > 0) firstNameInput.text = nameParts[0];
                        if (nameParts.Length > 1) lastNameInput.text = nameParts[nameParts.Length - 1];
                    }

                    if (!string.IsNullOrEmpty(contact))
                        contactInput.text = contact;
                }
            });
        }

        // ✅ Restrict contact input to 11 digits only
        contactInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        contactInput.characterLimit = 11;
        contactInput.onValueChanged.AddListener(OnContactChanged);

        // ✅ Populate time dropdown options
        if (timeDropdown != null && timeDropdown.options.Count == 0)
        {
            timeDropdown.options.Clear();
            timeDropdown.options.Add(new TMP_Dropdown.OptionData("08:00 AM"));
            timeDropdown.options.Add(new TMP_Dropdown.OptionData("09:00 AM"));
            timeDropdown.options.Add(new TMP_Dropdown.OptionData("10:00 AM"));
            timeDropdown.options.Add(new TMP_Dropdown.OptionData("11:00 AM"));
            timeDropdown.options.Add(new TMP_Dropdown.OptionData("01:00 PM"));
            timeDropdown.options.Add(new TMP_Dropdown.OptionData("02:00 PM"));
            timeDropdown.options.Add(new TMP_Dropdown.OptionData("03:00 PM"));
            timeDropdown.options.Add(new TMP_Dropdown.OptionData("04:00 PM"));
            timeDropdown.RefreshShownValue();
        }

        // ✅ Date input auto-format
        if (dateInput != null)
            dateInput.onValueChanged.AddListener(OnPreferredDateChanged);
    }

    private void OnContactChanged(string value)
    {
        // only allow digits, max 11
        contactInput.text = new string(Array.FindAll(value.ToCharArray(), char.IsDigit));
        if (contactInput.text.Length > 11)
            contactInput.text = contactInput.text.Substring(0, 11);
    }

    private void OnPreferredDateChanged(string input)
    {
        if (suppressDateCallback) return;

        string digits = "";
        foreach (char c in input)
        {
            if (char.IsDigit(c))
                digits += c;
        }

        string formatted = "";

        if (digits.Length >= 2)
        {
            int month = int.Parse(digits.Substring(0, 2));
            if (month > 12) month = 12;
            formatted = month.ToString("00");
        }
        else if (digits.Length > 0)
        {
            formatted = digits;
        }

        if (digits.Length >= 4)
        {
            int day = int.Parse(digits.Substring(2, 2));
            if (day > 31) day = 31;
            formatted += day.ToString("00");
        }
        else if (digits.Length > 2)
        {
            formatted += digits.Substring(2);
        }

        if (digits.Length > 4)
        {
            string year = digits.Substring(4);
            formatted += year;
        }

        if (formatted.Length > 2) formatted = formatted.Insert(2, "/");
        if (formatted.Length > 5) formatted = formatted.Insert(5, "/");
        if (formatted.Length > 10) formatted = formatted.Substring(0, 10);

        suppressDateCallback = true;
        dateInput.text = formatted;
        suppressDateCallback = false;

        dateInput.caretPosition = dateInput.text.Length;
    }

    // Public so you can hook it directly in Button OnClick
    public void OnSubmit()
    {
        if (string.IsNullOrEmpty(userUid))
        {
            Debug.LogError("❌ User not logged in.");
            if (statusText != null)
                statusText.text = "You must be logged in to submit.";
            return;
        }

        // Collect inputs...
        string firstName = firstNameInput.text.Trim();
        string middleName = middleNameInput.text.Trim(); // optional
        string lastName = lastNameInput.text.Trim();
        string contact = contactInput.text.Trim();
        string email = emailInput.text.Trim();
        string property = propertyDropdown != null && propertyDropdown.options.Count > 0
            ? propertyDropdown.options[propertyDropdown.value].text.ToLower()
            : "";
        string date = dateInput.text.Trim();
        string time = timeDropdown.options[timeDropdown.value].text;
        string message = messageInput.text.Trim();

        // Required field check
        if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) ||
            string.IsNullOrEmpty(contact) || string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(property) || string.IsNullOrEmpty(date) ||
            string.IsNullOrEmpty(time))
        {
            if (statusText != null)
                statusText.text = "⚠ Please fill in all required fields.";
            return;
        }

        // ✅ Philippine number validation
        if (!Regex.IsMatch(contact, @"^09\d{9}$"))
        {
            if (statusText != null)
                statusText.text = "⚠ Contact must be 11 digits and start with 09.";
            return;
        }

        // 🔑 Link to accepted inquiry
        dbRef.Child("user_inquiries").Child(userUid).Child(property).GetValueAsync()
        .ContinueWithOnMainThread(task =>
        {
            if (!task.IsCompleted || !task.Result.Exists)
            {
                Debug.LogError("❌ No accepted inquiry found for this property.");
                if (statusText != null)
                    statusText.text = "You must have an accepted inquiry before booking.";
                return;
            }

            string inquiryId = task.Result.Value.ToString();
            dbRef.Child("inquiries").Child(property).Child(inquiryId).GetValueAsync()
            .ContinueWithOnMainThread(inquiryTask =>
            {
                if (!inquiryTask.IsCompleted || !inquiryTask.Result.Exists)
                {
                    Debug.LogError("❌ Inquiry data missing.");
                    return;
                }

                string assignedAgentUid = inquiryTask.Result.Child("assignedAgentUid").Value?.ToString();
                string assignedAgentName = inquiryTask.Result.Child("assignedAgentName").Value?.ToString();

                if (string.IsNullOrEmpty(assignedAgentUid))
                {
                    Debug.LogError("❌ Inquiry not yet accepted by any agent.");
                    if (statusText != null)
                        statusText.text = "Wait until an agent accepts your inquiry before booking.";
                    return;
                }

                // 🔑 Create appointment linked to agent
                string appointmentId = dbRef.Child("appointments").Push().Key;

                AppointmentData appointmentData = new AppointmentData
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
                    assignedAgentUid = assignedAgentUid,
                    assignedAgentName = assignedAgentName
                };

                string appointmentJson = JsonUtility.ToJson(appointmentData);

                dbRef.Child("appointments").Child(appointmentId).SetRawJsonValueAsync(appointmentJson)
                .ContinueWithOnMainThread(saveTask =>
                {
                    if (saveTask.IsFaulted)
                    {
                        Debug.LogError("❌ Failed to submit appointment: " + saveTask.Exception);
                        if (statusText != null)
                            statusText.text = "Failed to submit appointment. Try again.";
                    }
                    else
                    {
                        Debug.Log("✅ Appointment submitted successfully: " + appointmentId);
                        if (statusText != null)
                            statusText.text = "⏳ Appointment submitted. Awaiting agent approval...";
                        ListenForAppointmentUpdates(appointmentId);
                    }
                });
            });
        });
    }

    private void ListenForAppointmentUpdates(string appointmentId)
    {
        DatabaseReference appointmentRef = dbRef.Child("appointments").Child(appointmentId);

        appointmentRef.ValueChanged += (sender, args) =>
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError("Database error: " + args.DatabaseError.Message);
                return;
            }

            if (!args.Snapshot.Exists) return;

            var data = args.Snapshot.Value as System.Collections.IDictionary;
            if (data == null) return;

            string status = data.Contains("status") ? data["status"].ToString() : "pending";
            string agentName = data.Contains("assignedAgentName") ? data["assignedAgentName"].ToString() : "";

            if (statusText != null)
            {
                if (status == "approved")
                    statusText.text = $"✅ Approved by {agentName}";
                else if (status == "declined")
                    statusText.text = "❌ Declined";
                else
                    statusText.text = "⏳ Pending approval...";
            }
        };
    }
}
