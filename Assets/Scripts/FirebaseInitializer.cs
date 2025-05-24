using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System;
using System.Threading.Tasks;

public class FirebaseInitializer : MonoBehaviour
{
    public static FirebaseAuth Auth { get; private set; }
    public static FirebaseDatabase Database { get; private set; }
    public static bool IsFirebaseReady { get; private set; } = false;

    public static event Action OnFirebaseReady;

    private async void Awake()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            Auth = FirebaseAuth.DefaultInstance;
            Database = FirebaseDatabase.DefaultInstance;
            IsFirebaseReady = true;
            Debug.Log("Firebase initialized.");

            OnFirebaseReady?.Invoke();
        }
        else
        {
            Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            IsFirebaseReady = false;
            // Optionally, you could retry or handle this case as needed.
        }
    }
}
