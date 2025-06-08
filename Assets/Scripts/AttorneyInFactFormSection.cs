using UnityEngine;
using TMPro;

public class AttorneyInFactFormSection : MonoBehaviour
{
    [Header("Attorney In Fact Info")]
    public TMP_InputField lastName;
    public TMP_InputField firstName;
    public TMP_InputField middleName;
    public TMP_Dropdown genderDropdown;
    public TMP_InputField contactNumberOrEmail;
    public TMP_InputField address;

    [Header("Validation UI")]
    public TMP_Text validationMessageText; // Assign this in the Inspector

    public bool ValidateAttorneyInFactPanel()
    {
        bool anyInput = HasAnyInput();

        if (!anyInput)
        {
            // All empty, skip validation
            ClearValidation();
            Debug.Log("[Info] Attorney-in-Fact section skipped.");
            return true;
        }

        // Since there's input, all fields must be completed
        if (!ValidateField("Attorney-in-Fact Last Name", lastName)) return false;
        if (!ValidateField("Attorney-in-Fact First Name", firstName)) return false;
        if (!ValidateField("Attorney-in-Fact Middle Name", middleName)) return false;
        if (!ValidateDropdown("Attorney-in-Fact Gender", genderDropdown)) return false;
        if (!ValidateField("Attorney-in-Fact Contact Number or Email", contactNumberOrEmail)) return false;
        if (!ValidateField("Attorney-in-Fact Address", address)) return false;

        // All validations passed
        ClearValidation();
        return true;
    }

    private bool HasAnyInput()
    {
        return
            !string.IsNullOrWhiteSpace(lastName?.text) ||
            !string.IsNullOrWhiteSpace(firstName?.text) ||
            !string.IsNullOrWhiteSpace(middleName?.text) ||
            (genderDropdown != null && genderDropdown.value != 0) ||
            !string.IsNullOrWhiteSpace(contactNumberOrEmail?.text) ||
            !string.IsNullOrWhiteSpace(address?.text);
    }

    private bool ValidateField(string fieldName, TMP_InputField input)
    {
        if (input == null || string.IsNullOrWhiteSpace(input.text))
        {
            SetValidationMessage($"{fieldName} is required.");
            Debug.LogWarning($"{fieldName} is required.");
            return false;
        }
        return true;
    }

    private bool ValidateDropdown(string fieldName, TMP_Dropdown dropdown)
    {
        if (dropdown == null || dropdown.value == 0)
        {
            SetValidationMessage($"{fieldName} is required.");
            Debug.LogWarning($"{fieldName} is required.");
            return false;
        }
        return true;
    }

    private void SetValidationMessage(string message)
    {
        if (validationMessageText != null)
        {
            validationMessageText.text = message;
            validationMessageText.color = Color.red; // Show message in red
            validationMessageText.gameObject.SetActive(true); // Make sure visible
        }
    }

    private void ClearValidation()
    {
        if (validationMessageText != null)
        {
            validationMessageText.text = "";
            validationMessageText.gameObject.SetActive(false); // Hide message when cleared
        }
    }
}
