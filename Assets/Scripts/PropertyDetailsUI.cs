using UnityEngine;

public class PropertyDetailsUI : MonoBehaviour
{
    public GameObject appointmentPanel;

    public void ShowAppointmentPanel()
    {
        appointmentPanel.SetActive(true);
    }
}
