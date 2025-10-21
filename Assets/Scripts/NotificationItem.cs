using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Globalization;

public class NotificationItem : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public TMP_Text timestampText;
    public Button viewButton;

    [Header("Prefabs")]
    public GameObject appointmentDetailsPrefab; // ViewAppointment prefab

    [Header("Icons")]
    public Sprite approvedIcon;
    public Sprite declinedIcon;
    public Sprite pendingIcon;

    // Appointment Data
    private string appointmentId;
    private string fullName;
    private string contact;
    private string email;
    private string date;
    private string property;

    public enum NotificationType
    {
        Approved,
        Declined,
        Pending
    }

    

    public void SetData(
        
        string title,
        string description,
        NotificationType type,
        DateTime? timestamp = null,
        string appointmentId = "",
        string fullName = "",
        string contact = "",
        string email = "",
        string date = "",
        string property = "")
    {
        this.appointmentId = appointmentId;
        this.fullName = fullName;
        this.contact = contact;
        this.email = email;
        this.date = date;
        this.property = property;

        if (titleText != null) titleText.text = title;
        if (descriptionText != null) descriptionText.text = description;

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
                    iconImage.sprite = pendingIcon;
                    break;
            }
        }

        if (timestampText != null)
        {
            DateTime displayTime = timestamp ?? DateTime.UtcNow;
            try
            {
                TimeZoneInfo phTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                displayTime = TimeZoneInfo.ConvertTimeFromUtc(displayTime.ToUniversalTime(), phTimeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                displayTime = displayTime.ToLocalTime();
            }

            timestampText.text = displayTime.ToString("MMM dd, hh:mm tt", CultureInfo.InvariantCulture);
        }

        if (viewButton != null)
        {
            viewButton.onClick.RemoveAllListeners();
            viewButton.onClick.AddListener(ViewAppointment);
        }
    }

    public void ViewAppointment()
    
    {
        Debug.Log("ViewAppointment called");
        if (appointmentDetailsPrefab == null)
        {
            Debug.LogError("❌ AppointmentDetails Prefab is not assigned!");
            return;
        }

        // Use UIReferences.Instance.modalContainer directly
        Transform modalContainer = UIReferences.Instance != null ? UIReferences.Instance.modalContainer : null;
        if (modalContainer == null)
        {
            Debug.LogError("❌ ModalContainer not found! Assign it in UIReferences.");
            return;
        }

        GameObject popup = Instantiate(appointmentDetailsPrefab, modalContainer);
        popup.transform.SetAsLastSibling();
        popup.SetActive(true);

        RectTransform rt = popup.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

         AppointmentRescheduleUI ui = popup.GetComponent<AppointmentRescheduleUI>();
    if (ui != null)
    {
        Debug.Log("[NotificationItem] Passing appointmentId: " + appointmentId); // <--- Add this line
        ui.LoadAppointment(appointmentId);
    }
    else
    {
        Debug.LogError("❌ AppointmentRescheduleUI script is missing on prefab!");
    }
    }
}