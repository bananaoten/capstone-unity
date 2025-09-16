using UnityEngine;

public class MessageButtonHandler : MonoBehaviour
{
    public GameObject chatPanel;             // The messaging panel UI
    public MessageListener messageListener;  // Your listener script
    public MessageBadgeManager badgeManager; // Badge script

   // inside your handler
public void OnMessageButtonClicked()
{
    // Open chat panel
    if (chatPanel != null) chatPanel.SetActive(true);

    // Mark all admin messages as read in DB (singleton)
    if (MessageBadgeManager.Instance != null)
        MessageBadgeManager.Instance.MarkAllAsReadInDatabase();
}

}
