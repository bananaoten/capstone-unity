using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;
using Firebase.Database;
using System.Threading.Tasks;

public class ProfileManager : MonoBehaviour
{
    [Header("UI Canvases")]
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
    public TMP_Text setupValidationText;      // For setup page validation
    public TMP_Text updateValidationText;     // For update profile validation (assign in inspector)

    [Header("Buttons")]
    public Button proceedButton;
    public Button updateButton;

    private FirebaseAuth auth;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        SetAllPagesInactive();

        if (proceedButton != null)
            proceedButton.onClick.AddListener(OnProceedButtonClicked);

        if (updateButton != null)
            updateButton.onClick.AddListener(OnUpdateProfileClicked);
    }

    public void StartProfileFlow()
    {
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
        profileSetupPage.SetActive(true);
        profilePage.SetActive(false);
        welcomePage.SetActive(false);
        setupValidationText.text = "";
    }

    public void ShowWelcomePage()
    {
        profileSetupPage.SetActive(false);
        profilePage.SetActive(false);
        welcomePage.SetActive(true);
    }

    public void OnProceedButtonClicked()
    {
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
        string email = auth.CurrentUser.Email; // ✅ Get the email

        var profileData = new UserProfileData()
        {
            FullName = fullName,
            ContactNumber = contact,
            Email = email // ✅ Store email
        };

        string json = JsonUtility.ToJson(profileData);

        try
        {
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
            await reference.Child("users").Child(userId).Child("profile").SetRawJsonValueAsync(json);

            PlayerPrefs.SetInt("ProfileCompleted", 1);
            PlayerPrefs.Save();

            ShowWelcomePage();
            LoadProfileData();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error saving profile: " + ex.Message);
        }
    }

    public async void LoadProfileData()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("User not logged in.");
            return;
        }

        string userId = auth.CurrentUser.UserId;

        try
        {
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
            DataSnapshot snapshot = await reference.Child("users").Child(userId).Child("profile").GetValueAsync();

            if (snapshot.Exists)
            {
                var json = snapshot.GetRawJsonValue();
                UserProfileData data = JsonUtility.FromJson<UserProfileData>(json);

                fullNameText.text = data.FullName;
                contactNumberText.text = data.ContactNumber;
                updateFullNameInput.text = data.FullName;

                Debug.Log("Loaded Email: " + data.Email); // Optional: See if email loads
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error loading profile: " + ex.Message);
        }
    }

    public void LoadAndShowProfileAfterLogin()
    {
        ShowWelcomePage();
        LoadProfileData();
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

        if (newPassword.Length < 6)
        {
            updateValidationText.text = "New password must be at least 6 characters.";
            return;
        }

        FirebaseUser user = auth.CurrentUser;
        if (user == null)
        {
            updateValidationText.text = "User not logged in.";
            return;
        }

        var credential = EmailAuthProvider.GetCredential(user.Email, oldPassword);

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

            // Update name in Firebase Realtime Database
            string userId = user.UserId;
            var reference = FirebaseDatabase.DefaultInstance.RootReference;

            var profileUpdate = new UserProfileData()
            {
                FullName = fullName,
                ContactNumber = contactNumberText.text,
                Email = user.Email // ✅ Keep email up to date
            };

            string json = JsonUtility.ToJson(profileUpdate);
            await reference.Child("users").Child(userId).Child("profile").SetRawJsonValueAsync(json);

            // Update UI
            fullNameText.text = fullName;
            updateFullNameInput.text = fullName;

            updateValidationText.color = Color.green;
            updateValidationText.text = "Profile updated successfully!";
        }
        catch (System.Exception ex)
        {
            updateValidationText.text = "Failed to update password: " + ex.Message;
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
        if (profileSetupPage != null) profileSetupPage.SetActive(false);
        if (profilePage != null) profilePage.SetActive(false);
        if (welcomePage != null) profilePage.SetActive(false);
    }
}

[System.Serializable]
public class UserProfileData    
{
    public string FullName;
    public string ContactNumber;
    public string Email; // ✅ Added email
}
