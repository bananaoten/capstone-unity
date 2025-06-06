using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using System.Text.RegularExpressions;

public class AuthManager : MonoBehaviour
{
    [Header("Login UI")]
    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPasswordInput;
    public TextMeshProUGUI loginErrorText;

    [Header("Sign Up UI")]
    public TMP_InputField signUpEmailInput;
    public TMP_InputField signUpPasswordInput;
    public TMP_InputField signUpConfirmPasswordInput;
    public TextMeshProUGUI signUpErrorText;

    [Header("Canvases")]
    public GameObject loginCanvas;
    public GameObject signUpCanvas;
    public GameObject signupSetupLandingCanvas;
    public GameObject userHomeCanvas;

    [Header("Managers")]
    public ProfileManager profileManager; // Assign in Inspector

    private FirebaseAuth auth;

    void Start()
    {
        loginErrorText.text = "";
        signUpErrorText.text = "";

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                Debug.Log("Firebase initialized.");
            }
            else
            {
                Debug.LogError("Firebase init failed: " + task.Result);
            }
        });
    }

    public void OnLogin()
    {
        string email = loginEmailInput.text.Trim();
        string password = loginPasswordInput.text;

        if (!IsValidEmail(email))
        {
            loginErrorText.text = "Invalid email format.";
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            loginErrorText.text = "Password cannot be empty.";
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                loginErrorText.text = "Login failed: " + GetErrorMessage(task.Exception);
                return;
            }

            FirebaseUser user = task.Result.User;
            loginErrorText.text = "Login successful. Welcome, " + user.Email;

            loginCanvas.SetActive(false);
            userHomeCanvas.SetActive(true);

            // Load and show profile after login
            if (profileManager != null)
            {
                profileManager.LoadAndShowProfileAfterLogin();
            }
        });
    }

    public void OnSignUp()
    {
        string email = signUpEmailInput.text.Trim();
        string password = signUpPasswordInput.text;
        string confirmPassword = signUpConfirmPasswordInput.text;

        if (!IsValidEmail(email))
        {
            signUpErrorText.text = "Invalid email format.";
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            signUpErrorText.text = "Password cannot be empty.";
            return;
        }

        if (password.Length < 8)
        {
            signUpErrorText.text = "Password must be at least 8 characters.";
            return;
        }

        if (password != confirmPassword)
        {
            signUpErrorText.text = "Passwords do not match.";
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                signUpErrorText.text = "Sign up failed: " + GetErrorMessage(task.Exception);
                return;
            }

            FirebaseUser newUser = task.Result.User;
            signUpErrorText.text = "Sign up successful. Welcome, " + newUser.Email;

            signUpCanvas.SetActive(false);
            signupSetupLandingCanvas.SetActive(true);
        });
    }

    private bool IsValidEmail(string email)
    {
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }

   private string GetErrorMessage(System.AggregateException exception)
{
    if (exception == null) return "An unknown error occurred.";

    foreach (var e in exception.InnerExceptions)
    {
        if (e is FirebaseException firebaseEx)
        {
            switch ((Firebase.Auth.AuthError)firebaseEx.ErrorCode)
            {
                case Firebase.Auth.AuthError.InvalidEmail:
                    return "Invalid email address format.";
                case Firebase.Auth.AuthError.WrongPassword:
                case Firebase.Auth.AuthError.UserNotFound:
                    return "Incorrect email or password.";
                case Firebase.Auth.AuthError.UserDisabled:
                    return "Your account has been disabled.";
                case Firebase.Auth.AuthError.WeakPassword:
                    return "Password is too weak.";
                case Firebase.Auth.AuthError.EmailAlreadyInUse:
                    return "Email is already in use.";
                default:
                    break; // Fall through to default message
            }
        }
    }

    // Fallback: don't say internal error — show friendly message
    return "Incorrect email or password.";
}



    public void Logout()
    {
        if (auth != null)
        {
            auth.SignOut();  // Sign out from Firebase
        }

        // Hide user-specific UI and show the landing/login page
        userHomeCanvas.SetActive(false);
        signUpCanvas.SetActive(false);
        signupSetupLandingCanvas.SetActive(false);
        loginCanvas.SetActive(true);
    }
}
