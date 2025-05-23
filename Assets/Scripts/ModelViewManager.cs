using UnityEngine;
using TMPro;
using Firebase.Database;
using Firebase.Auth;
using System.Threading.Tasks;

public class ModelViewManager : MonoBehaviour
{
    public GameObject userHomeCanvas;
    public GameObject propertyDetails1Canvas;

    public TMP_Text viewText; // Text to display view count
    private string modelId = "model1"; // Can be customized for other models

    private DatabaseReference dbReference;
    private FirebaseAuth auth;

    async void Start()
{
    var dependencyStatus = await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();
    if (dependencyStatus == Firebase.DependencyStatus.Available)
    {
        auth = FirebaseAuth.DefaultInstance;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        if (auth.CurrentUser != null)
        {
            Debug.Log("Firebase and user ready.");
            LoadViewCount(); // only after confirming
        }
        else
        {
            Debug.LogWarning("No user signed in.");
        }
    }
    else
    {
        Debug.LogError("Could not resolve Firebase dependencies: " + dependencyStatus);
    }
}


    public async void OnModel1ButtonClick()
    {
        await IncrementViewCount();
        userHomeCanvas.SetActive(false);
        propertyDetails1Canvas.SetActive(true);
    }

    private async Task IncrementViewCount()
    {
        var modelRef = dbReference.Child("models").Child(modelId).Child("views");

        DataSnapshot snapshot = await modelRef.GetValueAsync();

        int currentViews = 0;
        if (snapshot.Exists && int.TryParse(snapshot.Value.ToString(), out int parsedViews))
        {
            currentViews = parsedViews;
        }

        currentViews++;
        await modelRef.SetValueAsync(currentViews);

        if (viewText != null)
            viewText.text = $"Views: {currentViews}";
    }

    private async void LoadViewCount()
    {
        var modelRef = dbReference.Child("models").Child(modelId).Child("views");

        DataSnapshot snapshot = await modelRef.GetValueAsync();

        if (snapshot.Exists && int.TryParse(snapshot.Value.ToString(), out int views))
        {
            if (viewText != null)
                viewText.text = $"Views: {views}";
        }
    }
}
    