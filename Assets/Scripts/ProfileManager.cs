using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;
using Firebase.Database;

public class ProfileManager : MonoBehaviour
{
    [Header("UI Canvases")]
    public GameObject landingPage;
    public GameObject profileSetupPage;
    public GameObject profilePage;
    public GameObject welcomePage;

    [Header("Principal Buyer Form Reference")]
    public PrincipalBuyerFormUpdated principalBuyerForm;

    [Header("Input Fields (Setup Page)")]
    public TMP_InputField firstNameInput;
    public TMP_InputField lastNameInput;
    public TMP_InputField contactNumberInput;

    [Header("Input Fields (Profile Update)")]
    public TMP_InputField updateFirstNameInput;
    public TMP_InputField updateLastNameInput;
    public TMP_InputField oldPasswordInput;
    public TMP_InputField newPasswordInput;

    [Header("Text Fields (Profile Page)")]
    public TMP_Text fullNameText;
    public TMP_Text contactNumberText;

    [Header("Validation Messages")]
    public TMP_Text setupValidationText;
    public TMP_Text updateValidationText;

    [Header("Buttons")]
    public Button proceedButton;
    public Button updateButton;

    private FirebaseAuth auth;
    private bool firebaseReady = false;

    void Start()
    {
        SetAllPagesInactive();

        // 🔒 Enforce digits-only & 11-digit max for PH contact numbers
        if (contactNumberInput != null)
        {
            contactNumberInput.contentType = TMP_InputField.ContentType.IntegerNumber;
            contactNumberInput.characterLimit = 11;
            contactNumberInput.onValueChanged.AddListener(OnContactNumberChanged);
        }

        if (FirebaseInitializer.IsFirebaseReady)
        {
            InitializeFirebase();
        }
        else
        {
            FirebaseInitializer.OnFirebaseReady += InitializeFirebase;
        }

        if (proceedButton != null)
            proceedButton.onClick.AddListener(OnProceedButtonClicked);

        if (updateButton != null)
            updateButton.onClick.AddListener(OnUpdateProfileClicked);

        // Load locally saved profile info every time scene starts
        LoadProfileDataFromLocal();
    }

    private void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        firebaseReady = true;
        Debug.Log("Firebase is ready (ProfileManager).");

