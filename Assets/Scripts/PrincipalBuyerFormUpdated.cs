using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Globalization; // for TryParseExact

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
    public TMP_InputField religion;             // Optional
    public TMP_InputField contactNumber;
    public TMP_InputField facebookAccount;      // Optional
    public TMP_InputField presentAddress;
    public TMP_InputField permanentAddress;
    public TMP_InputField employerName;
    public TMP_InputField businessNature;
    public TMP_InputField positionAndDepartment;
    public TMP_InputField employerAddress;
    public TMP_InputField officeTelephone;      // Optional
    public TMP_InputField emailAddress;
    public TMP_InputField contactPerson;        // Optional
    public TMP_InputField tinId;
    public TMP_InputField pagibigNo;            // Optional unless using Pag-IBIG financing
    public TMP_Dropdown sourceOfIncomeDropdown; // Optional

    public string PrincipalBuyerCivilStatus => civilStatus.options[civilStatus.value].text;

    // 🔹 Birthday formatter support
    private bool suppressBirthdayCallback = false;

    private void Start()
    {
        if (birthday != null)
        {
            birthday.characterLimit = 10; // MM/DD/YYYY
            birthday.onValueChanged.AddListener(OnBirthdayChanged);
            birthday.onEndEdit.AddListener(OnBirthdayEndEdit);
        }
    }

    private void OnBirthdayChanged(string input)
    {
        if (suppressBirthdayCallback) return;

        string digits = "";
        foreach (char c in input)
        {
            if (char.IsDigit(c))
                digits += c;
        }

        string formatted = "";

        if (digits.Length >= 2)
        {
            int month = int.Parse(digits.Substring(0, 2));
            if (month > 12) month = 12;
            formatted = month.ToString("00");
        }
        else if (digits.Length > 0)
        {
            formatted = digits;
        }

        if (digits.Length >= 4)
        {
            int day = int.Parse(digits.Substring(2, 2));
            if (day > 31) day = 31;
            formatted += day.ToString("00");
        }
        else if (digits.Length > 2)
        {
            formatted += digits.Substring(2);
        }

        if (digits.Length > 4)
        {
            string year = digits.Substring(4);
            formatted += year;
        }

        if (formatted.Length > 2) formatted = formatted.Insert(2, "/");
        if (formatted.Length > 5) formatted = formatted.Insert(5, "/");
        if (formatted.Length > 10) formatted = formatted.Substring(0, 10);

        suppressBirthdayCallback = true;
        birthday.text = formatted;
        suppressBirthdayCallback = false;

        birthday.caretPosition = birthday.text.Length;

        if (birthday.text.Length == 10)
        {
            CalculateAgeFromBirthday();
        }
    }

    private void OnBirthdayEndEdit(string input)
    {
        CalculateAgeFromBirthday();
    }

    private void CalculateAgeFromBirthday()
    {
        if (birthday == null || age == null) return;

        string txt = birthday.text?.Trim();
        if (string.IsNullOrEmpty(txt) || txt.Length != 10)
        {
            age.text = "";
            return;
        }

        DateTime birthDate;
        bool parsed = DateTime.TryParseExact(txt, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out birthDate);

        if (!parsed)
        {
            age.text = "";
            return;
        }

        int calculatedAge = DateTime.Now.Year - birthDate.Year;
        if (DateTime.Now.Date < birthDate.AddYears(calculatedAge).Date)
            calculatedAge--;

        age.text = calculatedAge.ToString();
    }

    public bool ValidatePrincipalBuyerPanel()
    {
        // ✅ Required fields
        if (!ValidateField("Last Name", lastName)) return false;
        if (!ValidateField("First Name", firstName)) return false;
        if (!ValidateField("Middle Name", middleName)) return false;
        if (!ValidateField("Birthday", birthday)) return false;
        if (!ValidateField("Age", age)) return false;
        if (!ValidateField("Place of Birth", placeOfBirth)) return false;
        if (!ValidateDropdown("Gender", genderDropdown)) return false;
        if (!ValidateDropdown("Civil Status", civilStatus)) return false;
        if (!ValidateField("Citizenship", citizenship)) return false;
        if (!ValidateField("Contact Number", contactNumber)) return false;
        if (!ValidateField("Present Address", presentAddress)) return false;
        if (!ValidateField("Permanent Address", permanentAddress)) return false;
        if (!ValidateField("Employer Name", employerName)) return false;
        if (!ValidateField("Nature of Business", businessNature)) return false;
        if (!ValidateField("Position and Department", positionAndDepartment)) return false;
        if (!ValidateField("Employer Address", employerAddress)) return false;
        if (!ValidateField("Email Address", emailAddress)) return false;
        if (!ValidateField("TIN ID", tinId)) return false;

        // 🟡 Optional fields (skip validation if empty)
        if (!string.IsNullOrWhiteSpace(religion?.text))
            Debug.Log("[Info] Religion provided: " + religion.text);

        if (!string.IsNullOrWhiteSpace(facebookAccount?.text))
            Debug.Log("[Info] Facebook account provided.");

        if (!string.IsNullOrWhiteSpace(contactPerson?.text))
            Debug.Log("[Info] Contact person provided.");

        if (!string.IsNullOrWhiteSpace(officeTelephone?.text))
            Debug.Log("[Info] Office telephone provided.");

        if (pagibigNo != null && !string.IsNullOrWhiteSpace(pagibigNo.text))
            Debug.Log("[Info] PAGIBIG No. provided.");

        if (sourceOfIncomeDropdown != null && sourceOfIncomeDropdown.value > 0)
            Debug.Log("[Info] Source of Income selected: " + sourceOfIncomeDropdown.options[sourceOfIncomeDropdown.value].text);

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
