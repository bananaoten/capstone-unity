using UnityEngine;
using UnityEngine.UI;

public class CardboardLookMoveWithProgress : MonoBehaviour
{
    public float lookTime = 2f;          // How long to stare before moving
    public float moveSpeed = 2f;         // Movement speed
    public Image progressImage;          // UI loading circle
    public float thresholdAngle = 5f;    // Smaller angle threshold (e.g., 5 degrees)

    private float lookTimer = 0f;
    private bool isMoving = false;
    private Vector3 lastLookDirection;   // Track the previous direction to detect changes
    private Rigidbody rb;

    void Start()
    {
        lastLookDirection = transform.forward; // Initialize last direction to current forward

        // Get Rigidbody from parent (e.g., Player)
        rb = transform.parent.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("⚠️ Rigidbody not found on the parent GameObject! Add one for collision.");
        }
    }

    void Update()
    {
        Vector3 currentLookDirection = transform.forward;
        currentLookDirection.y = 0f;  // Prevent vertical movement
        currentLookDirection.Normalize();

        float angle = Vector3.Angle(lastLookDirection, currentLookDirection);

        if (angle > thresholdAngle)
        {
            lookTimer = 0f;
            isMoving = false;
            if (progressImage != null)
            {
                progressImage.fillAmount = 0f;
                progressImage.gameObject.SetActive(true);
            }
        }

        if (angle <= thresholdAngle)
        {
            lookTimer += Time.deltaTime;
            if (progressImage != null)
                progressImage.fillAmount = lookTimer / lookTime;

            if (lookTimer >= lookTime)
            {
                isMoving = true;
                if (progressImage != null)
                    progressImage.gameObject.SetActive(false);
            }
        }

        if (isMoving)
        {
            MoveInCurrentLookDirection(currentLookDirection);
        }

        lastLookDirection = currentLookDirection;
    }

    private void MoveInCurrentLookDirection(Vector3 direction)
    {
        if (rb != null)
        {
            Vector3 move = direction * moveSpeed * Time.deltaTime;
            rb.MovePosition(rb.position + move); // Collision-safe movement
        }
        else
        {
            // Fallback if Rigidbody not found (not recommended for VR)
            transform.parent.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
        }
    }
}
