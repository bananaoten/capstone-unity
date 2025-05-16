using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PasswordVisibilityToggle : MonoBehaviour
{
    public TMP_InputField passwordInputField;
    public Sprite eyeOpenIcon;     // Assign in Inspector
    public Sprite eyeClosedIcon;   // Assign in Inspector

    private bool isPasswordHidden = true;
    private Image iconImage;

    private void Awake()
    {
        // Get the Image component on the same GameObject this script is attached to
        iconImage = GetComponent<Image>();

        if (iconImage == null)
            Debug.LogError("No Image component found on this GameObject! Please attach this script to the button image.");
    }

    public void TogglePasswordVisibility()
    {
        if (passwordInputField == null)
        {
            Debug.LogError("Password input field not assigned!");
            return;
        }

        if (iconImage == null)
        {
            Debug.LogError("Icon Image not found!");
            return;
        }

        if (isPasswordHidden)
        {
            passwordInputField.contentType = TMP_InputField.ContentType.Standard;
            passwordInputField.ForceLabelUpdate();
            iconImage.sprite = eyeOpenIcon;
            isPasswordHidden = false;
        }
        else
        {
            passwordInputField.contentType = TMP_InputField.ContentType.Password;
            passwordInputField.ForceLabelUpdate();
            iconImage.sprite = eyeClosedIcon;
            isPasswordHidden = true;
        }
    }
}
