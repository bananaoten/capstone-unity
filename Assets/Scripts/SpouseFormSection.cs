using UnityEngine;
using TMPro;
using System;
using System.Globalization;

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
