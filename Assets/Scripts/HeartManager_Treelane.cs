using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;
using Firebase.Database;
using System.Threading.Tasks;

public class HeartManager_Treelane : MonoBehaviour
{
    [System.Serializable]
    public class TreelaneModel
    {
        public string modelId;
        public Button heartButton;
        public TMP_Text heartCountText;
        public GameObject propertyDetailsCanvas;
        [HideInInspector] public bool hasHearted;
        [HideInInspector] public int heartCount;
    }

    [Header("Treelane Models")]
    public GameObject userHomeCanvas;
    public TreelaneModel treelaneCornerLeft;
    public TreelaneModel treelaneMiddle;
    public TreelaneModel treelaneCornerRight;

    [Header("Heart Sprites")]
    public Sprite heartOutlineSprite;
    public Sprite heartFilledSprite;

    private FirebaseAuth auth;
    private DatabaseReference dbRef;
    private string userId;

    async void Start()
    {
        if (FirebaseInitializer.IsFirebaseReady)
        {
            InitializeFirebase();
            await WaitForUserLoginAndLoad();
        }
        else
        {
            FirebaseInitializer.OnFirebaseReady += OnFirebaseReady;
        }
    }

    private async void OnFirebaseReady()
    {
        FirebaseInitializer.OnFirebaseReady -= OnFirebaseReady;
        InitializeFirebase();
        await WaitForUserLoginAndLoad();
    }

    private void InitializeFirebase()
    {
        auth = FirebaseInitializer.Auth;
        dbRef = FirebaseInitializer.Database.RootReference;
        Debug.Log("HeartManager_Treelane Firebase Initialized");
    }

    private async Task WaitForUserLoginAndLoad()
    {
        int retries = 0;
        while (auth.CurrentUser == null && retries < 50)
        {
            await Task.Delay(100);
            retries++;
        }

        if (auth.CurrentUser != null)
        {
            userId = auth.CurrentUser.UserId;
            Debug.Log("HeartManager_Treelane: User logged in: " + userId);

            SetupModel(treelaneCornerLeft);
            SetupModel(treelaneMiddle);
            SetupModel(treelaneCornerRight);

            await LoadAllHeartData();
        }
        else
        {
            Debug.LogWarning("HeartManager_Treelane: Firebase user not found.");
        }
    }

    private void SetupModel(TreelaneModel model)
    {
        if (model.heartButton != null)
        {
            model.heartButton.interactable = false;
            model.heartButton.onClick.AddListener(() => OnHeartButtonClicked(model));
        }
    }

    private async Task LoadAllHeartData()
    {
        await LoadHeartData(treelaneCornerLeft);
        await LoadHeartData(treelaneMiddle);
        await LoadHeartData(treelaneCornerRight);
    }

    private async Task LoadHeartData(TreelaneModel model)
    {
        if (model.heartCountText == null || string.IsNullOrEmpty(model.modelId)) return;

        var modelRef = dbRef.Child("models").Child(model.modelId);

        // Load heart count
        var heartCountSnap = await modelRef.Child("heartCount").GetValueAsync();
        model.heartCount = 0;
        if (heartCountSnap.Exists) int.TryParse(heartCountSnap.Value.ToString(), out model.heartCount);
        model.heartCountText.text = model.heartCount.ToString();

        // Load user heart state
        var userHeartSnap = await modelRef.Child("heartedUsers").Child(userId).GetValueAsync();
        model.hasHearted = userHeartSnap.Exists && userHeartSnap.Value.ToString() == "true";

        UpdateHeartUI(model);

        if (model.heartButton != null)
        {
            model.heartButton.interactable = true;
            Debug.Log($"Heart button for {model.modelId} now interactable");
        }
    }

    private async void OnHeartButtonClicked(TreelaneModel model)
    {
        if (string.IsNullOrEmpty(userId) || model.heartButton == null) return;

        var modelRef = dbRef.Child("models").Child(model.modelId);
        var heartedUserRef = modelRef.Child("heartedUsers").Child(userId);

        var heartCountSnap = await modelRef.Child("heartCount").GetValueAsync();
        model.heartCount = 0;
        if (heartCountSnap.Exists) int.TryParse(heartCountSnap.Value.ToString(), out model.heartCount);

        if (model.hasHearted)
        {
            model.heartCount = Mathf.Max(0, model.heartCount - 1);
            model.hasHearted = false;
            await heartedUserRef.RemoveValueAsync();
        }
        else
        {
            model.heartCount += 1;
            model.hasHearted = true;
            await heartedUserRef.SetValueAsync(true);
        }

        await modelRef.Child("heartCount").SetValueAsync(model.heartCount);

        model.heartCountText.text = model.heartCount.ToString();
        UpdateHeartUI(model);

        // Toggle canvases
        if (userHomeCanvas != null) userHomeCanvas.SetActive(false);
        if (model.propertyDetailsCanvas != null) model.propertyDetailsCanvas.SetActive(true);
    }

    private void UpdateHeartUI(TreelaneModel model)
    {
        if (model.heartButton == null) return;

        var heartImage = model.heartButton.GetComponent<Image>();
        if (heartImage != null)
        {
            heartImage.sprite = model.hasHearted ? heartFilledSprite : heartOutlineSprite;
        }
    }
}
