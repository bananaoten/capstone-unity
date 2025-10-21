// ...existing code...
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;

public class AppointmentNotificationManager : MonoBehaviour
{
    [Header("UI Containers")]
    public Transform notificationContainer; // Assign: ScrollView Content

    [Header("Notification Prefabs")]
    public GameObject approvedNotificationPrefab;
    public GameObject declinedNotificationPrefab;
    public GameObject pendingNotificationPrefab;

    [Header("Legacy/Generic Inquiry Notification Prefabs")]
    public GameObject inquiryAcceptedNotificationPrefab;
    public GameObject inquiryDeclinedNotificationPrefab;

    [Header("Inquiry Notification Prefabs - Lancris")]
    public GameObject lancrisInquiryAcceptedNotificationPrefab;
    public GameObject lancrisInquiryDeclinedNotificationPrefab;

    [Header("Inquiry Notification Prefabs - Treelane")]
    public GameObject treelaneInquiryAcceptedNotificationPrefab;
    public GameObject treelaneInquiryDeclinedNotificationPrefab;

    private DatabaseReference dbRef;
    private FirebaseAuth auth;

    // track which inquiry listeners we've attached to avoid duplicates
    private HashSet<string> listeningInquiries = new HashSet<string>();

    private void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
    }

    private void Start()
    {
        StartCoroutine(InitializeFirebase());
    }

    private IEnumerator InitializeFirebase()
    {
        var check = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => check.IsCompleted);

        if (check.Result != DependencyStatus.Available)
        {
            Debug.LogError("❌ Firebase dependencies not available: " + check.Result);
            yield break;
        }

        yield return new WaitUntil(() => auth.CurrentUser != null);

        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        string currentUserId = auth.CurrentUser.UserId;

        Debug.Log("✅ Listening for appointment updates for user: " + currentUserId);

        // Load saved notifications first
        StartCoroutine(LoadSavedNotifications(currentUserId));

        // Start listening for changes in appointments
        FirebaseDatabase.DefaultInstance
            .GetReference("appointments")
            .OrderByChild("userUid")
            .EqualTo(currentUserId)
            .ValueChanged += OnAppointmentsChanged;

        // Subscribe to inquiry notifications (accepted/rejected/declined)
        SubscribeToInquiryNotifications(currentUserId);
    }

    private void OnAppointmentsChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Firebase error: " + args.DatabaseError.Message);
            return;
        }

        if (!args.Snapshot.Exists) return;

        foreach (var child in args.Snapshot.Children)
        {
            string status = child.Child("status").Value?.ToString();
            bool notifFlag = false;
            if (child.Child("notification").Value != null)
            {
                bool.TryParse(child.Child("notification").Value.ToString(), out notifFlag);
            }

            if (notifFlag)
            {
                Debug.Log($"🔔 Notification detected for appointment {child.Key}, status = {status}");

                string title = "";
                string description = "";
                NotificationItem.NotificationType type = NotificationItem.NotificationType.Pending;

                if (status == "approved")
                {
                    title = "Appointment Approved";
                    description = "✅ Your appointment has been approved!";
                    type = NotificationItem.NotificationType.Approved;
                }
                else if (status == "declined" || status == "rejected")
                {
                    title = "Appointment Declined";
                    description = "❌ Unfortunately, your appointment was declined. Please contact the admin for more details.";
                    type = NotificationItem.NotificationType.Declined;
                }
                else
                {
                    title = "Appointment Pending";
                    description = "⏳ Your appointment is pending approval.";
                    type = NotificationItem.NotificationType.Pending;
                }

                // appointments use the generic appointment source
                AddNotification(
                    title,
                    description,
                    type,
                    DateTime.UtcNow,
                    child.Key, // appointmentId
                    child.Child("fname").Value?.ToString() ?? "",
                    child.Child("contact").Value?.ToString() ?? "",
                    child.Child("email").Value?.ToString() ?? "",
                    child.Child("date").Value?.ToString() ?? "",
                    child.Child("property").Value?.ToString() ?? "",
                    null, // prefabOverride
                    "appointment" // source
                );

                // Mark as read in Firebase
                dbRef.Child("appointments").Child(child.Key).Child("notification").SetValueAsync(false);
            }
        }
    }

    private void SubscribeToInquiryNotifications(string currentUserId)
    {
        var userInquiriesRef = dbRef.Child("user_inquiries").Child(currentUserId);

        userInquiriesRef.ValueChanged += (s, e) =>
        {
            if (e.Snapshot == null || !e.Snapshot.Exists) return;

            foreach (var child in e.Snapshot.Children)
            {
                string subdivision = child.Key;
                string inquiryId = child.Value?.ToString();
                if (!string.IsNullOrEmpty(inquiryId))
                {
                    AttachInquiryListener(subdivision, inquiryId);
                }
            }
        };

        userInquiriesRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || !task.Result.Exists) return;

            foreach (var child in task.Result.Children)
            {
                string subdivision = child.Key;
                string inquiryId = child.Value?.ToString();
                if (!string.IsNullOrEmpty(inquiryId))
                {
                    AttachInquiryListener(subdivision, inquiryId);
                }
            }
        });
    }

    private void AttachInquiryListener(string subdivision, string inquiryId)
    {
        string listenKey = $"{subdivision}/{inquiryId}";
        if (listeningInquiries.Contains(listenKey)) return;

        var inquiryRef = dbRef.Child("inquiries").Child(subdivision).Child(inquiryId);
        inquiryRef.ValueChanged += (s, e) =>
        {
            if (e.Snapshot == null || !e.Snapshot.Exists) return;
            HandleInquiryNotificationSnapshot(subdivision, inquiryId, e.Snapshot);
        };

        inquiryRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || !task.Result.Exists) return;
            HandleInquiryNotificationSnapshot(subdivision, inquiryId, task.Result);
        });

        listeningInquiries.Add(listenKey);
    }

    // ...existing code...
    private void HandleInquiryNotificationSnapshot(string subdivision, string inquiryId, DataSnapshot snapshot)
    {
        // normalize subdivision to avoid casing mismatch
        subdivision = subdivision?.ToLowerInvariant();

        string statusRaw = snapshot.Child("status").Value?.ToString();
        string status = statusRaw?.ToLowerInvariant();
        bool notifSent = false;
        if (snapshot.Child("notificationSent").Value != null)
        {
            bool.TryParse(snapshot.Child("notificationSent").Value.ToString(), out notifSent);
        }

        // choose correct prefab based on subdivision + status
        GameObject selectedPrefab = null;
        string source = $"inquiry/{subdivision}"; // will be saved so loader can pick correct prefab later

        if (status == "accepted" && !notifSent)
        {
            string title = "Inquiry Accepted";
            string description = "✅ Your inquiry has been accepted. An agent will contact you shortly.";
            NotificationItem.NotificationType type = NotificationItem.NotificationType.Approved;

            // prefer subdivision-specific accepted prefab, then generic inquiry accepted, then approvedNotificationPrefab
            if (subdivision == "lancris" && lancrisInquiryAcceptedNotificationPrefab != null)
                selectedPrefab = lancrisInquiryAcceptedNotificationPrefab;
            else if (subdivision == "treelane" && treelaneInquiryAcceptedNotificationPrefab != null)
                selectedPrefab = treelaneInquiryAcceptedNotificationPrefab;
            else if (inquiryAcceptedNotificationPrefab != null)
                selectedPrefab = inquiryAcceptedNotificationPrefab;
            else
                selectedPrefab = approvedNotificationPrefab;

            Debug.Log($"[Notifications] Inquiry accepted -> subdivision={subdivision}, using prefab={(selectedPrefab!=null?selectedPrefab.name:"NULL")}, inquiryId={inquiryId}");

            AddNotification(
                title,
                description,
                type,
                DateTime.UtcNow,
                inquiryId,
                snapshot.Child("fullName").Value?.ToString() ?? "",
                snapshot.Child("phone").Value?.ToString() ?? "",
                snapshot.Child("email").Value?.ToString() ?? "",
                "",
                snapshot.Child("propertyId").Value?.ToString() ?? "",
                selectedPrefab,
                source
            );

            dbRef.Child("inquiries").Child(subdivision).Child(inquiryId).Child("notificationSent").SetValueAsync(true);
        }
        else if ((status == "declined" || status == "rejected") && !notifSent)
        {
            string title = "Inquiry Declined";
            string description = "❌ Your inquiry was declined. You may resubmit or contact support for more details.";
            NotificationItem.NotificationType type = NotificationItem.NotificationType.Declined;

            // prefer subdivision-specific declined prefab, then generic inquiry declined, then declinedNotificationPrefab
            if (subdivision == "lancris" && lancrisInquiryDeclinedNotificationPrefab != null)
                selectedPrefab = lancrisInquiryDeclinedNotificationPrefab;
            else if (subdivision == "treelane" && treelaneInquiryDeclinedNotificationPrefab != null)
                selectedPrefab = treelaneInquiryDeclinedNotificationPrefab;
            else if (inquiryDeclinedNotificationPrefab != null)
                selectedPrefab = inquiryDeclinedNotificationPrefab;
            else
                selectedPrefab = declinedNotificationPrefab;

            Debug.Log($"[Notifications] Inquiry declined -> subdivision={subdivision}, using prefab={(selectedPrefab!=null?selectedPrefab.name:"NULL")}, inquiryId={inquiryId}");

            AddNotification(
                title,
                description,
                type,
                DateTime.UtcNow,
                inquiryId,
                snapshot.Child("fullName").Value?.ToString() ?? "",
                snapshot.Child("phone").Value?.ToString() ?? "",
                snapshot.Child("email").Value?.ToString() ?? "",
                "",
                snapshot.Child("propertyId").Value?.ToString() ?? "",
                selectedPrefab,
                source
            );

            dbRef.Child("inquiries").Child(subdivision).Child(inquiryId).Child("notificationSent").SetValueAsync(true);
        }
    }
