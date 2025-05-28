using System;
using UnityEngine;
using TMPro;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class PrincipalBuyerInfo
{
    public TMP_InputField lastName;
    public TMP_InputField firstName;
    public TMP_InputField middleName;
    public TMP_Dropdown gender;
    public TMP_InputField birthday;
    public TMP_InputField age;
    public TMP_InputField placeOfBirth;
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
    public TMP_InputField pagibigMidNo;
    public TMP_Dropdown sourceOfIncome;
}

[System.Serializable]
public class MarketingDepartmentInfo
{
    public TMP_InputField subdivisionBlockLot;
    public TMP_InputField houseDescription;
    public TMP_InputField lotArea;
    public TMP_InputField houseArea;
    public TMP_InputField totalContractPrice;
    public TMP_InputField downPayment;
    public TMP_InputField dpTerm;
    public TMP_InputField loanAmount;
    public TMP_InputField maTerm;
    public TMP_InputField monthlyAmortization;
}

[System.Serializable]
public class SpouseInfo
{
    public TMP_InputField lastName;
    public TMP_InputField firstName;
    public TMP_InputField middleName;
    public TMP_Dropdown gender;
    public TMP_InputField birthday;
    public TMP_InputField age;
    public TMP_InputField placeOfBirth;
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
    public TMP_InputField pagibigMidNo;
    public TMP_Dropdown sourceOfIncome;
}

[System.Serializable]
public class AttorneyInFactInfo
{
    public TMP_InputField lastName;
    public TMP_InputField firstName;
    public TMP_InputField middleName;
    public TMP_Dropdown gender;
    public TMP_InputField contactInfo;
    public TMP_InputField address;
}

[System.Serializable]
public class CharacterReference
{
    public TMP_InputField lastName;
    public TMP_InputField firstName;
    public TMP_InputField middleName;
    public TMP_InputField contactNumber;
    public TMP_InputField facebookAccount;
    public TMP_InputField address;
}

[System.Serializable]
public class CoBorrowerInfo
{
    public PrincipalBuyerInfo coBorrower;
    public SpouseInfo coBorrowerSpouse;
    public AttorneyInFactInfo coBorrowerAttorney;
    public CharacterReference[] coBorrowerCharacterReferences;
}


public class PrincipalBuyerForm : MonoBehaviour
{
    public PrincipalBuyerInfo principalBuyerInfo;
    public SpouseInfo spouseInfo;
    public AttorneyInFactInfo attorneyInfo;
    public CharacterReference[] characterReferences;
    public MarketingDepartmentInfo marketingInfo;
    public CoBorrowerInfo coBorrowerInfo;

    public UnityEngine.UI.Button submitButton;
    public TMP_Text feedbackText;

