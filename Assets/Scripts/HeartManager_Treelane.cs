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
        auth = FirebaseAuth.DefaultInstance;
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        SetupModel(treelaneCornerLeft);
        SetupModel(treelaneMiddle);
        SetupModel(treelaneCornerRight);

        await WaitForUserLogin();
    }

    private async Task WaitForUserLogin()
    {
        int retries = 0;
        while ((auth.CurrentUser == null || string.IsNullOrEmpty(auth.CurrentUser.UserId)) && retries < 50)
        {
            await Task.Delay(100);
            retries++;
        }

        if (auth.CurrentUser != null)
        {
            userId = auth.CurrentUser.UserId;
            await LoadAllHeartData();
        }
        else
        {
            Debug.LogWarning("Firebase user not found.");
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

        var heartCountSnap = await modelRef.Child("heartCount").GetValueAsync();
        model.heartCount = 0;
        if (heartCountSnap.Exists) int.TryParse(heartCountSnap.Value.ToString(), out model.heartCount);
        model.heartCountText.text = model.heartCount.ToString();

        var userHeartSnap = await modelRef.Child("heartedUsers").Child(userId).GetValueAsync();
        model.hasHearted = userHeartSnap.Exists && userHeartSnap.Value.ToString() == "true";

        UpdateHeartUI(model);
        if (model.heartButton != null) model.heartButton.interactable = true;
    }

    private async void OnHeartButtonClicked(TreelaneModel model)
    {
        if (string.IsNullOrEmpty(userId)) return;
        if (model.heartButton == null) return;

        var modelRef = dbRef.Child("models").Child(model.modelId);
        var heartedUserRef = modelRef.Child("heartedUsers").Child(userId);

        var currentHeartSnap = await heartedUserRef.GetValueAsync();
        bool isCurrentlyHearted = currentHeartSnap.Exists && currentHeartSnap.Value.ToString() == "true";

        var heartCountSnap = await modelRef.Child("heartCount").GetValueAsync();
        model.heartCount = 0;
        if (heartCountSnap.Exists) int.TryParse(heartCountSnap.Value.ToString(), out model.heartCount);

        if (isCurrentlyHearted)
        {
            model.heartCount = Mathf.Max(model.heartCount - 1, 0);
            await heartedUserRef.RemoveValueAsync();
            model.hasHearted = false;
        }
        else
        {
            model.heartCount += 1;
            await heartedUserRef.SetValueAsync(true);
            model.hasHearted = true;
        }

        await modelRef.Child("heartCount").SetValueAsync(model.heartCount);
        model.heartCountText.text = model.heartCount.ToString();

        UpdateHeartUI(model);

        if (userHomeCanvas != null) userHomeCanvas.SetActive(false);
        if (model.propertyDetailsCanvas != null) model.propertyDetailsCanvas.SetActive(true);
    }

    private void UpdateHeartUI(TreelaneModel model)
    {
        if (model.heartButton != null)
        {
            Image heartImage = model.heartButton.GetComponent<Image>();
            if (heartImage != null)
            {
                heartImage.sprite = model.hasHearted ? heartFilledSprite : heartOutlineSprite;
            }
        }
    }
}