// ...existing code...

   // ...existing code...
    private void AddNotification(
        string title,
        string description,
        NotificationItem.NotificationType type,
        DateTime timestamp,
        string appointmentId = "",
        string fullName = "",
        string contact = "",
        string email = "",
        string date = "",
        string property = "",
        GameObject prefabOverride = null,
        string source = "appointment") // source indicates where notification came from (appointment | inquiry/lancris | inquiry/treelane)
    {
        if (notificationContainer == null)
        {
            Debug.LogWarning("⚠️ Notification container not assigned!");
            return;
        }

        GameObject prefabToUse = pendingNotificationPrefab;
        if (prefabOverride != null)
        {
            prefabToUse = prefabOverride;
        }
        else if (type == NotificationItem.NotificationType.Approved)
        {
            prefabToUse = approvedNotificationPrefab;
        }
        else if (type == NotificationItem.NotificationType.Declined)
        {
            prefabToUse = declinedNotificationPrefab;
        }

        Debug.Log($"[Notifications] AddNotification: source={source}, type={type}, prefab={(prefabToUse!=null?prefabToUse.name:"NULL")}");

        GameObject notifGO = Instantiate(prefabToUse, notificationContainer);
        notifGO.transform.SetSiblingIndex(0);

        NotificationItem notifItem = notifGO.GetComponent<NotificationItem>();
        if (notifItem != null)
        {
            notifItem.SetData(title, description, type, timestamp, appointmentId, fullName, contact, email, date, property);
        }

        SaveNotificationToDatabase(title, description, type, timestamp, appointmentId, fullName, contact, email, date, property, source);
    }
