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

        // Only validate on the last step
        if (!isLastStep || !ValidateCurrentStep())
        {
            ShowSubmissionStatus("Please complete all required fields before submitting.", false);
            return;
        }

        submitFormButton.interactable = false;

        // --- Collect Principal Buyer Data ---
        var principalData = new Dictionary<string, object>
        {
            { "lastName", principalBuyerForm.lastName.text },
            { "firstName", principalBuyerForm.firstName.text },
            { "middleName", principalBuyerForm.middleName.text },
            { "birthday", principalBuyerForm.birthday.text },
            { "age", principalBuyerForm.age.text },
            { "placeOfBirth", principalBuyerForm.placeOfBirth.text },
            { "gender", principalBuyerForm.genderDropdown.options[principalBuyerForm.genderDropdown.value].text },
            { "civilStatus", principalBuyerForm.PrincipalBuyerCivilStatus },
            { "citizenship", principalBuyerForm.citizenship.text },
            { "religion", principalBuyerForm.religion.text },
            { "contactNumber", principalBuyerForm.contactNumber.text },
            { "facebookAccount", principalBuyerForm.facebookAccount.text },
            { "presentAddress", principalBuyerForm.presentAddress.text },
            { "permanentAddress", principalBuyerForm.permanentAddress.text },
            { "employerName", principalBuyerForm.employerName.text },
            { "businessNature", principalBuyerForm.businessNature.text },
            { "positionAndDepartment", principalBuyerForm.positionAndDepartment.text },
            { "employerAddress", principalBuyerForm.employerAddress.text },
            { "officeTelephone", principalBuyerForm.officeTelephone.text },
            { "emailAddress", principalBuyerForm.emailAddress.text },
            { "contactPerson", principalBuyerForm.contactPerson.text },
            { "tinId", principalBuyerForm.tinId.text },
            { "pagibigNo", principalBuyerForm.pagibigNo.text },
            { "sourceOfIncome", principalBuyerForm.sourceOfIncomeDropdown.options[principalBuyerForm.sourceOfIncomeDropdown.value].text }
        };

        // --- Principal Buyer Spouse Data (if married) ---
       Dictionary<string, object> spouseData = null;
