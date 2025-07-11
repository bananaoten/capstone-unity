using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CalendarManager : MonoBehaviour
{
    [SerializeField] TMP_Text MonthAndYear; // Text component to display the month and year

    [SerializeField] private GameObject[] days; // Array of day GameObjects in the calendar grid

    private int showYear; // The year currently being displayed
    private int showMonth; // The month currently being displayed
    
    [SerializeField] Button left; // Button to navigate to the previous month
    [SerializeField] Button right; // Button to navigate to the next month

    public static int currentDateSelected; // Static variable to store the currently selected date

    // Adds listeners to the left and right buttons when the script is enabled
    private void OnEnable()
    {
        left.onClick.AddListener(Left);
        right.onClick.AddListener(Right);
    }

    // Removes listeners from the left and right buttons when the script is disabled
    private void OnDisable()
    {
        left.onClick.RemoveListener(Left);
        right.onClick.RemoveListener(Right);
    }

    // Initializes the calendar with the current date when the script starts
    private void Start()
    {
        UpdateCalender(DateTime.Now.Year, DateTime.Now.Month);
    }

    // Updates the calendar to display the specified year and month
    void UpdateCalender(int year, int month)
    {
        showYear = year;
        showMonth = month;
        
        DateTime temp = new DateTime(year, month, 1); // Create a DateTime object for the first day of the month
        MonthAndYear.text = temp.ToString("MMMM") + " " + temp.ToString("yyyy"); // Update the month and year text

        int startDay = GetMonthStartDay(temp.Year, temp.Month); // Get the day of the week for the first day of the month
        int endDay = GetTotalNumberOfDays(temp.Year, temp.Month); // Get the total number of days in the month
        int previousEndDate;

        // Loop through the calendar grid
        for (int w = 0; w < 6; w++) // 6 rows (weeks)
        {
            for (int i = 0; i < 7; i++) // 7 columns (days)
            {
                int currentField = (w * 7) + i; // Calculate the current field index in the grid

                // If the current field is before the start of the month or after the end of the month
                if (currentField < startDay || currentField - startDay >= endDay)
                {
                    days[currentField].GetComponent<Day>().dayModeIndex = 0; // Set day mode to Disabled
                    days[currentField].GetComponent<Day>().time = -1; // Reset time
                }
                else
                {
                    days[currentField].GetComponent<Day>().dayModeIndex = 2; // Set day mode to Normal
                    days[currentField].GetComponent<Day>().time = -1; // Reset time
                }

                // If the current field is within the current month
                if (currentField >= startDay && currentField - startDay < endDay)
                {
                    days[currentField].GetComponent<Day>().dateNum = (currentField - startDay) + 1; // Set the date number
                }
                // If the current field is before the start of the month
                else if (currentField < startDay)
                {
                    if (temp.Month != 1)
                    {
                        previousEndDate = GetTotalNumberOfDays(temp.Year, temp.Month - 1); // Get the total number of days in the previous month
                    }
                    else
                    {
                        previousEndDate = 31; // December of the previous year
                    }

                    int sub = startDay - currentField; // Calculate the offset

                    // If the offset is valid
                    if (sub > 0 && sub < 7)
                    {
                        days[currentField].GetComponent<Day>().dateNum = previousEndDate - (sub - 1); // Set the date number for the previous month
                    }
                }
                // If the current field is after the end of the month
                else if (currentField - startDay >= endDay)
                {
                    int sub = (currentField - startDay) - endDay + 1; // Calculate the offset

                    // If the offset is valid
                    if (sub > 0 && sub < 15)
                    {
                        days[currentField].GetComponent<Day>().dateNum = sub; // Set the date number for the next month
                    }
                }
            }
        }
        
        // Highlight the current day if the current month and year are being displayed
        if(DateTime.Now.Year == year && DateTime.Now.Month == month)
        {
            days[(DateTime.Now.Day - 1) + startDay].GetComponent<Day>().dayModeIndex = 1; // Set day mode to Current
            days[(DateTime.Now.Day - 1) + startDay].GetComponent<Day>().time = -1; // Reset time
        }
    }
    
    // Gets the day of the week for the first day of the specified month and year
    int GetMonthStartDay(int year, int month)
    {
        DateTime temp = new DateTime(year, month, 1);
        return (int)temp.DayOfWeek;
    }
    
    // Gets the total number of days in the specified month and year
    int GetTotalNumberOfDays(int year, int month)
    {
        return DateTime.DaysInMonth(year, month);
    }

    // Updates the calendar to the previous month
    void Left()
    {
        if (showMonth != 1)
        {
            UpdateCalender(showYear, showMonth - 1);
        }
        else
        {
            UpdateCalender(showYear - 1, 12); // Wrap around to December of the previous year
        }
    }

    // Updates the calendar to the next month
    void Right()
    {
        if (showMonth != 12)
        {
            UpdateCalender(showYear, showMonth + 1);
        }
        else
        {
            UpdateCalender(showYear + 1, 1); // Wrap around to January of the next year
        }
    }
    
}
