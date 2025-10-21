using UnityEngine;

public class UIReferences : MonoBehaviour
{
    public static UIReferences Instance;
    public Transform modalContainer;
    public GameObject appointmentReschedulePrefab; // Add this line

    private void Awake()
    {
        Instance = this;
    }
}