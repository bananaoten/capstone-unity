using UnityEngine;
using TMPro;
using Firebase.Database;
using Firebase.Auth;
using System.Collections.Generic;
using Firebase.Extensions; 
using UnityEngine.SceneManagement;

public class MessageBadgeManager : MonoBehaviour
{
    public static MessageBadgeManager Instance; // Singleton

    [Header("Global Badge Settings")]
    [Tooltip("List of all badge objects in different nav bars")]
    public List<GameObject> badgeObjects = new List<GameObject>();
    [Tooltip("List of all badge texts in different nav bars")]
    public List<TextMeshProUGUI> badgeTexts = new List<TextMeshProUGUI>();

    private DatabaseReference dbRef;
    private DatabaseReference treelaneRef;
    private DatabaseReference lancrisRef;

    private int treelaneUnread = 0;
    private int lancrisUnread = 0;
    private int unreadCount = 0;

    private bool needsUIUpdate = false; // 🔑 flag to update UI inside Update()

    public delegate void BadgeUpdated(int count);
    public event BadgeUpdated OnBadgeUpdated; // optional for other scripts

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnEnable()
    {
        // 🔹 Ensure references are refreshed after every scene load
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[BadgeManager] Scene loaded: {scene.name}, refreshing badge references...");
        ValidateBadgeReferences();
        UpdateBadgeUI(); // force refresh
    }

    void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        FirebaseAuth.DefaultInstance.StateChanged += OnAuthStateChanged;

        // Ensure refs exist at startup
        ValidateBadgeReferences();

