using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FormNavigationWithValidation : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject[] formPanels;

    [Header("Navigation")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private TMP_Text stepText;
    [SerializeField] private Image progressBarFill;
    [SerializeField] private TMP_Text errorText;

    [Header("Form Input Data")]
    public PrincipalBuyerForm principalBuyerForm;

    private int currentStep = 0;

    void Start()
    {
        ShowCurrentPanel();
        nextButton.onClick.AddListener(NextPanel);
        previousButton.onClick.AddListener(PreviousPanel);
    }

    public void NextPanel()
    {
        if (!ValidateCurrentStep())
            return;

        if (currentStep < formPanels.Length - 1)
        {
            formPanels[currentStep].SetActive(false);
            currentStep++;
            ShowCurrentPanel();
        }
    }

    public void PreviousPanel()
    {
        if (currentStep > 0)
        {
            formPanels[currentStep].SetActive(false);
            currentStep--;
            ShowCurrentPanel();
        }
    }

    private void ShowCurrentPanel()
    {
        for (int i = 0; i < formPanels.Length; i++)
        {
            formPanels[i].SetActive(i == currentStep);
        }

        stepText.text = $"Step {currentStep + 1} of {formPanels.Length}";
        progressBarFill.fillAmount = (float)(currentStep + 1) / formPanels.Length;

        previousButton.interactable = currentStep > 0;
        nextButton.interactable = currentStep < formPanels.Length - 1;

        errorText.text = ""; // Clear error
    }

    private bool ValidateCurrentStep()
    {
        bool isValid = true;
        errorText.text = "";

        switch (currentStep)
        {
            case 0:
                isValid = principalBuyerForm.ValidateForm();
                break;

            case 1:
                if (principalBuyerForm.principalBuyerInfo.civilStatus.options[principalBuyerForm.principalBuyerInfo.civilStatus.value].text == "Married")
                {
                    isValid = principalBuyerForm.ValidateField("Spouse Last Name", principalBuyerForm.spouseInfo.lastName)
                           && principalBuyerForm.ValidateField("Spouse First Name", principalBuyerForm.spouseInfo.firstName)
                           && principalBuyerForm.ValidateField("Spouse Middle Name", principalBuyerForm.spouseInfo.middleName)
                           && principalBuyerForm.ValidateField("Birthday", principalBuyerForm.spouseInfo.birthday)
                           && principalBuyerForm.ValidateField("Age", principalBuyerForm.spouseInfo.age)
                           && principalBuyerForm.ValidateField("Place of Birth", principalBuyerForm.spouseInfo.placeOfBirth)
                           && principalBuyerForm.ValidateField("Citizenship", principalBuyerForm.spouseInfo.citizenship)
                           && principalBuyerForm.ValidateField("Religion", principalBuyerForm.spouseInfo.religion)
                           && principalBuyerForm.ValidateField("Contact Number", principalBuyerForm.spouseInfo.contactNumber)
                           && principalBuyerForm.ValidateField("Facebook Account", principalBuyerForm.spouseInfo.facebookAccount)
                           && principalBuyerForm.ValidateField("Employer Name", principalBuyerForm.spouseInfo.employerName)
                           && principalBuyerForm.ValidateField("Nature of Business", principalBuyerForm.spouseInfo.businessNature)
                           && principalBuyerForm.ValidateField("Position and Department", principalBuyerForm.spouseInfo.positionAndDepartment)
                           && principalBuyerForm.ValidateField("Employer Address", principalBuyerForm.spouseInfo.employerAddress)
                           && principalBuyerForm.ValidateField("Office Telephone", principalBuyerForm.spouseInfo.officeTelephone)
                           && principalBuyerForm.ValidateField("Email Address", principalBuyerForm.spouseInfo.emailAddress)
                           && principalBuyerForm.ValidateField("Contact Person", principalBuyerForm.spouseInfo.contactPerson)
                           && principalBuyerForm.ValidateField("TIN ID", principalBuyerForm.spouseInfo.tinId)
                           && principalBuyerForm.ValidateField("PAGIBIG No.", principalBuyerForm.spouseInfo.pagibigNo)
                           && principalBuyerForm.ValidateField("Source of Income", principalBuyerForm.spouseInfo.sourceOfIncome);
                }
                break;

            case 2:
                if (principalBuyerForm.IsAttorneyInfoFilled(principalBuyerForm.attorneyInfo))
                {
                    isValid = principalBuyerForm.ValidateField("Attorney Last Name", principalBuyerForm.attorneyInfo.lastName)
                           && principalBuyerForm.ValidateField("Attorney First Name", principalBuyerForm.attorneyInfo.firstName)
                           && principalBuyerForm.ValidateField("Attorney Middle Name", principalBuyerForm.attorneyInfo.middleName)
                           && principalBuyerForm.ValidateField("Attorney Contact Info", principalBuyerForm.attorneyInfo.contactInfo)
                           && principalBuyerForm.ValidateField("Attorney Address", principalBuyerForm.attorneyInfo.address);
                }
                break;

            case 3:
                for (int i = 0; i < principalBuyerForm.characterReferences.Length; i++)
                {
                    var reference = principalBuyerForm.characterReferences[i];
                    if (principalBuyerForm.IsCharacterReferenceFilled(reference))
                    {
                        isValid &= principalBuyerForm.ValidateField($"Character Reference {i + 1} Last Name", reference.lastName)
                                && principalBuyerForm.ValidateField($"Character Reference {i + 1} First Name", reference.firstName)
                                && principalBuyerForm.ValidateField($"Character Reference {i + 1} Middle Name", reference.middleName)
                                && principalBuyerForm.ValidateField($"Character Reference {i + 1} Contact Number", reference.contactNumber)
                                && principalBuyerForm.ValidateField($"Character Reference {i + 1} Facebook Account", reference.facebookAccount)
                                && principalBuyerForm.ValidateField($"Character Reference {i + 1} Address", reference.address);
                    }
                }
                break;

            case 4:
                if (principalBuyerForm.IsPrincipalBuyerInfoFilled(principalBuyerForm.coBorrowerInfo.coBorrower))
                {
                    isValid = principalBuyerForm.ValidateField("Co-Borrower Last Name", principalBuyerForm.coBorrowerInfo.coBorrower.lastName)
                           && principalBuyerForm.ValidateField("Co-Borrower First Name", principalBuyerForm.coBorrowerInfo.coBorrower.firstName)
                           && principalBuyerForm.ValidateField("Co-Borrower Middle Name", principalBuyerForm.coBorrowerInfo.coBorrower.middleName)
                           && principalBuyerForm.ValidateField("Birthday", principalBuyerForm.coBorrowerInfo.coBorrower.birthday)
                           && principalBuyerForm.ValidateField("Age", principalBuyerForm.coBorrowerInfo.coBorrower.age)
                           && principalBuyerForm.ValidateField("Place of Birth", principalBuyerForm.coBorrowerInfo.coBorrower.placeOfBirth)
                           && principalBuyerForm.ValidateField("Citizenship", principalBuyerForm.coBorrowerInfo.coBorrower.citizenship)
                           && principalBuyerForm.ValidateField("Religion", principalBuyerForm.coBorrowerInfo.coBorrower.religion)
                           && principalBuyerForm.ValidateField("Contact Number", principalBuyerForm.coBorrowerInfo.coBorrower.contactNumber)
                           && principalBuyerForm.ValidateField("Facebook Account", principalBuyerForm.coBorrowerInfo.coBorrower.facebookAccount)
                           && principalBuyerForm.ValidateField("Present Address", principalBuyerForm.coBorrowerInfo.coBorrower.presentAddress)
                           && principalBuyerForm.ValidateField("Permanent Address", principalBuyerForm.coBorrowerInfo.coBorrower.permanentAddress)
                           && principalBuyerForm.ValidateField("Employer Name", principalBuyerForm.coBorrowerInfo.coBorrower.employerName)
                           && principalBuyerForm.ValidateField("Nature of Business", principalBuyerForm.coBorrowerInfo.coBorrower.businessNature)
                           && principalBuyerForm.ValidateField("Position and Department", principalBuyerForm.coBorrowerInfo.coBorrower.positionAndDepartment)
                           && principalBuyerForm.ValidateField("Employer Address", principalBuyerForm.coBorrowerInfo.coBorrower.employerAddress)
                           && principalBuyerForm.ValidateField("Office Telephone", principalBuyerForm.coBorrowerInfo.coBorrower.officeTelephone)
                           && principalBuyerForm.ValidateField("Email Address", principalBuyerForm.coBorrowerInfo.coBorrower.emailAddress)
                           && principalBuyerForm.ValidateField("Contact Person", principalBuyerForm.coBorrowerInfo.coBorrower.contactPerson)
                           && principalBuyerForm.ValidateField("TIN ID", principalBuyerForm.coBorrowerInfo.coBorrower.tinId)
                           && principalBuyerForm.ValidateField("PAGIBIG No.", principalBuyerForm.coBorrowerInfo.coBorrower.pagibigNo)
                           && principalBuyerForm.ValidateField("Source of Income", principalBuyerForm.coBorrowerInfo.coBorrower.sourceOfIncome);
                }
                break;

            case 5:
                if (principalBuyerForm.coBorrowerInfo.coBorrower.civilStatus.options[principalBuyerForm.coBorrowerInfo.coBorrower.civilStatus.value].text == "Married")
                {
                    isValid = principalBuyerForm.ValidateField("Co-Borrower Spouse Last Name", principalBuyerForm.coBorrowerInfo.coBorrowerSpouse.lastName)
                           && principalBuyerForm.ValidateField("Co-Borrower Spouse First Name", principalBuyerForm.coBorrowerInfo.coBorrowerSpouse.firstName)
                           && principalBuyerForm.ValidateField("Co-Borrower Spouse Middle Name", principalBuyerForm.coBorrowerInfo.coBorrowerSpouse.middleName)
                           && principalBuyerForm.ValidateField("Birthday", principalBuyerForm.coBorrowerInfo.coBorrowerSpouse.birthday)
                           && principalBuyerForm.ValidateField("Age", principalBuyerForm.coBorrowerInfo.coBorrowerSpouse.age)
                           && principalBuyerForm.ValidateField("Place of Birth", principalBuyerForm.coBorrowerInfo.coBorrowerSpouse.placeOfBirth)
                           && principalBuyerForm.ValidateField("Citizenship", principalBuyerForm.coBorrowerInfo.coBorrowerSpouse.citizenship)
                           && principalBuyerForm.ValidateField("Religion", principalBuyerForm.coBorrowerInfo.coBorrowerSpouse.religion)
                           && principalBuyerForm.ValidateField("Contact Number", principalBuyerForm.coBorrowerInfo.coBorrowerSpouse.contactNumber)
                           && principalBuyerForm.ValidateField("Facebook Account", principalBuyerForm.coBorrowerInfo.coBorrowerSpouse.facebookAccount)
                           && principalBuyerForm.ValidateField("Employer Name", principalBuyerForm.coBorrowerInfo.coBorrowerSpouse.employerName)
                           && principalBuyerForm.ValidateField("Nature of Business", principalBuyerForm.coBorrowerInfo.coBorrowerSpouse.businessNature)
                           && principalBuyerForm.ValidateField("Position and Department", principalBuyerForm.coBorrowerInfo.coBorrowerSpouse.positionAndDepartment)
                           && principalBuyerForm.ValidateField("Employer Address", principalBuyerForm.coBorrowerInfo.coBorrowerSpouse.employerAddress)
                           && principalBuyerForm.ValidateField("Office Telephone", principalBuyerForm.coBorrowerInfo.coBorrowerSpouse.officeTelephone)
                           && principalBuyerForm.ValidateField("Email Address", principalBuyerForm.coBorrowerInfo.coBorrowerSpouse.emailAddress)
                           && principalBuyerForm.ValidateField("Contact Person", principalBuyerForm.coBorrowerInfo.coBorrowerSpouse.contactPerson)
                           && principalBuyerForm.ValidateField("TIN ID", principalBuyerForm.coBorrowerInfo.coBorrowerSpouse.tinId)
                           && principalBuyerForm.ValidateField("PAGIBIG No.", principalBuyerForm.coBorrowerInfo.coBorrowerSpouse.pagibigNo)
                           && principalBuyerForm.ValidateField("Source of Income", principalBuyerForm.coBorrowerInfo.coBorrowerSpouse.sourceOfIncome);
                }
                break;
        }

        if (!isValid)
        {
            errorText.text = "Please complete all required fields in this step.";
        }

        return isValid;
    }
}