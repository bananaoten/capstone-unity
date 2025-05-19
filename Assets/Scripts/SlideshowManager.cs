using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SlideshowManager : MonoBehaviour
{
    public Sprite[] slideshowImages;       // Sprites to display
    public Image displayImage;             // The main UI Image component
    public float slideDuration = 0.4f;     // Duration of the slide animation

    private int currentIndex = 0;
    private bool isSliding = false;

    void Start()
    {
        displayImage.sprite = slideshowImages[currentIndex];
    }

    public void ShowNext()
    {
        if (isSliding) return;

        int nextIndex = (currentIndex + 1) % slideshowImages.Length;
        StartCoroutine(SlideToImage(nextIndex, -1));
    }

    public void ShowPrevious()
    {
        if (isSliding) return;

        int prevIndex = (currentIndex - 1 + slideshowImages.Length) % slideshowImages.Length;
        StartCoroutine(SlideToImage(prevIndex, 1));
    }

    IEnumerator SlideToImage(int newIndex, int direction)
    {
        isSliding = true;

        float width = ((RectTransform)displayImage.transform).rect.width;
        Vector2 startPos = displayImage.rectTransform.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(-direction * width, 0);

        // Create temporary image object
        GameObject tempGO = new GameObject("TempImage", typeof(Image));
        tempGO.transform.SetParent(displayImage.transform.parent, false);
        Image tempImage = tempGO.GetComponent<Image>();
        RectTransform tempRect = tempImage.rectTransform;

        tempImage.sprite = slideshowImages[newIndex];
        tempImage.preserveAspect = true;

        tempRect.anchorMin = tempRect.anchorMax = new Vector2(0.5f, 0.5f);
        tempRect.pivot = new Vector2(0.5f, 0.5f);
        tempRect.sizeDelta = displayImage.rectTransform.sizeDelta;
        tempRect.anchoredPosition = startPos + new Vector2(direction * width, 0);

        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);

            displayImage.rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            tempRect.anchoredPosition = Vector2.Lerp(tempRect.anchoredPosition, startPos, t);
            yield return null;
        }

        // Finalize
        displayImage.sprite = slideshowImages[newIndex];
        displayImage.rectTransform.anchoredPosition = startPos;
        Destroy(tempGO);
        currentIndex = newIndex;
        isSliding = false;
    }
}