    private DatabaseReference dbReference;

void Start()
{
    Debug.Log("PrincipalBuyerForm Start() called.");

    if (submitButton == null)
    {
        Debug.LogError("submitButton is NULL!");
        return;
    }

    if (FirebaseInitializer.IsFirebaseReady)
    {
        Debug.Log("Firebase already ready. Initializing form.");
        InitForm();
    }
    else
    {
        Debug.Log("Waiting for Firebase to be ready...");
        FirebaseInitializer.OnFirebaseReady += InitForm;
    }
}

private void InitForm()
{
    Debug.Log("InitForm called.");
    dbReference = FirebaseInitializer.Database.RootReference;

    // Avoid adding multiple listeners
    submitButton.onClick.RemoveAllListeners();
    submitButton.onClick.AddListener(SubmitToFirebase);

    Debug.Log("submitButton listener added and dbReference assigned.");
}



void SubmitToFirebase()
{
    Debug.Log("SubmitToFirebase called");

    if (!ValidateForm())
    {
        Debug.Log("Validation failed");
        return;
    }

    Debug.Log("Validation passed, preparing to submit.");

    Dictionary<string, object> buyerData = SerializeBuyerInfo(principalBuyerInfo);

    // Principal Buyer's Spouse Info
    if (principalBuyerInfo.civilStatus.options[principalBuyerInfo.civilStatus.value].text == "Married")
    {
        buyerData["spouseInfo"] = SerializeSpouseInfo(spouseInfo);
    }

    // Principal Buyer’s Attorney-in-Fact
    if (IsAttorneyInfoFilled(attorneyInfo))
    {
        buyerData["attorneyInfo"] = SerializeAttorney(attorneyInfo);
    }

    // Principal Buyer’s Character References
    List<Dictionary<string, object>> characterRefList = SerializeCharacterReferences(characterReferences);
    if (characterRefList.Count > 0)
    {
        buyerData["characterReferences"] = characterRefList;
    }

    // Marketing Info
    buyerData["marketingDepartment"] = SerializeMarketing(marketingInfo);

    // Co-Borrower Info
    bool isCoBorrowerFilled = IsPrincipalBuyerInfoFilled(coBorrowerInfo.coBorrower);
    if (isCoBorrowerFilled)
    {
        Dictionary<string, object> coBorrowerData = SerializeBuyerInfo(coBorrowerInfo.coBorrower);

        // Co-Borrower's Spouse Info
        if (coBorrowerInfo.coBorrower.civilStatus.options[coBorrowerInfo.coBorrower.civilStatus.value].text == "Married")
        {
            coBorrowerData["spouseInfo"] = SerializeSpouseInfo(coBorrowerInfo.coBorrowerSpouse);
        }

        // Co-Borrower Attorney-in-Fact
        if (IsAttorneyInfoFilled(coBorrowerInfo.coBorrowerAttorney))
        {
            coBorrowerData["attorneyInfo"] = SerializeAttorney(coBorrowerInfo.coBorrowerAttorney);
        }

        // Co-Borrower Character References
        List<Dictionary<string, object>> coBorrowerCharacterRefs = SerializeCharacterReferences(coBorrowerInfo.coBorrowerCharacterReferences);
        if (coBorrowerCharacterRefs.Count > 0)
        {
            coBorrowerData["characterReferences"] = coBorrowerCharacterRefs;
        }

        buyerData["coBorrowerInfo"] = coBorrowerData;
    }

    string key = dbReference.Child("principalBuyers").Push().Key;
    dbReference.Child("principalBuyers").Child(key).SetValueAsync(buyerData).ContinueWithOnMainThread(task =>
    {
        feedbackText.text = task.IsCompleted ? "Submission Successful!" : "Error submitting form. Please try again.";
        feedbackText.color = task.IsCompleted ? Color.green : Color.red;

        if (task.IsCompleted)
        {
            StartCoroutine(HideFeedbackAfterSeconds(3));
            ClearFormFields(); // Clear on success
        }
    });
}


    Dictionary<string, object> SerializeBuyerInfo(PrincipalBuyerInfo info) => new Dictionary<string, object>
    {
        { "lastName", SafeText(info.lastName) },
        { "firstName", SafeText(info.firstName) },
        { "middleName", SafeText(info.middleName) },
        { "gender", SafeDropdown(info.gender) },
        { "birthday", SafeText(info.birthday) },
        { "age", SafeText(info.age) },
        { "placeOfBirth", SafeText(info.placeOfBirth) },
        { "civilStatus", SafeDropdown(info.civilStatus) },
        { "citizenship", SafeText(info.citizenship) },
        { "religion", SafeText(info.religion) },
        { "contactNumber", SafeText(info.contactNumber) },
        { "facebookAccount", SafeText(info.facebookAccount) },
        { "presentAddress", SafeText(info.presentAddress) },
        { "permanentAddress", SafeText(info.permanentAddress) },
        { "employerName", SafeText(info.employerName) },
        { "businessNature", SafeText(info.businessNature) },
        { "positionAndDepartment", SafeText(info.positionAndDepartment) },
        { "employerAddress", SafeText(info.employerAddress) },
        { "officeTelephone", SafeText(info.officeTelephone) },
        { "emailAddress", SafeText(info.emailAddress) },
        { "contactPerson", SafeText(info.contactPerson) },
        { "tinId", SafeText(info.tinId) },
        { "pagibigMidNo", string.IsNullOrWhiteSpace(SafeText(info.pagibigMidNo)) ? null : SafeText(info.pagibigMidNo) },
        { "sourceOfIncome", SafeDropdown(info.sourceOfIncome) }
    };

    string SafeText(TMP_InputField field)
    {
        return field != null ? field.text : "";
    }

    string SafeDropdown(TMP_Dropdown dropdown)
    {
        return dropdown != null && dropdown.options.Count > dropdown.value ? dropdown.options[dropdown.value].text : "";
    }

