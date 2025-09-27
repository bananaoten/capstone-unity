using UnityEngine;
using Firebase.Database;
using System;

public class AppointmentNotificationManager : MonoBehaviour
{
    [Header("UI")]
    public Transform notificationContainer;   // ✅ Assign: Content of ScrollView
    public GameObject notificationPrefab;     // ✅ Assign: Prefab with NotificationItem script

    private DatabaseReference dbRef;

    void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        // Listen for appointment changes
        dbRef.Child("appointments").ValueChanged += OnAppointmentsChanged;
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

            // Read notification flag
            bool notif = false;
            if (child.Child("notification").Value != null)
            {
                bool.TryParse(child.Child("notification").Value.ToString(), out notif);
            }

            if (notif)
            {
                // Determine title/description/type
                string title = "";
                string description = "";
                NotificationItem.NotificationType type = NotificationItem.NotificationType.Pending;

                if (status == "approved")
                {
                    title = "Appointment Approved";
                    description = "✅ Your appointment has been approved!";
                    type = NotificationItem.NotificationType.Approved;
                }
                else if (status == "declined")
                {
                    title = "Appointment Declined";
                    description = "❌ Please contact admin for more details.";
                    type = NotificationItem.NotificationType.Declined;
                }
                else
                {
                    title = "Appointment Update";
                    description = "Your appointment status changed.";
                    type = NotificationItem.NotificationType.Pending;
                }

                // Add notification UI
                AddNotification(title, description, type, DateTime.UtcNow);

                // Reset notification so it only shows once
                dbRef.Child("appointments").Child(child.Key).Child("notification").SetValueAsync(false);
            }
        }
    }

    private void AddNotification(string title, string description, NotificationItem.NotificationType type, DateTime timestamp)
    {
        if (notificationPrefab == null || notificationContainer == null)
        {
            Debug.LogWarning("⚠️ Notification prefab or container is not assigned!");
            return;
        }

        GameObject notifGO = Instantiate(notificationPrefab, notificationContainer);

        // Use NotificationItem component
        NotificationItem notifItem = notifGO.GetComponent<NotificationItem>();
        if (notifItem != null)
        {
            notifItem.SetData(title, description, type, timestamp);
        }
        else
        {
            Debug.LogWarning("⚠️ Notification prefab is missing NotificationItem script!");
        }
    }
}
