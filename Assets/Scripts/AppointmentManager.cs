using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AppointmentManager : MonoBehaviour
{
    public GameObject calendarPopup;
    public TMP_InputField dateInputField; // If you're using TMP: use TMP_InputField

    // Show the calendar popup
    public void ShowCalendar()
    {
        calendarPopup.SetActive(true);
    }

    // Called when a date is picked from the calendar
    public void OnDateSelected(string date)
    {
        dateInputField.text = date;
        calendarPopup.SetActive(false);
    }
}
