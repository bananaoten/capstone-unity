using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SlideshowManager : MonoBehaviour
{
    public List<Sprite> slideSprites; // List of sprites to display
    public Image displayImage;        // The UI Image that shows the current slide

    public Button nextButton;
    public Button previousButton;

    private int currentSlideIndex = 0;

    void Start()
    {
        ShowSlide(currentSlideIndex);
        nextButton.onClick.AddListener(ShowNextSlide);
        previousButton.onClick.AddListener(ShowPreviousSlide);
    }

    void ShowSlide(int index)
    {
        if (slideSprites.Count == 0 || displayImage == null)
            return;

        displayImage.sprite = slideSprites[index];

        // Optional: Disable buttons at ends
        previousButton.interactable = index > 0;
        nextButton.interactable = index < slideSprites.Count - 1;
    }

    void ShowNextSlide()
    {
        if (currentSlideIndex < slideSprites.Count - 1)
        {
            currentSlideIndex++;
            ShowSlide(currentSlideIndex);
        }
    }

    void ShowPreviousSlide()
    {
        if (currentSlideIndex > 0)
        {
            currentSlideIndex--;
            ShowSlide(currentSlideIndex);
        }
    }
}
