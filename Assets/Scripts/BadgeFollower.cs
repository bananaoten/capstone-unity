using UnityEngine;
using TMPro;

public class BadgeFollower : MonoBehaviour
{
    public GameObject badgeObject;
    public TextMeshProUGUI badgeText;

    void OnEnable()
    {
        if (MessageBadgeManager.Instance != null)
            MessageBadgeManager.Instance.OnBadgeUpdated += UpdateBadge;
    }

    void OnDisable()
    {
        if (MessageBadgeManager.Instance != null)
            MessageBadgeManager.Instance.OnBadgeUpdated -= UpdateBadge;
    }

    private void UpdateBadge(int count)
    {
        if (badgeObject == null || badgeText == null) return;

        if (count > 0)
        {
            badgeObject.SetActive(true);
            badgeText.text = count.ToString();
        }
        else
        {
            badgeObject.SetActive(false);
        }
    }
}
