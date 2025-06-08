using UnityEngine;
using TMPro;

public class CharacterReferenceFormSection : MonoBehaviour
{
    [Header("Character Reference 1")]
    public TMP_InputField lastName1;
    public TMP_InputField firstName1;
    public TMP_InputField middleName1;
    public TMP_InputField contactNumber1;
    public TMP_InputField facebookAccount1;
    public TMP_InputField address1;

    [Header("Character Reference 2")]
    public TMP_InputField lastName2;
    public TMP_InputField firstName2;
    public TMP_InputField middleName2;
    public TMP_InputField contactNumber2;
    public TMP_InputField facebookAccount2;
    public TMP_InputField address2;

    [Header("Character Reference 3")]
    public TMP_InputField lastName3;
    public TMP_InputField firstName3;
    public TMP_InputField middleName3;
    public TMP_InputField contactNumber3;
    public TMP_InputField facebookAccount3;
    public TMP_InputField address3;

    [Header("Validation UI")]
    public TMP_Text validationMessageText; // Assign in Inspector

    public bool ValidateCharacterReferences()
    {
        // Validate each character reference block
        if (!ValidateSingleReference(1, lastName1, firstName1, middleName1, contactNumber1, facebookAccount1, address1)) return false;
        if (!ValidateSingleReference(2, lastName2, firstName2, middleName2, contactNumber2, facebookAccount2, address2)) return false;
        if (!ValidateSingleReference(3, lastName3, firstName3, middleName3, contactNumber3, facebookAccount3, address3)) return false;

        ClearValidation();
        return true;
    }

    private bool ValidateSingleReference(int referenceNumber,
        TMP_InputField lastName, TMP_InputField firstName, TMP_InputField middleName,
        TMP_InputField contactNumber, TMP_InputField facebookAccount, TMP_InputField address)
    {
        bool anyInput = HasAnyInput(lastName, firstName, middleName, contactNumber, facebookAccount, address);

        if (!anyInput)
        {
            // No input in this reference - skip validation for this one
            return true;
        }

        // If any input, all must be completed:
        if (!ValidateField($"Character Reference {referenceNumber} Last Name", lastName)) return false;
        if (!ValidateField($"Character Reference {referenceNumber} First Name", firstName)) return false;
        if (!ValidateField($"Character Reference {referenceNumber} Middle Name", middleName)) return false;
        if (!ValidateField($"Character Reference {referenceNumber} Contact Number/Telephone", contactNumber)) return false;
        if (!ValidateField($"Character Reference {referenceNumber} Facebook Account", facebookAccount)) return false;
        if (!ValidateField($"Character Reference {referenceNumber} Address", address)) return false;

        return true;
    }

    private bool HasAnyInput(params TMP_InputField[] inputs)
    {
        foreach (var input in inputs)
        {
            if (input != null && !string.IsNullOrWhiteSpace(input.text))
            {
                return true;
            }
        }
        return false;
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

    private void SetValidationMessage(string message)
    {
        if (validationMessageText != null)
        {
            validationMessageText.text = message;
            validationMessageText.color = Color.red;
            validationMessageText.gameObject.SetActive(true);
        }
    }

    private void ClearValidation()
    {
        if (validationMessageText != null)
        {
            validationMessageText.text = "";
            validationMessageText.gameObject.SetActive(false);
        }
    }
}
