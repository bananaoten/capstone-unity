using UnityEngine;
using UnityEngine.UI; // Use TMPro if you're using TMP_InputField
using TMPro;


public class CalendarDatePicker : MonoBehaviour
{
    public GameObject calendarPanel;         // Assign your calendar panel
public TMP_InputField targetInputField;

    // This method will be called when a date is picked
    public void OnDateSelected(string date)
    {
        targetInputField.text = date;       // Display the selected date
        calendarPanel.SetActive(false);     // Hide the calendar after picking
    }

    public void ShowCalendar()
    {
        calendarPanel.SetActive(true);
    }
}
