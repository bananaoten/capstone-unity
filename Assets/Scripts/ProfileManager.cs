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

    [Header("Input Fields (Setup Page)")]
    public TMP_InputField firstNameInput;
    public TMP_InputField lastNameInput;
    public TMP_InputField contactNumberInput;

    [Header("Input Fields (Profile Update)")]
    public TMP_InputField updateFullNameInput;
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

        // Removed landingPage auto-show - call StartProfileFlow manually after login

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

        if (string.IsNullOrEmpty(contact) || !IsDigitsOnly(contact))
        {
            setupValidationText.text = "Contact Number must contain digits only.";
            return;
        }

        if (auth.CurrentUser == null)
        {
            setupValidationText.text = "User not logged in.";
            return;
        }

        string fullName = firstName + " " + lastName;
        string userId = auth.CurrentUser.UserId;
        string email = auth.CurrentUser.Email;

        var profileData = new UserProfileData()
        {
            FullName = fullName,
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
                updateFullNameInput.text = data.FullName;

                // Save locally for persistence between scenes
                SaveProfileDataToLocal(data);
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

        string fullName = updateFullNameInput.text.Trim();
        string oldPassword = oldPasswordInput.text;
        string newPassword = newPasswordInput.text;

        if (string.IsNullOrEmpty(fullName))
        {
            updateValidationText.text = "Full Name cannot be empty.";
            return;
        }

        if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword))
        {
            updateValidationText.text = "Both old and new password fields are required.";
            return;
        }

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

        FirebaseUser user = auth.CurrentUser;
        if (user == null)
        {
            updateValidationText.text = "User not logged in.";
            return;
        }

        var credential = Firebase.Auth.EmailAuthProvider.GetCredential(user.Email, oldPassword);

        try
        {
            await user.ReauthenticateAsync(credential);
        }
        catch
        {
            updateValidationText.text = "Old password is incorrect.";
            return;
        }

        try
        {
            await user.UpdatePasswordAsync(newPassword);

            string userId = user.UserId;
            var reference = FirebaseDatabase.DefaultInstance.RootReference;

            var profileUpdate = new UserProfileData()
            {
                FullName = fullName,
                ContactNumber = contactNumberText.text,
                Email = user.Email
            };

            string json = JsonUtility.ToJson(profileUpdate);
            await reference.Child("users").Child(userId).Child("profile").SetRawJsonValueAsync(json);

            fullNameText.text = fullName;
            updateFullNameInput.text = fullName;

            // Save locally
            SaveProfileDataToLocal(profileUpdate);

            updateValidationText.color = Color.green;
            updateValidationText.text = "Profile updated successfully!";
        }
        catch (System.Exception ex)
        {
            updateValidationText.text = "Failed to update: " + ex.Message;
        }
    }

    private bool IsDigitsOnly(string str)
    {
        foreach (char c in str)
        {
            if (!char.IsDigit(c)) return false;
        }
        return true;
    }

    private void SetAllPagesInactive()
    {
        if (landingPage != null) landingPage.SetActive(false);
        if (profileSetupPage != null) profileSetupPage.SetActive(false);
        if (profilePage != null) profilePage.SetActive(false);
        if (welcomePage != null) welcomePage.SetActive(false);
    }

    // Save profile data locally in PlayerPrefs for persistence between scenes
    private void SaveProfileDataToLocal(UserProfileData data)
    {
        PlayerPrefs.SetString("FullName", data.FullName);
        PlayerPrefs.SetString("ContactNumber", data.ContactNumber);
        PlayerPrefs.Save();
    }

    // Load profile data from PlayerPrefs and update UI
    private void LoadProfileDataFromLocal()
    {
        string fullName = PlayerPrefs.GetString("FullName", "");
        string contactNumber = PlayerPrefs.GetString("ContactNumber", "");

        if (!string.IsNullOrEmpty(fullName))
        {
            fullNameText.text = fullName;
            updateFullNameInput.text = fullName;
        }

        if (!string.IsNullOrEmpty(contactNumber))
        {
            contactNumberText.text = contactNumber;
        }
    }

    public void LoadAndShowProfileAfterLogin()
    {
        ShowWelcomePage();
        LoadProfileData();
    }
}

[System.Serializable]
public class UserProfileData
{
    public string FullName;
    public string ContactNumber;
    public string Email;
}
