using UnityEngine;
using UnityEngine.UI;

public class TermsAndConditionsManager : MonoBehaviour
{
    [Header("Reference to the Signup Toggle")]
    public Toggle termsToggle;

    [Header("Signup and Terms Panels")]
    public GameObject signupPanel;
    public GameObject termsPanel;

    public void ShowTermsFromSignup()
    {
        signupPanel.SetActive(false);
        termsPanel.SetActive(true);
    }

    public void OnAgreePressed()
    {
        if (termsToggle != null)
        {
            termsToggle.isOn = true;
        }

        termsPanel.SetActive(false);
        signupPanel.SetActive(true);
    }

    public void OnDeclinePressed()
    {
        if (termsToggle != null)
        {
            termsToggle.isOn = false;
        }

        termsPanel.SetActive(false);
        signupPanel.SetActive(true);
    }
}