// ...existing code...

    private void SaveNotificationToDatabase(
        string title,
        string description,
        NotificationItem.NotificationType type,
        DateTime timestamp,
        string appointmentId = "",
        string fullName = "",
        string contact = "",
        string email = "",
        string date = "",
        string property = "",
        string source = "")
    {
        if (auth == null || auth.CurrentUser == null) return;

        string userId = auth.CurrentUser.UserId;
        string notifKey = dbRef.Child("user_notifications").Child(userId).Push().Key;

        var notifData = new Dictionary<string, object>
        {
            { "title", title },
            { "description", description },
            { "type", type.ToString() },
            { "timestamp", timestamp.ToString("o") },
            { "appointmentId", appointmentId },
            { "fullName", fullName },
            { "contact", contact },
            { "email", email },
            { "date", date },
            { "property", property },
            { "source", source }
        };

        dbRef.Child("user_notifications").Child(userId).Child(notifKey).SetValueAsync(notifData);
    }

   // ...existing code...
    private IEnumerator LoadSavedNotifications(string userId)
    {
        var task = dbRef.Child("user_notifications").Child(userId).GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsFaulted)
        {
            Debug.LogError("❌ Failed to load saved notifications: " + task.Exception);
            yield break;
        }

        if (!task.Result.Exists) yield break;

        List<DataSnapshot> notifications = new List<DataSnapshot>();
        foreach (var notif in task.Result.Children)
        {
            notifications.Add(notif);
        }

        // Sort ascending (oldest -> newest) so that when we insert each at sibling index 0
        // the last inserted (newest) ends up at the top.
        notifications.Sort((a, b) =>
        {
            DateTime timeA = DateTime.Parse(a.Child("timestamp").Value.ToString());
            DateTime timeB = DateTime.Parse(b.Child("timestamp").Value.ToString());
            return timeA.CompareTo(timeB); // oldest first
        });

        foreach (var notif in notifications)
        {
            string title = notif.Child("title").Value?.ToString();
            string description = notif.Child("description").Value?.ToString();
            string typeStr = notif.Child("type").Value?.ToString();
            string timeStr = notif.Child("timestamp").Value?.ToString();

            string appointmentId = notif.Child("appointmentId").Value?.ToString() ?? "";
            string fullName = notif.Child("fullName").Value?.ToString() ?? "";
            string contact = notif.Child("contact").Value?.ToString() ?? "";
            string email = notif.Child("email").Value?.ToString() ?? "";
            string date = notif.Child("date").Value?.ToString() ?? "";
            string property = notif.Child("property").Value?.ToString() ?? "";

            Enum.TryParse(typeStr, out NotificationItem.NotificationType type);
            DateTime.TryParse(timeStr, out DateTime time);

            GameObject prefabToUse = pendingNotificationPrefab;
            string source = notif.Child("source").Value?.ToString() ?? "appointment";

            // (existing prefab selection logic...)
            if (source.StartsWith("inquiry"))
            {
                string[] parts = source.Split('/');
                string subdivision = parts.Length > 1 ? parts[1] : "";

                if (subdivision == "lancris")
                {
                    if (type == NotificationItem.NotificationType.Approved && lancrisInquiryAcceptedNotificationPrefab != null)
                        prefabToUse = lancrisInquiryAcceptedNotificationPrefab;
                    else if (type == NotificationItem.NotificationType.Declined && lancrisInquiryDeclinedNotificationPrefab != null)
                        prefabToUse = lancrisInquiryDeclinedNotificationPrefab;
                    else
                        prefabToUse = (type == NotificationItem.NotificationType.Approved) ? approvedNotificationPrefab : (type == NotificationItem.NotificationType.Declined ? declinedNotificationPrefab : pendingNotificationPrefab);
                }
                else if (subdivision == "treelane")
                {
                    if (type == NotificationItem.NotificationType.Approved && treelaneInquiryAcceptedNotificationPrefab != null)
                        prefabToUse = treelaneInquiryAcceptedNotificationPrefab;
                    else if (type == NotificationItem.NotificationType.Declined && treelaneInquiryDeclinedNotificationPrefab != null)
                        prefabToUse = treelaneInquiryDeclinedNotificationPrefab;
                    else
                        prefabToUse = (type == NotificationItem.NotificationType.Approved) ? approvedNotificationPrefab : (type == NotificationItem.NotificationType.Declined ? declinedNotificationPrefab : pendingNotificationPrefab);
                }
                else
                {
                    if (type == NotificationItem.NotificationType.Approved && inquiryAcceptedNotificationPrefab != null)
                        prefabToUse = inquiryAcceptedNotificationPrefab;
                    else if (type == NotificationItem.NotificationType.Declined && inquiryDeclinedNotificationPrefab != null)
                        prefabToUse = inquiryDeclinedNotificationPrefab;
                    else
                        prefabToUse = (type == NotificationItem.NotificationType.Approved) ? approvedNotificationPrefab : (type == NotificationItem.NotificationType.Declined ? declinedNotificationPrefab : pendingNotificationPrefab);
                }
            }
            else // appointment or unknown
            {
                if (type == NotificationItem.NotificationType.Approved)
                    prefabToUse = approvedNotificationPrefab;
                else if (type == NotificationItem.NotificationType.Declined)
                    prefabToUse = declinedNotificationPrefab;
                else
                    prefabToUse = pendingNotificationPrefab;
            }

            GameObject notifGO = Instantiate(prefabToUse, notificationContainer);
            // put newest at top by always inserting at index 0 while iterating oldest->newest
            notifGO.transform.SetSiblingIndex(0);

            NotificationItem notifItem = notifGO.GetComponent<NotificationItem>();
            if (notifItem != null)
            {
                notifItem.SetData(title, description, type, time, appointmentId, fullName, contact, email, date, property);
            }
        }
    }
