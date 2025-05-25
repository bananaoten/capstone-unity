using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityController : MonoBehaviour
{
    public float gravityForce = 9.81f;
    public float groundCheckDistance = 0.2f;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer; // Set this in the Inspector to define what counts as ground

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // We'll handle gravity ourselves
    }

    void FixedUpdate()
    {
        if (!IsGrounded())
        {
            // Apply manual gravity only when not grounded
            rb.AddForce(Vector3.down * gravityForce, ForceMode.Acceleration);
        }
    }

    bool IsGrounded()
    {
        // Cast a sphere downward to check for ground, helps on slopes and stairs
        Vector3 origin = transform.position + Vector3.up * 0.1f; // Slightly above bottom to avoid clipping
        return Physics.SphereCast(origin, groundCheckRadius, Vector3.down, out _, groundCheckDistance, groundLayer);
    }

    void OnDrawGizmosSelected()
    {
        // Visualize the SphereCast for debugging in the editor
        Gizmos.color = Color.red;
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        Gizmos.DrawWireSphere(origin + Vector3.down * groundCheckDistance, groundCheckRadius);
    }
}
