using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class Day : MonoBehaviour
{
    public enum DayMode{
        Disabled, //0
        Current,  //1
        Normal    //2
    }

    [SerializeField] private DayMode dayMode; // The current mode of the day
    [SerializeField] private GameObject currentDateIndicator; // A small circle to highlight today's date
    [SerializeField] private TMP_Text dateText; // Text component to display the date number
    public int dateNum; // The date itself
    private Color dayColor; // Color of the day button (Gray: not in current month / White: in current month)
    public int dayModeIndex; // Index to control the day mode

    public float time; // Time variable for animation or state updates
    private void Start()
    {
        time = -1; // Initialize time
    }

    // Updates the day component each frame
    void Update()
    {
        if (dateNum != 0)
        {
            dateText.text = dateNum.ToString(); // Update the date text
        }

        if(time < 0)
        {
            DayModeSet(dayModeIndex); // Set the day mode based on the index
            SetUpUI(); // Set up the UI elements
            time = 1; // Reset time
        }
    }

    // Sets up the UI elements based on the day mode
    void SetUpUI()
    {
        if (dayMode == DayMode.Disabled)
        {
            dayColor = Color.gray; // Set color to gray
            gameObject.GetComponent<Image>().color = dayColor;
            gameObject.GetComponent<Button>().interactable = false; // Disable button interaction
            currentDateIndicator.SetActive(false); // Hide the current date indicator
        }
        else if (dayMode == DayMode.Current)
        {
            dayColor = Color.white; // Set color to white
            gameObject.GetComponent<Image>().color = dayColor;
            gameObject.GetComponent<Button>().interactable = true; // Enable button interaction
            currentDateIndicator.SetActive(true); // Show the current date indicator
        }
        else
        {
            dayColor = Color.white; // Set color to white
            gameObject.GetComponent<Image>().color = dayColor;
            gameObject.GetComponent<Button>().interactable = true; // Enable button interaction
            currentDateIndicator.SetActive(false); // Hide the current date indicator
        }
    }
    
    // Sets the day mode based on the given index
    void DayModeSet(int index)
    {
        if (index == 0)
        {
            dayMode = DayMode.Disabled;
        }
        else if (index == 1)
        {
            dayMode = DayMode.Current;
        }
        else
        {
            dayMode = DayMode.Normal;
        }
    }

    // Handles the mouse down event to select the date
    private void OnMouseDown()
    {
        CalendarManager.currentDateSelected = Int32.Parse(GetComponentInChildren<TMP_Text>().text); // Set the selected date
    }
}
