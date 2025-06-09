using UnityEngine;
using Firebase;  // For FirebaseException
using TMPro;
using Firebase.Auth;
using Firebase.Extensions;
using System.Text.RegularExpressions;
using System.Collections;
using UnityEngine.UI; // Needed for Toggle

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

    [Header("Terms & Conditions")]
    public Toggle termsToggle;
    public TextMeshProUGUI termsErrorText;

    [Header("Canvases")]
    public GameObject loginCanvas;
    public GameObject signUpCanvas;
    public GameObject signupSetupLandingCanvas;
    public GameObject userHomeCanvas;

    [Header("Managers")]
    public ProfileManager profileManager;

    private FirebaseAuth auth => FirebaseInitializer.Auth;

    void Start()
    {
        loginErrorText.text = "";
        signUpErrorText.text = "";
        if (termsErrorText != null) termsErrorText.text = "";
    }

    public void OnLogin()
    {
        string email = loginEmailInput.text.Trim();
        string password = loginPasswordInput.text;

        if (!IsValidEmail(email) || string.IsNullOrEmpty(password))
        {
            loginErrorText.text = "Incorrect Email or Password.";
            return;
        }

        if (!FirebaseInitializer.IsFirebaseReady || auth == null)
        {
            loginErrorText.text = "Initializing Firebase, please wait...";
            StartCoroutine(RetryAfterDelay(() => OnLogin(), 1f));
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

            ClearLoginInputs();

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

        signUpErrorText.text = "";
        termsErrorText.text = "";

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

        if (termsToggle == null || !termsToggle.isOn)
        {
            termsErrorText.text = "Terms and Conditions is required.";
            return;
        }

        if (!FirebaseInitializer.IsFirebaseReady || auth == null)
        {
            signUpErrorText.text = "Initializing Firebase, please wait...";
            StartCoroutine(RetryAfterDelay(() => OnSignUp(), 1f));
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

            ClearSignUpInputs();
        });
    }

    private IEnumerator RetryAfterDelay(System.Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
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
                }
            }
        }

        return "Incorrect email or password.";
    }

    public void Logout()
    {
        if (auth != null)
        {
            auth.SignOut();
        }

        userHomeCanvas.SetActive(false);
        signUpCanvas.SetActive(false);
        signupSetupLandingCanvas.SetActive(false);
        loginCanvas.SetActive(true);

        ClearLoginInputs();
        ClearSignUpInputs();
    }

    private void ClearLoginInputs()
    {
        loginEmailInput.text = "";
        loginPasswordInput.text = "";
        loginErrorText.text = "";
    }

    private void ClearSignUpInputs()
    {
        signUpEmailInput.text = "";
        signUpPasswordInput.text = "";
        signUpConfirmPasswordInput.text = "";
        signUpErrorText.text = "";

        if (termsToggle != null)
            termsToggle.isOn = false;

        if (termsErrorText != null)
            termsErrorText.text = "";
    }
}
