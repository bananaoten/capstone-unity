using UnityEngine;
using TMPro;

public class SpouseFormSection : MonoBehaviour
{
    [Header("Spouse Info")]
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

    [Header("Validation UI")]
    public TMP_Text validationMessage;

    public bool ValidateSpousePanel()
    {
        if (!ValidateField("Spouse Last Name", lastName)) return false;
        if (!ValidateField("Spouse First Name", firstName)) return false;
        if (!ValidateField("Spouse Middle Name", middleName)) return false;
        if (!ValidateField("Spouse Birthday", birthday)) return false;
        if (!ValidateField("Spouse Age", age)) return false;
        if (!ValidateField("Spouse Place of Birth", placeOfBirth)) return false;
        if (!ValidateDropdown("Spouse Gender", genderDropdown)) return false;
        if (!ValidateDropdown("Spouse Civil Status", civilStatus)) return false;
        if (!ValidateField("Spouse Citizenship", citizenship)) return false;
        if (!ValidateField("Spouse Religion", religion)) return false;
        if (!ValidateField("Spouse Contact Number", contactNumber)) return false;
        if (!ValidateField("Spouse Facebook Account", facebookAccount)) return false;
        if (!ValidateField("Spouse Employer Name", employerName)) return false;
        if (!ValidateField("Spouse Nature of Business", businessNature)) return false;
        if (!ValidateField("Spouse Position and Department", positionAndDepartment)) return false;
        if (!ValidateField("Spouse Employer Address", employerAddress)) return false;
        if (!ValidateField("Spouse Office Telephone", officeTelephone)) return false;
        if (!ValidateField("Spouse Email Address", emailAddress)) return false;
        if (!ValidateField("Spouse Contact Person", contactPerson)) return false;
        if (!ValidateField("Spouse TIN ID", tinId)) return false;

        // Optional fields info logs
        if (pagibigNo != null && string.IsNullOrWhiteSpace(pagibigNo.text))
        {
            Debug.Log("[Info] Spouse PAGIBIG No. is optional. You may leave it blank or type N/A.");
        }

        if (sourceOfIncomeDropdown != null && sourceOfIncomeDropdown.value == 0)
        {
            Debug.Log("[Info] Spouse Source of Income not selected. It's okay to leave as default if not applicable.");
        }

        // Clear validation message if everything is valid
        if (validationMessage != null)
        {
            validationMessage.text = "";
        }

        return true;
    }

    private bool ValidateField(string fieldName, TMP_InputField input)
    {
        if (input == null || string.IsNullOrWhiteSpace(input.text))
        {
            Debug.LogWarning($"{fieldName} is required.");
            if (validationMessage != null)
            {
                validationMessage.text = $"{fieldName} is required.";
            }
            return false;
        }
        return true;
    }

    private bool ValidateDropdown(string fieldName, TMP_Dropdown dropdown)
    {
        if (dropdown == null || dropdown.value == 0)
        {
            Debug.LogWarning($"{fieldName} is required.");
            if (validationMessage != null)
            {
                validationMessage.text = $"{fieldName} is required.";
            }
            return false;
        }
        return true;
    }
}