if (principalBuyerForm.PrincipalBuyerCivilStatus.ToLower() == "married" && principalBuyerForm.spouseSection != null)
{
    spouseData = new Dictionary<string, object>
    {
        { "lastName", principalBuyerForm.spouseSection.lastName.text },
        { "firstName", principalBuyerForm.spouseSection.firstName.text },
        { "middleName", principalBuyerForm.spouseSection.middleName.text },
        { "birthday", principalBuyerForm.spouseSection.birthday.text },
        { "age", principalBuyerForm.spouseSection.age.text },
        { "placeOfBirth", principalBuyerForm.spouseSection.placeOfBirth.text },
        { "gender", principalBuyerForm.spouseSection.genderDropdown.options[principalBuyerForm.spouseSection.genderDropdown.value].text },
        // REMOVED civilStatusDropdown
        { "citizenship", principalBuyerForm.spouseSection.citizenship.text },
        { "religion", principalBuyerForm.spouseSection.religion.text },
        { "contactNumber", principalBuyerForm.spouseSection.contactNumber.text },
        { "facebookAccount", principalBuyerForm.spouseSection.facebookAccount.text },
        { "employerName", principalBuyerForm.spouseSection.employerName.text },
        { "businessNature", principalBuyerForm.spouseSection.businessNature.text },
        { "positionAndDepartment", principalBuyerForm.spouseSection.positionAndDepartment.text },
        { "employerAddress", principalBuyerForm.spouseSection.employerAddress.text },
        { "officeTelephone", principalBuyerForm.spouseSection.officeTelephone.text },
        { "emailAddress", principalBuyerForm.spouseSection.emailAddress.text },
        { "contactPerson", principalBuyerForm.spouseSection.contactPerson.text },
        { "tinId", principalBuyerForm.spouseSection.tinId.text },
        { "pagibigNo", principalBuyerForm.spouseSection.pagibigNo.text },
        { "sourceOfIncome", principalBuyerForm.spouseSection.sourceOfIncomeDropdown.options[principalBuyerForm.spouseSection.sourceOfIncomeDropdown.value].text }
    };
}


        // --- Principal Buyer Attorney-in-Fact Data ---
        Dictionary<string, object> attorneyData = null;
        if (principalBuyerForm.attorneyInFactSection != null)
        {
            attorneyData = new Dictionary<string, object>
            {
                { "lastName", principalBuyerForm.attorneyInFactSection.lastName.text },
                { "firstName", principalBuyerForm.attorneyInFactSection.firstName.text },
                { "middleName", principalBuyerForm.attorneyInFactSection.middleName.text },
                { "gender", principalBuyerForm.attorneyInFactSection.genderDropdown.options[principalBuyerForm.attorneyInFactSection.genderDropdown.value].text },
                { "contactNumberOrEmail", principalBuyerForm.attorneyInFactSection.contactNumberOrEmail.text },
                { "address", principalBuyerForm.attorneyInFactSection.address.text }
            };
        }

        // --- Principal Buyer Character References Data ---
        Dictionary<string, object> characterReferencesData = null;
        if (principalBuyerForm.characterReferenceSection != null)
        {
            characterReferencesData = new Dictionary<string, object>
            {
                { "reference1", new Dictionary<string, object>
                    {
                        { "lastName", principalBuyerForm.characterReferenceSection.lastName1.text },
                        { "firstName", principalBuyerForm.characterReferenceSection.firstName1.text },
                        { "middleName", principalBuyerForm.characterReferenceSection.middleName1.text },
                        { "contactNumber", principalBuyerForm.characterReferenceSection.contactNumber1.text },
                        { "facebookAccount", principalBuyerForm.characterReferenceSection.facebookAccount1.text },
                        { "address", principalBuyerForm.characterReferenceSection.address1.text }
                    }
                },
                { "reference2", new Dictionary<string, object>
                    {
                        { "lastName", principalBuyerForm.characterReferenceSection.lastName2.text },
                        { "firstName", principalBuyerForm.characterReferenceSection.firstName2.text },
                        { "middleName", principalBuyerForm.characterReferenceSection.middleName2.text },
                        { "contactNumber", principalBuyerForm.characterReferenceSection.contactNumber2.text },
                        { "facebookAccount", principalBuyerForm.characterReferenceSection.facebookAccount2.text },
                        { "address", principalBuyerForm.characterReferenceSection.address2.text }
                    }
                },
                { "reference3", new Dictionary<string, object>
                    {
                        { "lastName", principalBuyerForm.characterReferenceSection.lastName3.text },
                        { "firstName", principalBuyerForm.characterReferenceSection.firstName3.text },
                        { "middleName", principalBuyerForm.characterReferenceSection.middleName3.text },
                        { "contactNumber", principalBuyerForm.characterReferenceSection.contactNumber3.text },
                        { "facebookAccount", principalBuyerForm.characterReferenceSection.facebookAccount3.text },
                        { "address", principalBuyerForm.characterReferenceSection.address3.text }
                    }
                }
            };
        }

        // --- Co-Borrower Data ---
        Dictionary<string, object> coBorrowerData = null;
        if (coBorrowerForm != null)
        {
            coBorrowerData = new Dictionary<string, object>
            {
                { "lastName", coBorrowerForm.lastName.text },
                { "firstName", coBorrowerForm.firstName.text },
                { "middleName", coBorrowerForm.middleName.text },
                { "birthday", coBorrowerForm.birthday.text },
                { "age", coBorrowerForm.age.text },
                { "placeOfBirth", coBorrowerForm.placeOfBirth.text },
                { "gender", coBorrowerForm.genderDropdown.options[coBorrowerForm.genderDropdown.value].text },
                { "civilStatus", coBorrowerForm.CoBorrowerCivilStatus },
                { "citizenship", coBorrowerForm.citizenship.text },
                { "religion", coBorrowerForm.religion.text },
                { "contactNumber", coBorrowerForm.contactNumber.text },
                { "facebookAccount", coBorrowerForm.facebookAccount.text },
                { "presentAddress", coBorrowerForm.presentAddress.text },
                { "permanentAddress", coBorrowerForm.permanentAddress.text },
                { "employerName", coBorrowerForm.employerName.text },
                { "businessNature", coBorrowerForm.businessNature.text },
                { "positionAndDepartment", coBorrowerForm.positionAndDepartment.text },
                { "employerAddress", coBorrowerForm.employerAddress.text },
                { "officeTelephone", coBorrowerForm.officeTelephone.text },
                { "emailAddress", coBorrowerForm.emailAddress.text },
                { "contactPerson", coBorrowerForm.contactPerson.text },
                { "tinId", coBorrowerForm.tinId.text },
                { "pagibigNo", coBorrowerForm.pagibigNo.text },
                { "sourceOfIncome", coBorrowerForm.sourceOfIncomeDropdown.options[coBorrowerForm.sourceOfIncomeDropdown.value].text }
            };

            // --- Co-Borrower Spouse Data (if married) ---
           if (coBorrowerForm.CoBorrowerCivilStatus.ToLower() == "married" && coBorrowerForm.spouseSection != null)
{
    coBorrowerData.Add("spouse", new Dictionary<string, object>
    {
        { "lastName", coBorrowerForm.spouseSection.lastName.text },
        { "firstName", coBorrowerForm.spouseSection.firstName.text },
        { "middleName", coBorrowerForm.spouseSection.middleName.text },
        { "birthday", coBorrowerForm.spouseSection.birthday.text },
        { "age", coBorrowerForm.spouseSection.age.text },
        { "placeOfBirth", coBorrowerForm.spouseSection.placeOfBirth.text },
        { "gender", coBorrowerForm.spouseSection.genderDropdown.options[coBorrowerForm.spouseSection.genderDropdown.value].text },
        // REMOVED civilStatusDropdown
        { "citizenship", coBorrowerForm.spouseSection.citizenship.text },
        { "religion", coBorrowerForm.spouseSection.religion.text },
        { "contactNumber", coBorrowerForm.spouseSection.contactNumber.text },
        { "facebookAccount", coBorrowerForm.spouseSection.facebookAccount.text },
        { "employerName", coBorrowerForm.spouseSection.employerName.text },
        { "businessNature", coBorrowerForm.spouseSection.businessNature.text },
        { "positionAndDepartment", coBorrowerForm.spouseSection.positionAndDepartment.text },
        { "employerAddress", coBorrowerForm.spouseSection.employerAddress.text },
        { "officeTelephone", coBorrowerForm.spouseSection.officeTelephone.text },
        { "emailAddress", coBorrowerForm.spouseSection.emailAddress.text },
        { "contactPerson", coBorrowerForm.spouseSection.contactPerson.text },
        { "tinId", coBorrowerForm.spouseSection.tinId.text },
        { "pagibigNo", coBorrowerForm.spouseSection.pagibigNo.text },
        { "sourceOfIncome", coBorrowerForm.spouseSection.sourceOfIncomeDropdown.options[coBorrowerForm.spouseSection.sourceOfIncomeDropdown.value].text }
    });
}

            // --- Co-Borrower Attorney-in-Fact Data ---
            if (coBorrowerForm.attorneyInFactSection != null)
            {
                coBorrowerData.Add("attorneyInFact", new Dictionary<string, object>
                {
                    { "lastName", coBorrowerForm.attorneyInFactSection.lastName.text },
                    { "firstName", coBorrowerForm.attorneyInFactSection.firstName.text },
                    { "middleName", coBorrowerForm.attorneyInFactSection.middleName.text },
                    { "gender", coBorrowerForm.attorneyInFactSection.genderDropdown.options[coBorrowerForm.attorneyInFactSection.genderDropdown.value].text },
                    { "contactNumberOrEmail", coBorrowerForm.attorneyInFactSection.contactNumberOrEmail.text },
                    { "address", coBorrowerForm.attorneyInFactSection.address.text }
                });
            }

            // --- Co-Borrower Character References Data ---
            if (coBorrowerForm.characterReferenceSection != null)
            {
                coBorrowerData.Add("characterReferences", new Dictionary<string, object>
                {
                    { "reference1", new Dictionary<string, object>
                        {
                            { "lastName", coBorrowerForm.characterReferenceSection.lastName1.text },
                            { "firstName", coBorrowerForm.characterReferenceSection.firstName1.text },
                            { "middleName", coBorrowerForm.characterReferenceSection.middleName1.text },
                            { "contactNumber", coBorrowerForm.characterReferenceSection.contactNumber1.text },
                            { "facebookAccount", coBorrowerForm.characterReferenceSection.facebookAccount1.text },
                            { "address", coBorrowerForm.characterReferenceSection.address1.text }
                        }
                    },
                    { "reference2", new Dictionary<string, object>
                        {
                            { "lastName", coBorrowerForm.characterReferenceSection.lastName2.text },
                            { "firstName", coBorrowerForm.characterReferenceSection.firstName2.text },
                            { "middleName", coBorrowerForm.characterReferenceSection.middleName2.text },
                            { "contactNumber", coBorrowerForm.characterReferenceSection.contactNumber2.text },
                            { "facebookAccount", coBorrowerForm.characterReferenceSection.facebookAccount2.text },
                            { "address", coBorrowerForm.characterReferenceSection.address2.text }
                        }
                    },
                    { "reference3", new Dictionary<string, object>
                        {
                            { "lastName", coBorrowerForm.characterReferenceSection.lastName3.text },
                            { "firstName", coBorrowerForm.characterReferenceSection.firstName3.text },
                            { "middleName", coBorrowerForm.characterReferenceSection.middleName3.text },
                            { "contactNumber", coBorrowerForm.characterReferenceSection.contactNumber3.text },
                            { "facebookAccount", coBorrowerForm.characterReferenceSection.facebookAccount3.text },
                            { "address", coBorrowerForm.characterReferenceSection.address3.text }
                        }
                    }
                });
            }
        }

        // --- Combine all data ---
        var allFormData = new Dictionary<string, object>
        {
            { "principalBuyer", principalData }
        };

        if (spouseData != null)
            allFormData.Add("principalBuyerSpouse", spouseData);
        if (attorneyData != null)
            allFormData.Add("principalBuyerAttorneyInFact", attorneyData);
        if (characterReferencesData != null)
            allFormData.Add("principalBuyerCharacterReferences", characterReferencesData);
        if (coBorrowerData != null)
            allFormData.Add("coBorrower", coBorrowerData);

        // --- Submit to Firebase ---
        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        dbRef.Child("reservation-data").Push().SetValueAsync(allFormData).ContinueWith(task =>
        {
            Task.Factory.StartNew(() =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Debug.Log("✅ Form data submitted to Firebase!");
                    ShowSubmissionStatus("Form submitted successfully!", true);

                    // Change the static text to "YOUR FORM HAS BEEN SUCCESSFULLY SUBMITTED"
                    if (submitStatusText != null)
                    {
                        submitStatusText.text = "YOUR FORM HAS BEEN SUCCESSFULLY SUBMITTED";
                        submitStatusText.gameObject.SetActive(true);
                    }
                }
                else
                {
                    Debug.LogError("❌ Failed to submit form: " + task.Exception);
                    ShowSubmissionStatus("Failed to submit the form. Please try again.", false);
                    submitFormButton.interactable = true;
                }
            }, System.Threading.CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
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

            case 4:
                return principalBuyerForm.ValidateCoBorrowerPrincipalInfoPanel();

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