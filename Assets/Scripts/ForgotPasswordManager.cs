using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using System.Collections;

public class ForgotPasswordManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField emailInputField;
    public TextMeshProUGUI statusText;

    [Header("Canvases")]
    public GameObject loginCanvas;
    public GameObject waitingCanvas;
    public GameObject successCanvas;

    private FirebaseAuth auth;

    void Start()
    {
        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var status = task.Result;
            if (status == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                Debug.Log("Firebase ready.");
                statusText.text = "Enter your email to reset password.";
            }
            else
            {
                Debug.LogError($"Could not resolve Firebase dependencies: {status}");
                statusText.text = $"⚠ Firebase error: {status}";
            }
        });
    }

    public void OnResetPasswordButtonClicked()
    {
        string email = emailInputField.text.Trim();

        if (string.IsNullOrEmpty(email))
        {
            statusText.text = "⚠ Please enter your email.";
            return;
        }

        statusText.text = "Sending reset email...";

        // Attempt to initialize auth if not done yet
        if (auth == null)
            auth = FirebaseAuth.DefaultInstance;

        auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Reset email failed: " + task.Exception);
                statusText.text = "❌ Failed to send reset email. Please check the email address.";
                return;
            }

            Debug.Log("Password reset email sent to: " + email);
            statusText.text = $"✅ Reset link sent to {email}";

            if (waitingCanvas != null)
                ShowCanvas(waitingCanvas);
        });
    }

    public void OnUserConfirmedPasswordReset()
    {
        if (successCanvas != null)
        {
            ShowCanvas(successCanvas);
            StartCoroutine(SwitchToLoginAfterDelay(3f));
        }
    }

    private IEnumerator SwitchToLoginAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (loginCanvas != null)
            ShowCanvas(loginCanvas);
    }

    private void ShowCanvas(GameObject activeCanvas)
    {
        if (loginCanvas != null) loginCanvas.SetActive(false);
        if (waitingCanvas != null) waitingCanvas.SetActive(false);
        if (successCanvas != null) successCanvas.SetActive(false);

        activeCanvas.SetActive(true);
        if (statusText != null) statusText.text = "";
    }
}
