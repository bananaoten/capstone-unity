using UnityEngine;
using TMPro;

public class MessagePageItem : MonoBehaviour
{
    public GameObject badgeObject;
    public TextMeshProUGUI badgeText;

    public void SetBadge(int unreadCount)
    {
        if (unreadCount > 0)
        {
            badgeObject.SetActive(true);
            badgeText.text = unreadCount.ToString();
        }
        else
        {
            badgeObject.SetActive(false);
        }
    }
}
