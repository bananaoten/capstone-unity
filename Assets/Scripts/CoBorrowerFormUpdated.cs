using UnityEngine;
using TMPro;
using System;
using System.Globalization;

public class CoBorrowerFormUpdated : MonoBehaviour
{
    [Header("Co-Borrower Info Section")]
    public TMP_InputField lastName;
    public TMP_InputField firstName;
    public TMP_InputField middleName;
    public TMP_InputField birthday;
    public TMP_InputField age;
    public TMP_InputField placeOfBirth;
    public TMP_Dropdown genderDropdown;
    public TMP_Dropdown coBorrowerCivilStatusDropdown;  // <-- Use this instead of civilStatus
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

    [Header("Co-Borrower Spouse Section")]
    public SpouseFormSection spouseSection;

    [Header("Co-Borrower Attorney In Fact Section")]
    public AttorneyInFactFormSection attorneyInFactSection;

    [Header("Co-Borrower Character References Section")]
    public CharacterReferenceFormSection characterReferenceSection;

    [Header("Validation UI")]
    public TMP_Text validationMessage;  // Assign in Unity Editor to show validation feedback

    // Property to always get the current civil status text
    public string CoBorrowerCivilStatus
    {
        get
        {
            if (coBorrowerCivilStatusDropdown != null && coBorrowerCivilStatusDropdown.options.Count > 0)
                return coBorrowerCivilStatusDropdown.options[coBorrowerCivilStatusDropdown.value].text;
            return "";
        }
    }

    // 🔹 Birthday formatter support
    private bool suppressBirthdayCallback = false;

    private void Start()
    {
        if (birthday != null)
        {
            birthday.characterLimit = 10; // MM/DD/YYYY
            birthday.onValueChanged.AddListener(OnBirthdayChanged);
            birthday.onEndEdit.AddListener(OnBirthdayEndEdit); // calculate age when user finishes
        }
    }

    private void OnBirthdayChanged(string input)
    {
        if (suppressBirthdayCallback) return;

        // Keep only digits
        string digits = "";
        foreach (char c in input)
        {
            if (char.IsDigit(c))
                digits += c;
        }

        string formatted = "";

        // Handle Month
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

        // Handle Day
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

        // Handle Year
        if (digits.Length > 4)
        {
            string year = digits.Substring(4);
            formatted += year;
        }

        // Insert slashes
        if (formatted.Length > 2) formatted = formatted.Insert(2, "/");
        if (formatted.Length > 5) formatted = formatted.Insert(5, "/");
        if (formatted.Length > 10) formatted = formatted.Substring(0, 10);

        suppressBirthdayCallback = true;
        birthday.text = formatted;
        suppressBirthdayCallback = false;

        // Keep caret at the end
        birthday.caretPosition = birthday.text.Length;

        // If full date typed → calculate immediately
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

    private bool HasAnyInput(params TMP_InputField[] fields)
    {
        foreach (var field in fields)
        {
            if (field != null && !string.IsNullOrWhiteSpace(field.text))
                return true;
        }
        return false;
    }

    public bool ValidateCoBorrowerInfo()
    {
        // Check if the user filled out any co-borrower fields
        bool hasAnyInput = HasAnyInput(
            lastName, firstName, middleName, birthday, age, placeOfBirth, 
            citizenship, religion, contactNumber, facebookAccount,
            presentAddress, permanentAddress, employerName, businessNature, 
            positionAndDepartment, employerAddress, officeTelephone,
            emailAddress, contactPerson, tinId, pagibigNo
        );

        // Also check dropdowns separately (value != 0 means user changed it)
        hasAnyInput |= (genderDropdown != null && genderDropdown.value != 0);
        hasAnyInput |= (coBorrowerCivilStatusDropdown != null && coBorrowerCivilStatusDropdown.value != 0);
        hasAnyInput |= (sourceOfIncomeDropdown != null && sourceOfIncomeDropdown.value != 0);

        if (!hasAnyInput)
        {
            // If everything is empty, allow to proceed
            ClearValidation();
            Debug.Log("✅ No co-borrower info provided. Skipping validation.");
            return true;
        }

        // If something is filled, validate all required fields
        if (!ValidateField("Last Name", lastName)) return false;
        if (!ValidateField("First Name", firstName)) return false;
        if (!ValidateField("Middle Name", middleName)) return false;
        if (!ValidateField("Birthday", birthday)) return false;
        if (!ValidateField("Age", age)) return false;
        if (!ValidateField("Place of Birth", placeOfBirth)) return false;
        if (!ValidateDropdown("Gender", genderDropdown)) return false;
        if (!ValidateDropdown("Civil Status", coBorrowerCivilStatusDropdown)) return false;
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

        ClearValidation();
        return true;
    }

    // Validate Co-Borrower Spouse panel by delegating
    public bool ValidateCoBorrowerSpousePanel()
    {
        if (spouseSection != null)
            return spouseSection.ValidateSpousePanel();
        return true;
    }

    // Validate Co-Borrower Attorney In Fact panel by delegating
    public bool ValidateCoBorrowerAttorneyInFactPanel()
    {
        if (attorneyInFactSection != null)
            return attorneyInFactSection.ValidateAttorneyInFactPanel();
        return true;
    }

    // Validate Co-Borrower Character References panel by delegating
    public bool ValidateCoBorrowerCharacterReferencesPanel()
    {
        if (characterReferenceSection != null)
            return characterReferenceSection.ValidateCharacterReferences();
        return true;
    }

    // Helper validation for InputFields
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

    private void SetValidationMessage(string message)
    {
        if (validationMessage != null)
        {
            validationMessage.text = message;
            validationMessage.gameObject.SetActive(true); // Make sure it's visible
        }
    }

    // Helper validation for Dropdowns
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

    // Clear the validation message in the UI
    private void ClearValidation()
    {
        if (validationMessage != null)
            validationMessage.text = "";
    }

    // Called when submitting the Co-Borrower form
    public void OnSubmitCoBorrowerForm()
    {
        bool isValid = ValidateCoBorrowerInfo();
        if (!isValid)
        {
            Debug.LogWarning("Co-borrower validation failed.");
            return;
        }

        ClearValidation();
        Debug.Log("✅ Co-borrower form validated successfully.");

        // Proceed to next page / panel here
        ProceedToNextPage();
    }

    private void ProceedToNextPage()
    {
        // Example: deactivate current panel and activate next panel, or trigger navigation event
        // gameObject.SetActive(false);
        // nextPanel.SetActive(true);

        Debug.Log("Navigating to next page...");
    }

    // Optional: You can have this for external access to validation message
    public string GetValidationMessage()
    {
        if (validationMessage != null)
            return validationMessage.text;
        return "Co-Borrower validation failed.";
    }

    // Placeholder for final submission logic
    public void FinalSubmitToFirebase()
    {
        Debug.Log("✅ Final co-borrower form submitted to Firebase.");
        // Implement Firebase submission logic here
    }
}
