using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine.UI;
using System.Collections;

public class EmailVerificationManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI messageText;

    [Header("Canvases")]
    public GameObject waitingForVerificationCanvas;
    public GameObject verificationSuccessCanvas;
    public GameObject nextPageCanvas;

    [Header("Resend Email Button")]
    public Button resendButton;

    [Header("Verification Timeout (seconds)")]
    public float verificationTimeout = 300f; // 5 minutes default

    private FirebaseAuth auth;
    private Coroutine timeoutCoroutine;

    private void Start()
    {
        CheckFirebaseDependencies();
    }

    private void CheckFirebaseDependencies()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var status = task.Result;
            if (status == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                messageText.text = "Firebase initialized.";
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {status}");
                messageText.text = $"Firebase init failed: {status}";
            }
        });
    }

    /// <summary>
    /// Sends a verification email and disables the resend button temporarily.
    /// </summary>
   public void SendVerificationEmail()
{
    if (auth == null)
    {
        // Try to get FirebaseAuth instance anyway
        auth = FirebaseAuth.DefaultInstance;
    }

    FirebaseUser user = auth?.CurrentUser;

    if (user == null)
    {
        messageText.text = "Please try again.";
        Debug.LogWarning("SendVerificationEmail called but no user is logged in.");
        return;
    }

    if (user.IsEmailVerified)
    {
        messageText.text = "Email already verified.";
        return;
    }

    if (resendButton != null)
        resendButton.interactable = false;

    user.SendEmailVerificationAsync().ContinueWithOnMainThread(task =>
    {
        if (task.IsFaulted || task.IsCanceled)
        {
            Debug.LogError("Failed to send verification email: " + task.Exception);
            messageText.text = "Failed to send verification email.";
            if (resendButton != null)
                resendButton.interactable = true;
            return;
        }

        messageText.text = $"Verification email sent to {user.Email}. Please check your Spam or Inbox";
        StartCoroutine(ReenableButtonAfterDelay(10f));
        Debug.Log("Verification email sent successfully.");
    });
}

    public void StartVerificationTimeout()
    {
        if (timeoutCoroutine != null)
            StopCoroutine(timeoutCoroutine);

        timeoutCoroutine = StartCoroutine(VerificationTimeoutCoroutine());
    }

    private IEnumerator VerificationTimeoutCoroutine()
    {
        Debug.Log($"Verification timeout started: waiting {verificationTimeout} seconds.");
        yield return new WaitForSeconds(verificationTimeout);

        Debug.Log("Verification timeout reached, checking user status...");

        FirebaseUser user = auth.CurrentUser;

        if (user == null)
        {
            Debug.LogWarning("No user logged in at timeout check.");
            yield break;
        }

        yield return user.ReloadAsync();

        Debug.Log($"User email verified? {user.IsEmailVerified}");

        if (!user.IsEmailVerified)
        {
            Debug.LogWarning($"Deleting unverified account: {user.Email}");
            user.DeleteAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && !task.IsFaulted)
                {
                    Debug.Log("Account deleted due to unverified email.");
                    messageText.text = "Email not verified in time. Account deleted.";
                    auth.SignOut();
                    ShowWaitingCanvas();
                }
                else
                {
                    Debug.LogError("Failed to delete unverified account: " + task.Exception);
                    messageText.text = "Failed to delete unverified account.";
                }
            });
        }
        else
        {
            Debug.Log("User already verified; no deletion needed.");
        }
    }

    private IEnumerator ReenableButtonAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (resendButton != null)
            resendButton.interactable = true;
    }

    /// <summary>
    /// Activates the waiting canvas and hides the others.
    /// </summary>
    public void ShowWaitingCanvas()
    {
        waitingForVerificationCanvas.SetActive(true);
        verificationSuccessCanvas.SetActive(false);
        nextPageCanvas.SetActive(false);

        messageText.text = "Please check your email for a verification link.";
    }

    /// <summary>
    /// Checks if the current user's email is verified.
    /// </summary>
    public void CheckIfEmailVerified()
    {
        if (auth == null)
        {
            messageText.text = "Firebase not initialized.";
            return;
        }

        FirebaseUser user = auth.CurrentUser;

        if (user == null)
        {
            messageText.text = "No user is signed in.";
            return;
        }

        user.ReloadAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Error checking email verification: " + task.Exception);
                messageText.text = "Error checking email verification.";
                return;
            }

            if (user.IsEmailVerified)
            {
                messageText.text = "Email verification successful!";
                waitingForVerificationCanvas.SetActive(false);
                verificationSuccessCanvas.SetActive(true);
                Invoke(nameof(ProceedToNextPage), 1.5f);
            }
            else
            {
                messageText.text = "Email not verified yet. Please check your inbox.";
            }
        });
    }

    private void ProceedToNextPage()
    {
        verificationSuccessCanvas.SetActive(false);
        nextPageCanvas.SetActive(true);
    }
}
