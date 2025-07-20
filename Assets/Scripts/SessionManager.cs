using UnityEngine;
using Firebase.Auth;
using Firebase.Extensions;

public class SessionManager : MonoBehaviour
{
    public GameObject loginCanvas;
    public GameObject homeCanvas;

    private FirebaseAuth auth;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;

        FirebaseUser user = auth.CurrentUser;

        if (user != null)
        {
            user.ReloadAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && !task.IsFaulted)
                {
                    if (user.IsEmailVerified)
                    {
                        loginCanvas.SetActive(false);
                        homeCanvas.SetActive(true);
                        Debug.Log("✅ Session restored for " + user.Email);
                    }
                    else
                    {
                        Debug.Log("⛔ Email not verified. Please verify.");
                        loginCanvas.SetActive(true);
                    }
                }
                else
                {
                    Debug.Log("⚠ Failed to restore session.");
                    loginCanvas.SetActive(true);
                }
            });
        }
        else
        {
            loginCanvas.SetActive(true);
        }
    }
}
