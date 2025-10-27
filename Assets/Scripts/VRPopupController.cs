using UnityEngine;
using System.Collections;

public class VRPopupController : MonoBehaviour
{
    [Header("Popup Settings")]
    public CanvasGroup canvasGroup;
    public float fadeDuration = 0.5f;
    public float displayDuration = 5f;
    public bool showOnStart = true; // set to false if you want to trigger it manually

    private bool hasShown = false;

    void Start()
    {
        if (showOnStart && !hasShown)
        {
            ShowPopup();
        }
    }

    public void ShowPopup()
    {
        if (!hasShown)
        {
            hasShown = true;
            StartCoroutine(ShowPopupRoutine());
        }
    }

    private IEnumerator ShowPopupRoutine()
    {
        canvasGroup.gameObject.SetActive(true);

        // Fade in
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1;

        yield return new WaitForSeconds(displayDuration);

        // Fade out
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0;
        canvasGroup.gameObject.SetActive(false);
    }
}