        ListenForMessages();
    }

    void OnDestroy()
    {
        if (treelaneRef != null) treelaneRef.ValueChanged -= HandleTreelaneUpdate;
        if (lancrisRef != null) lancrisRef.ValueChanged -= HandleLancrisUpdate;

        FirebaseAuth.DefaultInstance.StateChanged -= OnAuthStateChanged;
    }

    private void OnAuthStateChanged(object sender, System.EventArgs e)
    {
        ListenForMessages();
    }

    private void ListenForMessages()
    {
        string userId = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("[BadgeManager] No authenticated user, cannot listen for messages.");
            return;
        }

        // 🔹 Remove old listeners
        if (treelaneRef != null) treelaneRef.ValueChanged -= HandleTreelaneUpdate;
        if (lancrisRef != null) lancrisRef.ValueChanged -= HandleLancrisUpdate;

        // 🔹 Set up both refs
        treelaneRef = dbRef.Child("messages").Child("treelane").Child(userId);
        lancrisRef = dbRef.Child("messages").Child("lancris").Child(userId);

        treelaneRef.ValueChanged += HandleTreelaneUpdate;
        lancrisRef.ValueChanged += HandleLancrisUpdate;

        Debug.Log($"[BadgeManager] Listening for messages (userId={userId})");
    }

    private void HandleTreelaneUpdate(object sender, ValueChangedEventArgs e)
    {
        treelaneUnread = CountUnreadMessages(e);
        UpdateTotalUnread();
        Debug.Log($"[BadgeManager] Treelane unread={treelaneUnread}");
    }

    private void HandleLancrisUpdate(object sender, ValueChangedEventArgs e)
    {
        lancrisUnread = CountUnreadMessages(e);
        UpdateTotalUnread();
        Debug.Log($"[BadgeManager] Lancris unread={lancrisUnread}");
    }

    private int CountUnreadMessages(ValueChangedEventArgs e)
    {
        if (e.DatabaseError != null) return 0;

        int count = 0;
        if (e.Snapshot.Exists && e.Snapshot.HasChildren)
        {
            foreach (var child in e.Snapshot.Children)
            {
                var dict = child.Value as Dictionary<string, object>;
                if (dict != null && dict.ContainsKey("from") && dict.ContainsKey("isRead"))
                {
                    string from = dict["from"].ToString();
                    bool isRead = System.Convert.ToBoolean(dict["isRead"]);

                    if ((from == "admin" || from == "agent") && !isRead)
                        count++;
                }
            }
        }
        return count;
    }

    private void UpdateTotalUnread()
    {
        unreadCount = treelaneUnread + lancrisUnread;
        needsUIUpdate = true;
    }

    void Update()
    {
        if (needsUIUpdate)
        {
            UpdateBadgeUI();
            OnBadgeUpdated?.Invoke(unreadCount);
            needsUIUpdate = false;
        }
    }

    private void UpdateBadgeUI()
    {
        if (badgeObjects.Count == 0 || badgeTexts.Count == 0)
        {
            Debug.LogWarning("[BadgeManager] No badge objects/texts assigned!");
            return;
        }

        Debug.Log($"[BadgeManager] Updating UI: unread={unreadCount}");

        for (int i = 0; i < badgeObjects.Count; i++)
        {
            if (badgeObjects[i] == null || badgeTexts.Count <= i || badgeTexts[i] == null)
            {
                Debug.LogWarning($"[BadgeManager] Missing reference at index {i}");
                continue;
            }

            if (unreadCount > 0)
            {
                badgeObjects[i].SetActive(true);
                badgeTexts[i].text = unreadCount > 99 ? "99+" : unreadCount.ToString();
            }
            else
            {
                badgeObjects[i].SetActive(false);
            }
        }
    }

    // ✅ Clear only nav bar badge, no DB update
    public void ResetNavBarBadge()
    {
        unreadCount = 0;
        UpdateBadgeUI();
        OnBadgeUpdated?.Invoke(unreadCount);
    }

    // ✅ Mark all messages as read in Firebase
    public void MarkAllAsReadInDatabase()
    {
        string userId = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        if (string.IsNullOrEmpty(userId)) return;

        // 🔹 Treelane
        var treelaneMessages = dbRef.Child("messages").Child("treelane").Child(userId);
        treelaneMessages.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                foreach (var child in task.Result.Children)
                {
                    var dict = child.Value as Dictionary<string, object>;
                    if (dict != null && dict.ContainsKey("from") &&
                        (dict["from"].ToString() == "admin" || dict["from"].ToString() == "agent"))
                    {
                        treelaneMessages.Child(child.Key).Child("isRead").SetValueAsync(true);
                    }
                }
            }
        });

        // 🔹 Lancris
        var lancrisMessages = dbRef.Child("messages").Child("lancris").Child(userId);
        lancrisMessages.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                foreach (var child in task.Result.Children)
                {
                    var dict = child.Value as Dictionary<string, object>;
                    if (dict != null && dict.ContainsKey("from") &&
                        (dict["from"].ToString() == "admin" || dict["from"].ToString() == "agent"))
                    {
                        lancrisMessages.Child(child.Key).Child("isRead").SetValueAsync(true);
                    }
                }
            }
        });

        ResetBadge();
    }

    public void ResetBadge()
    {
        treelaneUnread = 0;
        lancrisUnread = 0;
        unreadCount = 0;

        UpdateBadgeUI();
        OnBadgeUpdated?.Invoke(unreadCount);
    }

    // 🔹 Fallback auto-find (active + inactive objects)
    private void ValidateBadgeReferences()
    {
        if (badgeObjects.Count == 0)
        {
            GameObject[] found = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in found)
            {
                if (go.CompareTag("MessageBadge"))
                    badgeObjects.Add(go);
            }
            Debug.Log($"[BadgeManager] Auto-assigned {badgeObjects.Count} badge objects.");
        }

        if (badgeTexts.Count == 0)
        {
            TextMeshProUGUI[] tmps = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
            foreach (var t in tmps)
            {
                if (t.gameObject.name.ToLower().Contains("badge"))
                    badgeTexts.Add(t);
            }
            Debug.Log($"[BadgeManager] Auto-assigned {badgeTexts.Count} badge texts.");
        }
    }
}
