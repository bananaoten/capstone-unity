using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class NotificationItem : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public TMP_Text timestampText; // optional, assign in prefab

    [Header("Icons")]
    public Sprite approvedIcon;
    public Sprite declinedIcon;
    public Sprite pendingIcon; // optional, in case you add more states later

    public enum NotificationType
    {
        Approved,
        Declined,
        Pending
    }

    /// <summary>
    /// Sets the notification data (title, description, type, timestamp).
    /// </summary>
    public void SetData(string title, string description, NotificationType type, DateTime? timestamp = null)
    {
        if (titleText != null) titleText.text = title;
        if (descriptionText != null) descriptionText.text = description;

        // Handle icon based on type
        if (iconImage != null)
        {
            switch (type)
            {
                case NotificationType.Approved:
                    iconImage.sprite = approvedIcon;
                    break;
                case NotificationType.Declined:
                    iconImage.sprite = declinedIcon;
                    break;
                case NotificationType.Pending:
                    if (pendingIcon != null)
                        iconImage.sprite = pendingIcon;
                    break;
            }
        }

        // Optional timestamp
        if (timestampText != null)
        {
            DateTime showTime = timestamp ?? DateTime.Now;
            timestampText.text = showTime.ToString("MMM dd, h:mm tt");
        }
    }
}
