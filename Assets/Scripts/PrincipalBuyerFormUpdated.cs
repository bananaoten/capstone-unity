using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PrincipalBuyerFormUpdated : MonoBehaviour
{
    [Header("Spouse Section")]
    public SpouseFormSection spouseSection;

    [Header("Attorney In Fact Section")]
    public AttorneyInFactFormSection attorneyInFactSection;

    [Header("Character References Section")]
    public CharacterReferenceFormSection characterReferenceSection;

    [Header("Co-Borrower Section")]
    public CoBorrowerFormUpdated coBorrowerSection; // 👈 ADD THIS

    [Header("Validation UI")]
    public TMP_Text validationMessage;

    [Header("Principal Buyer Info")]
    public TMP_InputField lastName;
    public TMP_InputField firstName;
    public TMP_InputField middleName;
    public TMP_InputField birthday;
    public TMP_InputField age;
    public TMP_InputField placeOfBirth;
    public TMP_Dropdown genderDropdown;
    public TMP_Dropdown civilStatus;
    public TMP_InputField citizenship;
    public TMP_InputField religion;
    public TMP_InputField contactNumber;
    public TMP_InputField facebookAccount;
    public TMP_InputField presentAddress;
    public TMP_InputField permanentAddress;
    public TMP_InputField employerName;
    public TMP_InputField businessNature;
    public TMP_InputField positionAndDepartment;
    public TMP_InputField employerAddress;
    public TMP_InputField officeTelephone;
    public TMP_InputField emailAddress;
    public TMP_InputField contactPerson;
    public TMP_InputField tinId;
    public TMP_InputField pagibigNo;
    public TMP_Dropdown sourceOfIncomeDropdown;

    public string PrincipalBuyerCivilStatus => civilStatus.options[civilStatus.value].text;

    public bool ValidatePrincipalBuyerPanel()
    {
        if (!ValidateField("Last Name", lastName)) return false;
        if (!ValidateField("First Name", firstName)) return false;
        if (!ValidateField("Middle Name", middleName)) return false;
        if (!ValidateField("Birthday", birthday)) return false;
        if (!ValidateField("Age", age)) return false;
        if (!ValidateField("Place of Birth", placeOfBirth)) return false;
        if (!ValidateDropdown("Gender", genderDropdown)) return false;
        if (!ValidateDropdown("Civil Status", civilStatus)) return false;
        if (!ValidateField("Citizenship", citizenship)) return false;
        if (!ValidateField("Religion", religion)) return false;
        if (!ValidateField("Contact Number", contactNumber)) return false;
        if (!ValidateField("Facebook Account", facebookAccount)) return false;
        if (!ValidateField("Present Address", presentAddress)) return false;
        if (!ValidateField("Permanent Address", permanentAddress)) return false;
        if (!ValidateField("Employer Name", employerName)) return false;
        if (!ValidateField("Nature of Business", businessNature)) return false;
        if (!ValidateField("Position and Department", positionAndDepartment)) return false;
        if (!ValidateField("Employer Address", employerAddress)) return false;
        if (!ValidateField("Office Telephone", officeTelephone)) return false;
        if (!ValidateField("Email Address", emailAddress)) return false;
        if (!ValidateField("Contact Person", contactPerson)) return false;
        if (!ValidateField("TIN ID", tinId)) return false;

        if (pagibigNo != null && string.IsNullOrWhiteSpace(pagibigNo.text))
        {
            Debug.Log("[Info] PAGIBIG No. is optional.");
        }

        if (sourceOfIncomeDropdown != null && sourceOfIncomeDropdown.value == 0)
        {
            Debug.Log("[Info] Source of Income is optional.");
        }

        ClearValidation();
        return true;
    }

    private bool ValidateField(string fieldName, TMP_InputField input)
    {
        if (input == null || string.IsNullOrWhiteSpace(input.text))
        {
            Debug.LogWarning($"{fieldName} is required.");
            SetValidationMessage($"{fieldName} is required.");
            return false;
        }
        return true;
    }

    private bool ValidateDropdown(string fieldName, TMP_Dropdown dropdown)
    {
        if (dropdown == null || dropdown.value == 0)
        {
            Debug.LogWarning($"{fieldName} is required.");
            SetValidationMessage($"{fieldName} is required.");
            return false;
        }
        return true;
    }

   private void SetValidationMessage(string message)
{
    Debug.Log("Setting validation message: " + message); // Add this line
    if (validationMessage != null)
    {
        validationMessage.text = message;
    }
}

    private void ClearValidation()
    {
        if (validationMessage != null)
        {
            validationMessage.text = "";
        }
    }

    // Add this method inside your PrincipalBuyerFormUpdated class
public string GetValidationMessage()
{
    return validationMessage != null ? validationMessage.text : "Please fill all required fields.";
}

    // Delegated panel validators
    public bool ValidateSpousePanel()
    {
        return spouseSection == null || spouseSection.ValidateSpousePanel();
    }

    public bool ValidateAttorneyInFactPanel()
    {
        return attorneyInFactSection == null || attorneyInFactSection.ValidateAttorneyInFactPanel();
    }

    public bool ValidateCharacterReferencesPanel()
    {
        return characterReferenceSection == null || characterReferenceSection.ValidateCharacterReferences();
    }

    // ✅ Co-Borrower delegated validation
    public bool ValidateCoBorrowerPrincipalInfoPanel()
    {
        return coBorrowerSection == null || coBorrowerSection.ValidateCoBorrowerInfo();
    }

    public bool ValidateCoBorrowerSpousePanel()
    {
        return coBorrowerSection == null || coBorrowerSection.ValidateCoBorrowerSpousePanel();
    }

    public bool ValidateCoBorrowerAttorneyInFactPanel()
    {
        return coBorrowerSection == null || coBorrowerSection.ValidateCoBorrowerAttorneyInFactPanel();
    }

    public bool ValidateCoBorrowerCharacterReferencesPanel()
    {
        return coBorrowerSection == null || coBorrowerSection.ValidateCoBorrowerCharacterReferencesPanel();
    }

    public void FinalSubmitToFirebase()
    {
        Debug.Log("✅ Final form submitted to Firebase.");
        // TODO: Add Firebase logic
    }
}
