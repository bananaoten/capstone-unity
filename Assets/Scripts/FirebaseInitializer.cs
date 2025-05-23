using UnityEngine;
using Firebase;
using System;
using System.Threading.Tasks;

public class FirebaseInitializer : MonoBehaviour
{
    public static FirebaseInitializer Instance { get; private set; }

    // Task completion source to notify others when initialization is done
    private TaskCompletionSource<bool> initTaskSource = new TaskCompletionSource<bool>();

    // Other scripts can await this task to know when Firebase is initialized
    public Task<bool> InitializationTask => initTaskSource.Task;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFirebase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async void InitializeFirebase()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

        if (dependencyStatus == DependencyStatus.Available)
        {
            Debug.Log("Firebase dependencies are available.");
            initTaskSource.SetResult(true);
        }
        else
        {
            Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            initTaskSource.SetResult(false);
        }
    }
}