// ...existing code...

    // optional helpers if you want explicit fallbacks elsewhere
    private GameObject FallbackAcceptedPrefabForSubdivision(string subdivision)
    {
        if (subdivision == "lancris" && lancrisInquiryAcceptedNotificationPrefab != null) return lancrisInquiryAcceptedNotificationPrefab;
        if (subdivision == "treelane" && treelaneInquiryAcceptedNotificationPrefab != null) return treelaneInquiryAcceptedNotificationPrefab;
        if (inquiryAcceptedNotificationPrefab != null) return inquiryAcceptedNotificationPrefab;
        return approvedNotificationPrefab;
    }

    private GameObject FallbackDeclinedPrefabForSubdivision(string subdivision)
    {
        if (subdivision == "lancris" && lancrisInquiryDeclinedNotificationPrefab != null) return lancrisInquiryDeclinedNotificationPrefab;
        if (subdivision == "treelane" && treelaneInquiryDeclinedNotificationPrefab != null) return treelaneInquiryDeclinedNotificationPrefab;
        if (inquiryDeclinedNotificationPrefab != null) return inquiryDeclinedNotificationPrefab;
        return declinedNotificationPrefab;
    }

    private GameObject inquiryAccepted_notification_prefab_or(GameObject fallback)
    {
        return inquiryAcceptedNotificationPrefab != null ? inquiryAcceptedNotificationPrefab : fallback;
    }

    private GameObject inquiryDeclined_notification_prefab_or(GameObject fallback)
    {
        return inquiryDeclinedNotificationPrefab != null ? inquiryDeclinedNotificationPrefab : fallback;
    }
}
// ...existing code...