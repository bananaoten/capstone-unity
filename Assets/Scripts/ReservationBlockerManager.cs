using UnityEngine;
using Firebase.Database;
using Firebase.Auth;

public class ReservationBlockerManager : MonoBehaviour
{
    [Header("Reservation Blocker")]
    [SerializeField] private GameObject reservationBlocker; // assign in Inspector

    private DatabaseReference dbRef;
    private string userId;

    void Start()
    {
        userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        ListenForInquiries();
    }

    private void ListenForInquiries()
    {
        // ✅ Listen to ALL inquiries for this user
        dbRef.Child("inquiries").Child(userId).ValueChanged += HandleInquiriesChanged;
    }

    private void HandleInquiriesChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null) return;

        bool hasAccepted = false;

        foreach (var inquirySnapshot in args.Snapshot.Children)
        {
            string status = inquirySnapshot.Child("status").Value?.ToString() ?? "pending";

            if (status == "accepted")
            {
                hasAccepted = true;
                break; // we only need one accepted
            }
        }

        // ✅ If at least one accepted → hide blocker
        reservationBlocker.SetActive(!hasAccepted);
    }
}