    Dictionary<string, object> SerializeSpouseInfo(SpouseInfo info) => new Dictionary<string, object>
    {
        { "lastName", info.lastName.text },
        { "firstName", info.firstName.text },
        { "middleName", info.middleName.text },
        { "gender", info.gender.options[info.gender.value].text },
        { "birthday", info.birthday.text },
        { "age", info.age.text },
        { "placeOfBirth", info.placeOfBirth.text },
        { "civilStatus", info.civilStatus.options[info.civilStatus.value].text },
        { "citizenship", info.citizenship.text },
        { "religion", info.religion.text },
        { "contactNumber", info.contactNumber.text },
        { "facebookAccount", info.facebookAccount.text },
        { "employerName", info.employerName.text },
        { "businessNature", info.businessNature.text },
        { "positionAndDepartment", info.positionAndDepartment.text },
        { "employerAddress", info.employerAddress.text },
        { "officeTelephone", info.officeTelephone.text },
        { "emailAddress", info.emailAddress.text },
        { "contactPerson", info.contactPerson.text },
        { "tinId", info.tinId.text },
        { "pagibigMidNo", string.IsNullOrWhiteSpace(info.pagibigMidNo.text) ? null : info.pagibigMidNo.text },
        { "sourceOfIncome", info.sourceOfIncome.options[info.sourceOfIncome.value].text }
    };

    Dictionary<string, object> SerializeAttorney(AttorneyInFactInfo info) => new Dictionary<string, object>
    {
        { "lastName", info.lastName.text },
        { "firstName", info.firstName.text },
        { "middleName", info.middleName.text },
        { "gender", info.gender.options[info.gender.value].text },
        { "contactInfo", info.contactInfo.text },
        { "address", info.address.text }
    };

    List<Dictionary<string, object>> SerializeCharacterReferences(CharacterReference[] references)
    {
        var list = new List<Dictionary<string, object>>();
        foreach (var r in references)
        {
            // Only add to the list if at least one field in the character reference is filled
            bool filled = !string.IsNullOrWhiteSpace(r.lastName.text) || !string.IsNullOrWhiteSpace(r.firstName.text) ||
                          !string.IsNullOrWhiteSpace(r.middleName.text) || !string.IsNullOrWhiteSpace(r.contactNumber.text) ||
                          !string.IsNullOrWhiteSpace(r.facebookAccount.text) || !string.IsNullOrWhiteSpace(r.address.text);

            if (filled)
            {
                list.Add(new Dictionary<string, object>
                {
                    { "lastName", r.lastName.text },
                    { "firstName", r.firstName.text },
                    { "middleName", r.middleName.text },
                    { "contactNumber", r.contactNumber.text },
                    { "facebookAccount", r.facebookAccount.text },
                    { "address", r.address.text }
                });
            }
        }
        return list;
    }

    Dictionary<string, object> SerializeMarketing(MarketingDepartmentInfo info) => new Dictionary<string, object>
    {
        { "subdivisionBlockLot", info.subdivisionBlockLot.text },
        { "houseDescription", info.houseDescription.text },
        { "lotArea", info.lotArea.text },
        { "houseArea", info.houseArea.text },
        { "totalContractPrice", info.totalContractPrice.text },
        { "downPayment", info.downPayment.text },
        { "dpTerm", info.dpTerm.text },
        { "loanAmount", info.loanAmount.text },
        { "maTerm", info.maTerm.text },
        { "monthlyAmortization", info.monthlyAmortization.text }
    };

