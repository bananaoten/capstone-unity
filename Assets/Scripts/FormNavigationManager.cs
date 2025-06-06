using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FormNavigationManager : MonoBehaviour
{
    [Header("Form Panels")]
    [SerializeField] private GameObject[] formPanels;

    [Header("Navigation UI")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;

    [Header("Progress UI")]
    [SerializeField] private TMP_Text stepText;
    [SerializeField] private Image progressBarFill;

    [SerializeField] private Image[] stepIcons; // drag your 9 icons here
[SerializeField] private Color activeColor = Color.green;
[SerializeField] private Color inactiveColor = Color.gray;


    private int currentStep = 0;

    void Start()
    {
        ShowCurrentPanel();
    }

    public void NextPanel()
    {
        if (currentStep < formPanels.Length - 1)
        {
            formPanels[currentStep].SetActive(false);
            currentStep++;
            ShowCurrentPanel();
        }
    }

    public void PreviousPanel()
    {
        if (currentStep > 0)
        {
            formPanels[currentStep].SetActive(false);
            currentStep--;
            ShowCurrentPanel();
        }
    }

   private void ShowCurrentPanel()
{
    for (int i = 0; i < formPanels.Length; i++)
    {
        formPanels[i].SetActive(i == currentStep);
    }

    stepText.text = $"Step {currentStep + 1} of {formPanels.Length}";
    progressBarFill.fillAmount = (float)(currentStep + 1) / formPanels.Length;

    previousButton.interactable = currentStep > 0;
    nextButton.interactable = currentStep < formPanels.Length - 1;

    // Update step icon colors
    for (int i = 0; i < stepIcons.Length; i++)
    {
        stepIcons[i].color = (i == currentStep) ? activeColor : inactiveColor;
    }
}

}
