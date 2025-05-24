using UnityEngine;
using TMPro;
using Firebase.Database;
using Firebase.Auth;
using System.Threading.Tasks;

public class TreelaneModelViewManager : MonoBehaviour
{
    public GameObject userHomeCanvas;

    public GameObject propertyDetailsLeftCanvas;
    public GameObject propertyDetailsMiddleCanvas;
    public GameObject propertyDetailsRightCanvas;

    public TMP_Text viewTextLeft;
    public TMP_Text viewTextMiddle;
    public TMP_Text viewTextRight;

    private void Start()
    {
        if (FirebaseInitializer.IsFirebaseReady)
        {
            Initialize();
        }
        else
        {
            FirebaseInitializer.OnFirebaseReady += Initialize;
        }
    }

    private void Initialize()
    {
        if (FirebaseInitializer.Auth.CurrentUser != null)
        {
            Debug.Log("Firebase and user ready.");
            LoadViewCount("treelanecornerleft", viewTextLeft);
            LoadViewCount("treelanemiddle", viewTextMiddle);
            LoadViewCount("treelanecornerright", viewTextRight);
        }
        else
        {
            Debug.LogWarning("No user signed in.");
        }
    }

    public async void OnClickLeftModel()
    {
        await IncrementViewCount("treelanecornerleft", viewTextLeft);
        userHomeCanvas.SetActive(false);
        propertyDetailsLeftCanvas.SetActive(true);
    }

    public async void OnClickMiddleModel()
    {
        await IncrementViewCount("treelanemiddle", viewTextMiddle);
        userHomeCanvas.SetActive(false);
        propertyDetailsMiddleCanvas.SetActive(true);
    }

    public async void OnClickRightModel()
    {
        await IncrementViewCount("treelanecornerright", viewTextRight);
        userHomeCanvas.SetActive(false);
        propertyDetailsRightCanvas.SetActive(true);
    }

    private async Task IncrementViewCount(string modelId, TMP_Text viewText)
    {
        var modelRef = FirebaseInitializer.Database.RootReference.Child("models").Child(modelId).Child("views");

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

    private async void LoadViewCount(string modelId, TMP_Text viewText)
    {
        var modelRef = FirebaseInitializer.Database.RootReference.Child("models").Child(modelId).Child("views");

        DataSnapshot snapshot = await modelRef.GetValueAsync();

        if (snapshot.Exists && int.TryParse(snapshot.Value.ToString(), out int views))
        {
            if (viewText != null)
                viewText.text = $"Views: {views}";
        }
    }
}