    IEnumerator HideFeedbackAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        feedbackText.text = "";
    }

    bool ValidateField(string label, TMP_InputField field)
    {
        if (string.IsNullOrWhiteSpace(field.text))
        {
            feedbackText.text = $"{label} is required";
            feedbackText.color = Color.red;
            return false;
        }
        return true;
    }

    // Helper to check if any field in PrincipalBuyerInfo is filled
    bool IsPrincipalBuyerInfoFilled(PrincipalBuyerInfo info)
    {
        return !string.IsNullOrWhiteSpace(info.lastName.text) ||
               !string.IsNullOrWhiteSpace(info.firstName.text) ||
               !string.IsNullOrWhiteSpace(info.middleName.text) ||
               !string.IsNullOrWhiteSpace(info.birthday.text) ||
               !string.IsNullOrWhiteSpace(info.age.text) ||
               !string.IsNullOrWhiteSpace(info.placeOfBirth.text) ||
               !string.IsNullOrWhiteSpace(info.citizenship.text) ||
               !string.IsNullOrWhiteSpace(info.religion.text) ||
               !string.IsNullOrWhiteSpace(info.contactNumber.text) ||
               !string.IsNullOrWhiteSpace(info.facebookAccount.text) ||
               !string.IsNullOrWhiteSpace(info.presentAddress.text) ||
               !string.IsNullOrWhiteSpace(info.permanentAddress.text) ||
               !string.IsNullOrWhiteSpace(info.employerName.text) ||
               !string.IsNullOrWhiteSpace(info.businessNature.text) ||
               !string.IsNullOrWhiteSpace(info.positionAndDepartment.text) ||
               !string.IsNullOrWhiteSpace(info.employerAddress.text) ||
               !string.IsNullOrWhiteSpace(info.officeTelephone.text) ||
               !string.IsNullOrWhiteSpace(info.emailAddress.text) ||
               !string.IsNullOrWhiteSpace(info.contactPerson.text) ||
               !string.IsNullOrWhiteSpace(info.tinId.text);
    }

    // Helper to check if any field in SpouseInfo is filled
    bool IsSpouseInfoFilled(SpouseInfo info)
    {
        return !string.IsNullOrWhiteSpace(info.lastName.text) ||
               !string.IsNullOrWhiteSpace(info.firstName.text) ||
               !string.IsNullOrWhiteSpace(info.middleName.text) ||
               !string.IsNullOrWhiteSpace(info.birthday.text) ||
               !string.IsNullOrWhiteSpace(info.age.text) ||
               !string.IsNullOrWhiteSpace(info.placeOfBirth.text) ||
               !string.IsNullOrWhiteSpace(info.citizenship.text) ||
               !string.IsNullOrWhiteSpace(info.religion.text) ||
               !string.IsNullOrWhiteSpace(info.contactNumber.text) ||
               !string.IsNullOrWhiteSpace(info.facebookAccount.text) ||
               !string.IsNullOrWhiteSpace(info.employerName.text) ||
               !string.IsNullOrWhiteSpace(info.businessNature.text) ||
               !string.IsNullOrWhiteSpace(info.positionAndDepartment.text) ||
               !string.IsNullOrWhiteSpace(info.employerAddress.text) ||
               !string.IsNullOrWhiteSpace(info.officeTelephone.text) ||
               !string.IsNullOrWhiteSpace(info.emailAddress.text) ||
               !string.IsNullOrWhiteSpace(info.contactPerson.text) ||
               !string.IsNullOrWhiteSpace(info.tinId.text);
    }

    // Helper to check if any field in AttorneyInFactInfo is filled
    bool IsAttorneyInfoFilled(AttorneyInFactInfo info)
    {
        return !string.IsNullOrWhiteSpace(info.lastName.text) ||
               !string.IsNullOrWhiteSpace(info.firstName.text) ||
               !string.IsNullOrWhiteSpace(info.middleName.text) ||
               !string.IsNullOrWhiteSpace(info.contactInfo.text) ||
               !string.IsNullOrWhiteSpace(info.address.text);
    }

    // Helper to check if any field in CharacterReference is filled
    bool IsCharacterReferenceFilled(CharacterReference reference)
    {
        return !string.IsNullOrWhiteSpace(reference.lastName.text) ||
               !string.IsNullOrWhiteSpace(reference.firstName.text) ||
               !string.IsNullOrWhiteSpace(reference.middleName.text) ||
               !string.IsNullOrWhiteSpace(reference.contactNumber.text) ||
               !string.IsNullOrWhiteSpace(reference.facebookAccount.text) ||
               !string.IsNullOrWhiteSpace(reference.address.text);
    }


    bool ValidateForm()
    {
        // Principal Buyer required fields
        if (!ValidateField("Last Name", principalBuyerInfo.lastName)) return false;
        if (!ValidateField("First Name", principalBuyerInfo.firstName)) return false;
        if (!ValidateField("Middle Name", principalBuyerInfo.middleName)) return false;
        // ... (other principal buyer validations remain the same) ...
        if (!ValidateField("Birthday", principalBuyerInfo.birthday)) return false;
        if (!ValidateField("Age", principalBuyerInfo.age)) return false;
        if (!ValidateField("Place of Birth", principalBuyerInfo.placeOfBirth)) return false;
        if (!ValidateField("Citizenship", principalBuyerInfo.citizenship)) return false;
        if (!ValidateField("Religion", principalBuyerInfo.religion)) return false;
        if (!ValidateField("Contact Number", principalBuyerInfo.contactNumber)) return false;
        if (!ValidateField("Facebook Account", principalBuyerInfo.facebookAccount)) return false;
        if (!ValidateField("Present Address", principalBuyerInfo.presentAddress)) return false;
        if (!ValidateField("Permanent Address", principalBuyerInfo.permanentAddress)) return false;
        if (!ValidateField("Employer Name", principalBuyerInfo.employerName)) return false;
        if (!ValidateField("Nature of Business", principalBuyerInfo.businessNature)) return false;
        if (!ValidateField("Position and Department", principalBuyerInfo.positionAndDepartment)) return false;
        if (!ValidateField("Employer Address", principalBuyerInfo.employerAddress)) return false;
        if (!ValidateField("Office Telephone", principalBuyerInfo.officeTelephone)) return false;
        if (!ValidateField("Email Address", principalBuyerInfo.emailAddress)) return false;
        if (!ValidateField("Contact Person", principalBuyerInfo.contactPerson)) return false;
        if (!ValidateField("TIN ID", principalBuyerInfo.tinId)) return false;


        // Validate Spouse Info if Married
        if (principalBuyerInfo.civilStatus.options[principalBuyerInfo.civilStatus.value].text == "Married")
        {
            if (!ValidateField("Spouse Last Name", spouseInfo.lastName)) return false;
            if (!ValidateField("Spouse First Name", spouseInfo.firstName)) return false;
            if (!ValidateField("Spouse Middle Name", spouseInfo.middleName)) return false;
            if (!ValidateField("Spouse Birthday", spouseInfo.birthday)) return false;
            if (!ValidateField("Spouse Age", spouseInfo.age)) return false;
            if (!ValidateField("Spouse Place of Birth", spouseInfo.placeOfBirth)) return false;
            if (!ValidateField("Spouse Citizenship", spouseInfo.citizenship)) return false;
            if (!ValidateField("Spouse Religion", spouseInfo.religion)) return false;
            if (!ValidateField("Spouse Contact Number", spouseInfo.contactNumber)) return false;
            if (!ValidateField("Spouse Facebook Account", spouseInfo.facebookAccount)) return false;
            if (!ValidateField("Spouse Employer Name", spouseInfo.employerName)) return false;
            if (!ValidateField("Spouse Nature of Business", spouseInfo.businessNature)) return false;
            if (!ValidateField("Spouse Position and Department", spouseInfo.positionAndDepartment)) return false;
            if (!ValidateField("Spouse Employer Address", spouseInfo.employerAddress)) return false;
            if (!ValidateField("Spouse Office Telephone", spouseInfo.officeTelephone)) return false;
            if (!ValidateField("Spouse Email Address", spouseInfo.emailAddress)) return false;
            if (!ValidateField("Spouse Contact Person", spouseInfo.contactPerson)) return false;
            if (!ValidateField("Spouse TIN ID", spouseInfo.tinId)) return false;
        }

        // Attorney-in-Fact: If any filled, all must be filled
        bool anyAttorneyFieldFilled = IsAttorneyInfoFilled(attorneyInfo);
        if (anyAttorneyFieldFilled)
        {
            if (!ValidateField("Attorney Last Name", attorneyInfo.lastName)) return false;
            if (!ValidateField("Attorney First Name", attorneyInfo.firstName)) return false;
            if (!ValidateField("Attorney Middle Name", attorneyInfo.middleName)) return false;
            if (!ValidateField("Attorney Contact Info", attorneyInfo.contactInfo)) return false;
            if (!ValidateField("Attorney Address", attorneyInfo.address)) return false;
        }

        // Character References: If any field filled in one entry, all in that entry must be filled
        for (int i = 0; i < characterReferences.Length; i++)
        {
            var reference = characterReferences[i];
            bool anyFilled = IsCharacterReferenceFilled(reference);

            if (anyFilled)
            {
                if (!ValidateField($"Character Reference {i + 1} Last Name", reference.lastName)) return false;
                if (!ValidateField($"Character Reference {i + 1} First Name", reference.firstName)) return false;
                if (!ValidateField($"Character Reference {i + 1} Middle Name", reference.middleName)) return false;
                if (!ValidateField($"Character Reference {i + 1} Contact Number", reference.contactNumber)) return false;
                if (!ValidateField($"Character Reference {i + 1} Facebook Account", reference.facebookAccount)) return false;
                if (!ValidateField($"Character Reference {i + 1} Address", reference.address)) return false;
            }
        }

        // Co-Borrower validation logic
        bool isCoBorrowerFilled = IsPrincipalBuyerInfoFilled(coBorrowerInfo.coBorrower);

        if (isCoBorrowerFilled)
        {
            // Co-Borrower required fields if any of their fields are filled
            if (!ValidateField("Co-Borrower Last Name", coBorrowerInfo.coBorrower.lastName)) return false;
            if (!ValidateField("Co-Borrower First Name", coBorrowerInfo.coBorrower.firstName)) return false;
            if (!ValidateField("Co-Borrower Middle Name", coBorrowerInfo.coBorrower.middleName)) return false;
            if (!ValidateField("Co-Borrower Birthday", coBorrowerInfo.coBorrower.birthday)) return false;
            if (!ValidateField("Co-Borrower Age", coBorrowerInfo.coBorrower.age)) return false;
            if (!ValidateField("Co-Borrower Place of Birth", coBorrowerInfo.coBorrower.placeOfBirth)) return false;
            if (!ValidateField("Co-Borrower Citizenship", coBorrowerInfo.coBorrower.citizenship)) return false;
            if (!ValidateField("Co-Borrower Religion", coBorrowerInfo.coBorrower.religion)) return false;
            if (!ValidateField("Co-Borrower Contact Number", coBorrowerInfo.coBorrower.contactNumber)) return false;
            if (!ValidateField("Co-Borrower Facebook Account", coBorrowerInfo.coBorrower.facebookAccount)) return false;
            if (!ValidateField("Co-Borrower Present Address", coBorrowerInfo.coBorrower.presentAddress)) return false;
            if (!ValidateField("Co-Borrower Permanent Address", coBorrowerInfo.coBorrower.permanentAddress)) return false;
            if (!ValidateField("Co-Borrower Employer Name", coBorrowerInfo.coBorrower.employerName)) return false;
            if (!ValidateField("Co-Borrower Nature of Business", coBorrowerInfo.coBorrower.businessNature)) return false;
            if (!ValidateField("Co-Borrower Position and Department", coBorrowerInfo.coBorrower.positionAndDepartment)) return false;
            if (!ValidateField("Co-Borrower Employer Address", coBorrowerInfo.coBorrower.employerAddress)) return false;
            if (!ValidateField("Co-Borrower Office Telephone", coBorrowerInfo.coBorrower.officeTelephone)) return false;
            if (!ValidateField("Co-Borrower Email Address", coBorrowerInfo.coBorrower.emailAddress)) return false;
            if (!ValidateField("Co-Borrower Contact Person", coBorrowerInfo.coBorrower.contactPerson)) return false;
            if (!ValidateField("Co-Borrower TIN ID", coBorrowerInfo.coBorrower.tinId)) return false;

            // Co-Borrower Spouse: if Married, spouse info must be completed
            string coBorrowerCivilStatus = coBorrowerInfo.coBorrower.civilStatus.options[coBorrowerInfo.coBorrower.civilStatus.value].text;
            if (coBorrowerCivilStatus == "Married")
            {
                if (!ValidateField("Co-Borrower Spouse Last Name", coBorrowerInfo.coBorrowerSpouse.lastName)) return false;
                if (!ValidateField("Co-Borrower Spouse First Name", coBorrowerInfo.coBorrowerSpouse.firstName)) return false;
                if (!ValidateField("Co-Borrower Spouse Middle Name", coBorrowerInfo.coBorrowerSpouse.middleName)) return false;
                if (!ValidateField("Co-Borrower Spouse Birthday", coBorrowerInfo.coBorrowerSpouse.birthday)) return false;
                if (!ValidateField("Co-Borrower Spouse Age", coBorrowerInfo.coBorrowerSpouse.age)) return false;
                if (!ValidateField("Co-Borrower Spouse Place of Birth", coBorrowerInfo.coBorrowerSpouse.placeOfBirth)) return false;
                if (!ValidateField("Co-Borrower Spouse Citizenship", coBorrowerInfo.coBorrowerSpouse.citizenship)) return false;
                if (!ValidateField("Co-Borrower Spouse Religion", coBorrowerInfo.coBorrowerSpouse.religion)) return false;
                if (!ValidateField("Co-Borrower Spouse Contact Number", coBorrowerInfo.coBorrowerSpouse.contactNumber)) return false;
                if (!ValidateField("Co-Borrower Spouse Facebook Account", coBorrowerInfo.coBorrowerSpouse.facebookAccount)) return false;
                if (!ValidateField("Co-Borrower Spouse Employer Name", coBorrowerInfo.coBorrowerSpouse.employerName)) return false;
                if (!ValidateField("Co-Borrower Spouse Nature of Business", coBorrowerInfo.coBorrowerSpouse.businessNature)) return false;
                if (!ValidateField("Co-Borrower Spouse Position and Department", coBorrowerInfo.coBorrowerSpouse.positionAndDepartment)) return false;
                if (!ValidateField("Co-Borrower Spouse Employer Address", coBorrowerInfo.coBorrowerSpouse.employerAddress)) return false;
                if (!ValidateField("Co-Borrower Spouse Office Telephone", coBorrowerInfo.coBorrowerSpouse.officeTelephone)) return false;
                if (!ValidateField("Co-Borrower Spouse Email Address", coBorrowerInfo.coBorrowerSpouse.emailAddress)) return false;
                if (!ValidateField("Co-Borrower Spouse Contact Person", coBorrowerInfo.coBorrowerSpouse.contactPerson)) return false;
                if (!ValidateField("Co-Borrower Spouse TIN ID", coBorrowerInfo.coBorrowerSpouse.tinId)) return false;
            }

            // Co-Borrower Attorney-in-Fact: If any filled, all must be filled
            bool anyCoBorrowerAttorneyFieldFilled = IsAttorneyInfoFilled(coBorrowerInfo.coBorrowerAttorney);
            if (anyCoBorrowerAttorneyFieldFilled)
            {
                if (!ValidateField("Co-Borrower Attorney Last Name", coBorrowerInfo.coBorrowerAttorney.lastName)) return false;
                if (!ValidateField("Co-Borrower Attorney First Name", coBorrowerInfo.coBorrowerAttorney.firstName)) return false;
                if (!ValidateField("Co-Borrower Attorney Middle Name", coBorrowerInfo.coBorrowerAttorney.middleName)) return false;
                if (!ValidateField("Co-Borrower Attorney Contact Info", coBorrowerInfo.coBorrowerAttorney.contactInfo)) return false;
                if (!ValidateField("Co-Borrower Attorney Address", coBorrowerInfo.coBorrowerAttorney.address)) return false;
            }

            // Co-Borrower Character References: If any field filled in one entry, all in that entry must be filled
            for (int i = 0; i < coBorrowerInfo.coBorrowerCharacterReferences.Length; i++)
            {
                var reference = coBorrowerInfo.coBorrowerCharacterReferences[i];
                bool anyFilled = IsCharacterReferenceFilled(reference);

                if (anyFilled)
                {
                    if (!ValidateField($"Co-Borrower Character Reference {i + 1} Last Name", reference.lastName)) return false;
                    if (!ValidateField($"Co-Borrower Character Reference {i + 1} First Name", reference.firstName)) return false;
                    if (!ValidateField($"Co-Borrower Character Reference {i + 1} Middle Name", reference.middleName)) return false;
                    if (!ValidateField($"Co-Borrower Character Reference {i + 1} Contact Number", reference.contactNumber)) return false;
                    if (!ValidateField($"Co-Borrower Character Reference {i + 1} Facebook Account", reference.facebookAccount)) return false;
                    if (!ValidateField($"Co-Borrower Character Reference {i + 1} Address", reference.address)) return false;
                }
            }
        }

        return true;
    }

    // New method to clear all input fields
    void ClearFormFields()
    {
        // Clear Principal Buyer Info
        ClearPrincipalBuyerInfo(principalBuyerInfo);

        // Clear Spouse Info if married
        if (principalBuyerInfo.civilStatus.options[principalBuyerInfo.civilStatus.value].text == "Married")
        {
            ClearSpouseInfo(spouseInfo);
        }

        // Clear Attorney Info
        ClearAttorneyInfo(attorneyInfo);

        // Clear Character References
        foreach (var reference in characterReferences)
        {
            ClearCharacterReference(reference);
        }

        // Clear Marketing Info
        ClearMarketingInfo(marketingInfo);

        // Clear Co-Borrower Info
        ClearPrincipalBuyerInfo(coBorrowerInfo.coBorrower);
        // Clear Co-Borrower Spouse Info
        ClearSpouseInfo(coBorrowerInfo.coBorrowerSpouse);
        // Clear Co-Borrower Attorney Info
        ClearAttorneyInfo(coBorrowerInfo.coBorrowerAttorney);
        // Clear Co-Borrower Character References
        foreach (var reference in coBorrowerInfo.coBorrowerCharacterReferences)
        {
            ClearCharacterReference(reference);
        }

        // Reset dropdowns to default/first option (assuming 0 is a valid default)
        principalBuyerInfo.gender.value = 0;
        principalBuyerInfo.civilStatus.value = 0;
        principalBuyerInfo.sourceOfIncome.value = 0;

        spouseInfo.gender.value = 0;
        spouseInfo.civilStatus.value = 0;
        spouseInfo.sourceOfIncome.value = 0;

        attorneyInfo.gender.value = 0;

        coBorrowerInfo.coBorrower.gender.value = 0;
        coBorrowerInfo.coBorrower.civilStatus.value = 0;
        coBorrowerInfo.coBorrower.sourceOfIncome.value = 0;

        coBorrowerInfo.coBorrowerSpouse.gender.value = 0;
        coBorrowerInfo.coBorrowerSpouse.civilStatus.value = 0;
        coBorrowerInfo.coBorrowerSpouse.sourceOfIncome.value = 0;

        coBorrowerInfo.coBorrowerAttorney.gender.value = 0;
    }

    // Helper methods to clear individual sections
    void ClearPrincipalBuyerInfo(PrincipalBuyerInfo info)
    {
        info.lastName.text = "";
        info.firstName.text = "";
        info.middleName.text = "";
        info.birthday.text = "";
        info.age.text = "";
        info.placeOfBirth.text = "";
        info.citizenship.text = "";
        info.religion.text = "";
        info.contactNumber.text = "";
        info.facebookAccount.text = "";
        info.presentAddress.text = "";
        info.permanentAddress.text = "";
        info.employerName.text = "";
        info.businessNature.text = "";
        info.positionAndDepartment.text = "";
        info.employerAddress.text = "";
        info.officeTelephone.text = "";
        info.emailAddress.text = "";
        info.contactPerson.text = "";
        info.tinId.text = "";
        info.pagibigMidNo.text = "";
    }

    void ClearSpouseInfo(SpouseInfo info)
    {
        info.lastName.text = "";
        info.firstName.text = "";
        info.middleName.text = "";
        info.birthday.text = "";
        info.age.text = "";
        info.placeOfBirth.text = "";
        info.citizenship.text = "";
        info.religion.text = "";
        info.contactNumber.text = "";
        info.facebookAccount.text = "";
        info.employerName.text = "";
        info.businessNature.text = "";
        info.positionAndDepartment.text = "";
        info.employerAddress.text = "";
        info.officeTelephone.text = "";
        info.emailAddress.text = "";
        info.contactPerson.text = "";
        info.tinId.text = "";
        info.pagibigMidNo.text = "";
    }

    void ClearAttorneyInfo(AttorneyInFactInfo info)
    {
        info.lastName.text = "";
        info.firstName.text = "";
        info.middleName.text = "";
        info.contactInfo.text = "";
        info.address.text = "";
    }

    void ClearCharacterReference(CharacterReference reference)
    {
        reference.lastName.text = "";
        reference.firstName.text = "";
        reference.middleName.text = "";
        reference.contactNumber.text = "";
        reference.facebookAccount.text = "";
        reference.address.text = "";
    }

    void ClearMarketingInfo(MarketingDepartmentInfo info)
    {
        info.subdivisionBlockLot.text = "";
        info.houseDescription.text = "";
        info.lotArea.text = "";
        info.houseArea.text = "";
        info.totalContractPrice.text = "";
        info.downPayment.text = "";
        info.dpTerm.text = "";
        info.loanAmount.text = "";
        info.maTerm.text = "";
        info.monthlyAmortization.text = "";
    }
}