using UnityEngine;
using TMPro;
using Firebase.Database;
using Firebase.Auth;
using System.Collections.Generic;
using Firebase.Extensions; // ✅ for ContinueWithOnMainThread

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

    void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        FirebaseAuth.DefaultInstance.StateChanged += OnAuthStateChanged;
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
        if (string.IsNullOrEmpty(userId)) return;

        // 🔹 Remove old listeners
        if (treelaneRef != null) treelaneRef.ValueChanged -= HandleTreelaneUpdate;
        if (lancrisRef != null) lancrisRef.ValueChanged -= HandleLancrisUpdate;

        // 🔹 Set up both refs
        treelaneRef = dbRef.Child("messages").Child("treelane").Child(userId);
        lancrisRef = dbRef.Child("messages").Child("lancris").Child(userId);

        treelaneRef.ValueChanged += HandleTreelaneUpdate;
        lancrisRef.ValueChanged += HandleLancrisUpdate;
    }

    private void HandleTreelaneUpdate(object sender, ValueChangedEventArgs e)
    {
        treelaneUnread = CountUnreadMessages(e);
        UpdateTotalUnread();
    }

    private void HandleLancrisUpdate(object sender, ValueChangedEventArgs e)
    {
        lancrisUnread = CountUnreadMessages(e);
        UpdateTotalUnread();
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
        for (int i = 0; i < badgeObjects.Count; i++)
        {
            if (badgeObjects[i] == null || badgeTexts[i] == null) continue;

            if (unreadCount > 0)
            {
                badgeObjects[i].SetActive(true);
                badgeTexts[i].text = unreadCount.ToString();
            }
            else
            {
                badgeObjects[i].SetActive(false);
            }
        }
    }

    public void MarkAllAsReadInDatabase()
    {
        string userId = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        if (string.IsNullOrEmpty(userId)) return;

        // 🔹 Mark Treelane as read
        var treelaneMessages = dbRef.Child("messages").Child("treelane").Child(userId);
        treelaneMessages.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                foreach (var child in task.Result.Children)
                {
                    var dict = child.Value as Dictionary<string, object>;
                    if (dict != null && dict.ContainsKey("from") && dict["from"].ToString() == "admin")
                    {
                        treelaneMessages.Child(child.Key).Child("isRead").SetValueAsync(true);
                    }
                }
            }
        });

        // 🔹 Mark Lancris as read
        var lancrisMessages = dbRef.Child("messages").Child("lancris").Child(userId);
        lancrisMessages.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                foreach (var child in task.Result.Children)
                {
                    var dict = child.Value as Dictionary<string, object>;
                    if (dict != null && dict.ContainsKey("from") && dict["from"].ToString() == "admin")
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
}
