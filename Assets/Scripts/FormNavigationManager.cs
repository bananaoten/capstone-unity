using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Database;
using System.Collections.Generic;
using System.Threading.Tasks;

public class FormNavigationManager : MonoBehaviour
{
    [Header("Form Panels (Assign in Order)")]
    [SerializeField] private GameObject[] formPanels;

    [Header("Navigation UI")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button submitFormButton;

    [Header("Submission Status Text")]
    [SerializeField] private TMP_Text submitStatusText; // Drag your TMP_Text here

    [Header("Progress UI")]
    [SerializeField] private TMP_Text stepText;
    [SerializeField] private Image progressBarFill;
    [SerializeField] private Image[] stepIcons;
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color completedColor = Color.blue;

    [Header("Form Data Manager")]
    [SerializeField] private PrincipalBuyerFormUpdated principalBuyerForm;

    [Header("Co-Borrower Form")]
    [SerializeField] private CoBorrowerFormUpdated coBorrowerForm;

    [Header("Submission Status UI")]
    [SerializeField] private TMP_Text submissionStatusText;

    private int currentStepIndex = 0;

    void Start()
    {
        if (submitStatusText != null)
            submitStatusText.gameObject.SetActive(false);

        if (submissionStatusText != null)
        {
            submissionStatusText.text = "";
            submissionStatusText.color = Color.red;
        }

        if (formPanels == null || formPanels.Length == 0)
        {
            Debug.LogError("Form panels are not assigned!");
            return;
        }

        foreach (var panel in formPanels)
            panel.SetActive(false);

        nextButton.onClick.RemoveAllListeners();
        previousButton.onClick.RemoveAllListeners();
        submitFormButton.onClick.RemoveAllListeners();

        nextButton.onClick.AddListener(OnNextClicked);
        previousButton.onClick.AddListener(OnPreviousClicked);
        submitFormButton.onClick.AddListener(SubmitForm);

        ShowCurrentPanel();
    }

    private void OnNextClicked()
    {
        if (!ValidateCurrentStep())
        {
            // No error message, just block navigation
            return;
        }

        formPanels[currentStepIndex].SetActive(false);

        currentStepIndex++;

        // Skip Principal Buyer's Spouse Panel if Single
        if (currentStepIndex == 1 && principalBuyerForm.PrincipalBuyerCivilStatus.ToLower() == "single")
        {
            currentStepIndex++;
        }

        // Skip Co-Borrower's Spouse Panel if Single
        if (currentStepIndex == 5)
        {
            string coBorrowerCivilStatus = coBorrowerForm.CoBorrowerCivilStatus.ToLower();
            if (coBorrowerCivilStatus == "single")
            {
                currentStepIndex++;
            }
        }

        currentStepIndex = Mathf.Clamp(currentStepIndex, 0, formPanels.Length - 1);
        ClearSubmissionStatus();
        ShowCurrentPanel();
    }

    private void OnPreviousClicked()
    {
        formPanels[currentStepIndex].SetActive(false);
        currentStepIndex--;

        // Skip Principal Buyer's Spouse Panel if Single when going backwards
        if (currentStepIndex == 1 && principalBuyerForm.PrincipalBuyerCivilStatus.ToLower() == "single")
        {
            currentStepIndex--;
        }

        // Skip Co-Borrower's Spouse Panel if Single when going backwards
        if (currentStepIndex == 5 && coBorrowerForm.CoBorrowerCivilStatus.ToLower() == "single")
        {
            currentStepIndex--;
        }

        currentStepIndex = Mathf.Clamp(currentStepIndex, 0, formPanels.Length - 1);
        ClearSubmissionStatus();
        ShowCurrentPanel();
    }

   private void SubmitForm()
{
    int totalSteps = formPanels.Length;
    bool isLastStep = currentStepIndex == totalSteps - 1;

    if (!isLastStep || !ValidateCurrentStep())
    {
        ShowSubmissionStatus("Please complete all required fields before submitting.", false);
        return;
    }

    submitFormButton.interactable = false;

    // --- Build flat dictionary like React initialState ---
    var formData = new Dictionary<string, object>
    {
        // Principal Buyer
        { "principalLastName", principalBuyerForm.lastName.text },
        { "principalFirstName", principalBuyerForm.firstName.text },
        { "principalMiddleName", principalBuyerForm.middleName.text },
        { "principalBirthday", principalBuyerForm.birthday.text },
        { "principalAge", principalBuyerForm.age.text },
        { "principalGender", principalBuyerForm.genderDropdown.options[principalBuyerForm.genderDropdown.value].text },
        { "principalCivilStatus", principalBuyerForm.PrincipalBuyerCivilStatus },
        { "principalCitizenship", principalBuyerForm.citizenship.text },
        { "principalReligion", principalBuyerForm.religion.text },
        { "principalContact", principalBuyerForm.contactNumber.text },
        { "principalFacebook", principalBuyerForm.facebookAccount.text },
        { "principalPresentAddress", principalBuyerForm.presentAddress.text },
        { "principalPermanentAddress", principalBuyerForm.permanentAddress.text },
        { "principalEmployersName", principalBuyerForm.employerName.text },
        { "principalNatureOfBusiness", principalBuyerForm.businessNature.text },
        { "principalPositionInDepartment", principalBuyerForm.positionAndDepartment.text },
        { "principalEmployersAddress", principalBuyerForm.employerAddress.text },
        { "principalOfficeTel", principalBuyerForm.officeTelephone.text },
        { "principalEmployerEmail", principalBuyerForm.emailAddress.text },
        { "principalEmployerContactPerson", principalBuyerForm.contactPerson.text },
        { "principalTin", principalBuyerForm.tinId.text },
        { "principalPagibig", principalBuyerForm.pagibigNo.text },
        { "principalSourceOfIncome", principalBuyerForm.sourceOfIncomeDropdown.options[principalBuyerForm.sourceOfIncomeDropdown.value].text },

        // Character References
        { "principalRefName", principalBuyerForm.characterReferenceSection.lastName1.text },
        { "principalRefAddress", principalBuyerForm.characterReferenceSection.address1.text },
        { "principalRefContact", principalBuyerForm.characterReferenceSection.contactNumber1.text },
        { "principalCorefName", principalBuyerForm.characterReferenceSection.lastName2.text },
        { "principalCorefAddress", principalBuyerForm.characterReferenceSection.address2.text },
        { "principalCorefContact", principalBuyerForm.characterReferenceSection.contactNumber2.text },
        { "principalThirdRefName", principalBuyerForm.characterReferenceSection.lastName3.text },
        { "principalThirdRefAddress", principalBuyerForm.characterReferenceSection.address3.text },
        { "principalThirdRefContact", principalBuyerForm.characterReferenceSection.contactNumber3.text },

        // Attorney-in-Fact
        { "AttorneyLastName", principalBuyerForm.attorneyInFactSection.lastName.text },
        { "AttorneyFirstName", principalBuyerForm.attorneyInFactSection.firstName.text },
        { "AttorneyMiddleName", principalBuyerForm.attorneyInFactSection.middleName.text },
        { "Attorneygender", principalBuyerForm.attorneyInFactSection.genderDropdown.options[principalBuyerForm.attorneyInFactSection.genderDropdown.value].text },
        { "AttorneypresentAddress", principalBuyerForm.attorneyInFactSection.address.text },
        { "Attorneycontact", principalBuyerForm.attorneyInFactSection.contactNumberOrEmail.text }
    };

    // Spouse (if married)
    if (principalBuyerForm.PrincipalBuyerCivilStatus.ToLower() == "married")
    {
        formData["spouseLastName"] = principalBuyerForm.spouseSection.lastName.text;
        formData["spouseFirstName"] = principalBuyerForm.spouseSection.firstName.text;
        formData["spouseMiddleName"] = principalBuyerForm.spouseSection.middleName.text;
        formData["spouseBirthday"] = principalBuyerForm.spouseSection.birthday.text;
        formData["spouseAge"] = principalBuyerForm.spouseSection.age.text;
        formData["spouseGender"] = principalBuyerForm.spouseSection.genderDropdown.options[principalBuyerForm.spouseSection.genderDropdown.value].text;
        formData["spouseCitizenship"] = principalBuyerForm.spouseSection.citizenship.text;
        formData["spouseReligion"] = principalBuyerForm.spouseSection.religion.text;
        formData["spouseContact"] = principalBuyerForm.spouseSection.contactNumber.text;
        formData["spouseFacebook"] = principalBuyerForm.spouseSection.facebookAccount.text;
    }

    // Co-Borrower
    if (coBorrowerForm != null)
    {
        formData["coLastName"] = coBorrowerForm.lastName.text;
        formData["coFirstName"] = coBorrowerForm.firstName.text;
        formData["coMiddleName"] = coBorrowerForm.middleName.text;
        formData["coBirthday"] = coBorrowerForm.birthday.text;
        formData["coAge"] = coBorrowerForm.age.text;
        formData["coGender"] = coBorrowerForm.genderDropdown.options[coBorrowerForm.genderDropdown.value].text;
        formData["coCivilStatus"] = coBorrowerForm.CoBorrowerCivilStatus;
        formData["coCitizenship"] = coBorrowerForm.citizenship.text;
        formData["coReligion"] = coBorrowerForm.religion.text;
        formData["coContact"] = coBorrowerForm.contactNumber.text;
        formData["coFacebook"] = coBorrowerForm.facebookAccount.text;

        // Co-Borrower Spouse (if married)
        if (coBorrowerForm.CoBorrowerCivilStatus.ToLower() == "married")
        {
            formData["coSpouseLastName"] = coBorrowerForm.spouseSection.lastName.text;
            formData["coSpouseFirstName"] = coBorrowerForm.spouseSection.firstName.text;
            formData["coSpouseMiddleName"] = coBorrowerForm.spouseSection.middleName.text;
            formData["coSpouseBirthday"] = coBorrowerForm.spouseSection.birthday.text;
            formData["coSpouseAge"] = coBorrowerForm.spouseSection.age.text;
            formData["coSpouseGender"] = coBorrowerForm.spouseSection.genderDropdown.options[coBorrowerForm.spouseSection.genderDropdown.value].text;
            formData["coSpouseCitizenship"] = coBorrowerForm.spouseSection.citizenship.text;
            formData["coSpouseReligion"] = coBorrowerForm.spouseSection.religion.text;
            formData["coSpouseContact"] = coBorrowerForm.spouseSection.contactNumber.text;
            formData["coSpouseFacebook"] = coBorrowerForm.spouseSection.facebookAccount.text;
        }

        // Co-Borrower Attorney
        if (coBorrowerForm.attorneyInFactSection != null)
        {
            formData["coAttorneyLastName"] = coBorrowerForm.attorneyInFactSection.lastName.text;
            formData["coAttorneyFirstName"] = coBorrowerForm.attorneyInFactSection.firstName.text;
            formData["coAttorneyMiddleName"] = coBorrowerForm.attorneyInFactSection.middleName.text;
            formData["coAttorneyGender"] = coBorrowerForm.attorneyInFactSection.genderDropdown.options[coBorrowerForm.attorneyInFactSection.genderDropdown.value].text;
            formData["coAttorneyContact"] = coBorrowerForm.attorneyInFactSection.contactNumberOrEmail.text;
            formData["coAttorneyPresentAddress"] = coBorrowerForm.attorneyInFactSection.address.text;
        }
    }

    // --- Save under buyersForms/{uid} ---
    string uid = "user123"; // TODO: replace with FirebaseAuth.CurrentUser.UserId
    DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    dbRef.Child("buyersForms").Child(uid).SetValueAsync(formData).ContinueWith(task =>
    {
        if (task.IsCompletedSuccessfully)
        {
            ShowSubmissionStatus("YOUR FORM HAS BEEN SUCCESSFULLY SUBMITTED", true);
            if (submitStatusText != null)
            {
                submitStatusText.text = "YOUR FORM HAS BEEN SUCCESSFULLY SUBMITTED";
                submitStatusText.gameObject.SetActive(true);
            }
        }
        else
        {
            ShowSubmissionStatus("Failed to submit the form. Please try again.", false);
            submitFormButton.interactable = true;
        }
    });
}


    private void ShowSubmissionStatus(string message, bool isSuccess)
    {
        Debug.Log("🟢 ShowSubmissionStatus: " + message + " | Success: " + isSuccess);

        if (submissionStatusText == null)
        {
            Debug.LogWarning("⚠️ submissionStatusText is null");
            return;
        }

        // Force-enable GameObject and its parents
        submissionStatusText.gameObject.SetActive(true);
        if (submissionStatusText.transform.parent != null)
            submissionStatusText.transform.parent.gameObject.SetActive(true);

        submissionStatusText.text = message;
        submissionStatusText.color = isSuccess ? Color.green : Color.red;
    }

    private void ClearSubmissionStatus()
    {
        Debug.Log("❌ Clearing submission status text");
        if (submissionStatusText != null)
            submissionStatusText.text = "";
    }

    private void ShowCurrentPanel()
    {
        int totalSteps = formPanels.Length;

        for (int i = 0; i < totalSteps; i++)
            formPanels[i].SetActive(false);

        if (currentStepIndex >= 0 && currentStepIndex < totalSteps)
            formPanels[currentStepIndex].SetActive(true);

        stepText.text = $"Step {currentStepIndex + 1} of {totalSteps}";
        progressBarFill.fillAmount = (float)(currentStepIndex + 1) / totalSteps;

        for (int i = 0; i < stepIcons.Length; i++)
        {
            if (i < currentStepIndex)
                stepIcons[i].color = completedColor;
            else if (i == currentStepIndex)
                stepIcons[i].color = activeColor;
            else
                stepIcons[i].color = inactiveColor;
        }

        previousButton.interactable = currentStepIndex > 0;

        bool isLastStep = currentStepIndex == totalSteps - 1;
        nextButton.gameObject.SetActive(!isLastStep);
        submitFormButton.gameObject.SetActive(isLastStep);

        // Show "PLEASE SUBMIT YOUR FORM" only on the last step
        if (submitStatusText != null)
        {
            if (isLastStep)
            {
                submitStatusText.text = "Please SUBMIT your form";
                submitStatusText.gameObject.SetActive(true);
            }
            else
            {
                submitStatusText.gameObject.SetActive(false);
            }
        }
    }

    private bool ValidateCurrentStep()
    {
        string principalCivilStatus = principalBuyerForm.PrincipalBuyerCivilStatus.ToLower();

        string coBorrowerCivilStatus = "single";
        if (coBorrowerForm != null)
        {
            coBorrowerCivilStatus = coBorrowerForm.CoBorrowerCivilStatus.ToLower();
        }

        switch (currentStepIndex)
        {
            case 0:
                return principalBuyerForm.ValidatePrincipalBuyerPanel();

            case 1:
                if (principalCivilStatus == "married")
                {
                    return principalBuyerForm.spouseSection.ValidateSpousePanel();
                }
                else
                {
                    return true;
                }

            case 2:
                return principalBuyerForm.ValidateAttorneyInFactPanel();

            case 3:
                return principalBuyerForm.ValidateCharacterReferencesPanel();

           case 4: // Co-Borrower
    return coBorrowerForm != null && coBorrowerForm.ValidateCoBorrowerInfo();


            case 5:
                if (coBorrowerCivilStatus == "married")
                {
                    return principalBuyerForm.ValidateCoBorrowerSpousePanel();
                }
                else
                {
                    return true;
                }

            case 6:
                return principalBuyerForm.ValidateCoBorrowerAttorneyInFactPanel();

            case 7:
                return principalBuyerForm.ValidateCoBorrowerCharacterReferencesPanel();

            default:
                return true;
        }
    }
}