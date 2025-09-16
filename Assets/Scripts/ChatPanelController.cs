using UnityEngine;

public class ChatPanelController : MonoBehaviour
{
    [Header("UI")]
    public GameObject chatPanel; 
    public MessageBadgeManager badgeManager;

    // Call this from your MessageButton OnClick
    public void OpenChat()
    {
        if (chatPanel != null) chatPanel.SetActive(true);
        if (badgeManager != null) badgeManager.MarkAllAsReadInDatabase();
    }

    public void CloseChat()
    {
        if (chatPanel != null) chatPanel.SetActive(false);
    }
}