        FirebaseInitializer.OnFirebaseReady -= InitializeFirebase;
    }

    public void StartProfileFlow()
    {
        if (auth == null || auth.CurrentUser == null)
        {
            Debug.Log("No user logged in.");
            return;
        }

        bool profileCompleted = PlayerPrefs.GetInt("ProfileCompleted", 0) == 1;

        if (profileCompleted)
        {
            ShowWelcomePage();
            LoadProfileData();
        }
        else
        {
            ShowProfileSetupPage();
        }
    }

    public void ShowProfileSetupPage()
    {
        SetAllPagesInactive();
        profileSetupPage.SetActive(true);
        setupValidationText.text = "";
    }

    public void ShowWelcomePage()
    {
        SetAllPagesInactive();
        welcomePage.SetActive(true);
    }

    public void OnProceedButtonClicked()
    {
        Debug.Log("Proceed button clicked.");
        if (!firebaseReady)
        {
            setupValidationText.text = "Please wait... Firebase is not ready.";
            return;
        }

        if (auth.CurrentUser == null)
        {
            setupValidationText.text = "User not logged in.";
            return;
        }

        SaveProfileAsync();
    }

    public async void SaveProfileAsync()
    {
        setupValidationText.text = "";

        string firstName = firstNameInput.text.Trim();
        string lastName = lastNameInput.text.Trim();
        string contact = contactNumberInput.text.Trim();

        if (string.IsNullOrEmpty(firstName))
        {
            setupValidationText.text = "First Name is required.";
            return;
        }

        if (string.IsNullOrEmpty(lastName))
        {
            setupValidationText.text = "Last Name is required.";
            return;
        }

        // ✅ PH contact number validation
        if (string.IsNullOrEmpty(contact) || !IsValidPHContact(contact))
        {
            setupValidationText.text = "Invalid Contact Number. Must be 11 digits and start with '09'.";
            return;
        }

        if (auth.CurrentUser == null)
        {
            setupValidationText.text = "User not logged in.";
            return;
        }

        string userId = auth.CurrentUser.UserId;
        string email = auth.CurrentUser.Email;

        var profileData = new UserProfileData()
        {
            FirstName = firstName,
            LastName = lastName,
            ContactNumber = contact,
            Email = email
        };

        string json = JsonUtility.ToJson(profileData);

        try
        {
            var reference = FirebaseDatabase.DefaultInstance.RootReference;
            await reference.Child("users").Child(userId).Child("profile").SetRawJsonValueAsync(json);

            PlayerPrefs.SetInt("ProfileCompleted", 1);

            // Save locally for persistence between scenes
            SaveProfileDataToLocal(profileData);
            PlayerPrefs.Save();

            // 👉 Sync to Principal Buyer form
            SyncProfileToPrincipalBuyerForm(profileData);

            ShowWelcomePage();
            LoadProfileData();
        }
        catch (System.Exception ex)
        {
            setupValidationText.text = "Failed to save profile: " + ex.Message;
            Debug.LogError("Error saving profile: " + ex.Message);
        }
    }

    public async void LoadProfileData()
    {
        if (auth == null || auth.CurrentUser == null)
        {
            Debug.LogError("User not logged in.");
            return;
        }

        string userId = auth.CurrentUser.UserId;

        try
        {
            var reference = FirebaseDatabase.DefaultInstance.RootReference;
            var snapshot = await reference.Child("users").Child(userId).Child("profile").GetValueAsync();

            if (snapshot.Exists)
            {
                var json = snapshot.GetRawJsonValue();
                UserProfileData data = JsonUtility.FromJson<UserProfileData>(json);

                fullNameText.text = data.FullName;
                contactNumberText.text = data.ContactNumber;
                updateFirstNameInput.text = data.FirstName;
                updateLastNameInput.text = data.LastName;

                // Save locally
                SaveProfileDataToLocal(data);

                // 👉 Sync to Principal Buyer form
                SyncProfileToPrincipalBuyerForm(data);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error loading profile: " + ex.Message);
        }
    }

    public async void OnUpdateProfileClicked()
    {
        updateValidationText.text = "";
        updateValidationText.color = Color.red;

        string firstName = updateFirstNameInput.text.Trim();
        string lastName = updateLastNameInput.text.Trim();
        string oldPassword = oldPasswordInput.text;
        string newPassword = newPasswordInput.text;

        FirebaseUser user = auth.CurrentUser;
        if (user == null)
        {
            updateValidationText.text = "User not logged in.";
            return;
        }

        // ✅ Case 1: Update just names
        if (!string.IsNullOrEmpty(firstName) || !string.IsNullOrEmpty(lastName))
        {
            var profileUpdate = new UserProfileData()
            {
                FirstName = firstName,
                LastName = lastName,
                ContactNumber = contactNumberText.text,
                Email = user.Email
            };

            try
            {
                string json = JsonUtility.ToJson(profileUpdate);
                var reference = FirebaseDatabase.DefaultInstance.RootReference;
                await reference.Child("users").Child(user.UserId).Child("profile").SetRawJsonValueAsync(json);

                fullNameText.text = profileUpdate.FullName;
                updateFirstNameInput.text = profileUpdate.FirstName;
                updateLastNameInput.text = profileUpdate.LastName;

                SaveProfileDataToLocal(profileUpdate);
                SyncProfileToPrincipalBuyerForm(profileUpdate);

                updateValidationText.color = Color.green;
                updateValidationText.text = "Profile updated successfully!";
            }
            catch (System.Exception ex)
            {
                updateValidationText.text = "Failed to update: " + ex.Message;
            }
        }

        // ✅ Case 2: Update password
        if (!string.IsNullOrEmpty(oldPassword) && !string.IsNullOrEmpty(newPassword))
        {
            if (oldPassword == newPassword)
            {
                updateValidationText.text = "New password must be different from the old password.";
                return;
            }

            if (newPassword.Length < 8)
            {
                updateValidationText.text = "New password must be at least 8 characters.";
                return;
            }

            var credential = Firebase.Auth.EmailAuthProvider.GetCredential(user.Email, oldPassword);

            try
            {
                await user.ReauthenticateAsync(credential);
                await user.UpdatePasswordAsync(newPassword);

                updateValidationText.color = Color.green;
                updateValidationText.text = "Password updated successfully!";
            }
            catch
            {
                updateValidationText.text = "Old password is incorrect.";
                return;
            }
        }
    }

    // 🔒 Prevents letters/symbols if pasted
    private void OnContactNumberChanged(string value)
    {
        contactNumberInput.text = new string(System.Array.FindAll(value.ToCharArray(), char.IsDigit));
    }

    private bool IsValidPHContact(string contact)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(contact, @"^(09)\d{9}$");
    }

    private void SetAllPagesInactive()
    {
        if (landingPage != null) landingPage.SetActive(false);
        if (profileSetupPage != null) profileSetupPage.SetActive(false);
        if (profilePage != null) profilePage.SetActive(false);
        if (welcomePage != null) welcomePage.SetActive(false);
    }

    private void SaveProfileDataToLocal(UserProfileData data)
    {
        PlayerPrefs.SetString("FirstName", data.FirstName);
        PlayerPrefs.SetString("LastName", data.LastName);
        PlayerPrefs.SetString("FullName", data.FullName);
        PlayerPrefs.SetString("ContactNumber", data.ContactNumber);
        PlayerPrefs.Save();
    }

    private void LoadProfileDataFromLocal()
    {
        string firstName = PlayerPrefs.GetString("FirstName", "");
        string lastName = PlayerPrefs.GetString("LastName", "");
        string fullName = PlayerPrefs.GetString("FullName", "");
        string contactNumber = PlayerPrefs.GetString("ContactNumber", "");

        if (!string.IsNullOrEmpty(firstName) || !string.IsNullOrEmpty(lastName))
        {
            fullNameText.text = $"{firstName} {lastName}".Trim();
            updateFirstNameInput.text = firstName;
            updateLastNameInput.text = lastName;
        }
        else if (!string.IsNullOrEmpty(fullName))
        {
            fullNameText.text = fullName;
        }

        if (!string.IsNullOrEmpty(contactNumber))
        {
            contactNumberText.text = contactNumber;
        }

        if (!string.IsNullOrEmpty(firstName) || !string.IsNullOrEmpty(lastName) || !string.IsNullOrEmpty(contactNumber))
        {
            var localData = new UserProfileData
            {
                FirstName = firstName,
                LastName = lastName,
                ContactNumber = contactNumber,
                Email = ""
            };

            SyncProfileToPrincipalBuyerForm(localData);
        }
    }

    public void LoadAndShowProfileAfterLogin()
    {
        ShowWelcomePage();
        LoadProfileData();
    }

    private void SyncProfileToPrincipalBuyerForm(UserProfileData data)
    {
        if (principalBuyerForm != null)
        {
            if (principalBuyerForm.firstName != null)
                principalBuyerForm.firstName.text = data.FirstName;

            if (principalBuyerForm.lastName != null)
                principalBuyerForm.lastName.text = data.LastName;

            if (principalBuyerForm.contactNumber != null)
                principalBuyerForm.contactNumber.text = data.ContactNumber;
        }
    }
}

[System.Serializable]
public class UserProfileData
{
    public string FirstName;
    public string LastName;
    public string ContactNumber;
    public string Email;

    public string FullName => $"{FirstName} {LastName}".Trim();
}
