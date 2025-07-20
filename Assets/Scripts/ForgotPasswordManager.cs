using UnityEngine;
using TMPro;
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
        auth = FirebaseAuth.DefaultInstance;

        // Ensure only the login canvas is active at start
        loginCanvas.SetActive(true);
        waitingCanvas.SetActive(false);
        successCanvas.SetActive(false);
    }

    // Called when "Send Reset Email" button is clicked
    public void OnResetPasswordButtonClicked()
    {
        string email = emailInputField.text;

        if (string.IsNullOrEmpty(email))
        {
            statusText.text = "⚠ Please enter your email.";
            return;
        }

        auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                statusText.text = "❌ Failed to send reset email. Please check the email address.";
                return;
            }

            statusText.text = $"✅ Reset link sent to {email}";
            loginCanvas.SetActive(false);
            waitingCanvas.SetActive(true);
        });
    }

    // Called when "I've Reset My Password" button is clicked
    public void OnUserConfirmedPasswordReset()
    {
        waitingCanvas.SetActive(false);
        successCanvas.SetActive(true);
        StartCoroutine(SwitchToLoginCanvasAfterDelay(3));
    }

    private IEnumerator SwitchToLoginCanvasAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        successCanvas.SetActive(false);
        loginCanvas.SetActive(true);
    }
}
