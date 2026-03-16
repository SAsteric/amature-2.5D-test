using UnityEngine;

/// <summary>
/// Smoothly follows the player from a fixed isometric-style angle,
/// similar to Octopath Traveller's camera perspective.
///
/// Attach to the Main Camera. Drag the Player root into the Target field.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────
    [Header("Target")]
    [Tooltip("Drag the Player root GameObject here")]
    [SerializeField] private Transform target;

    [Header("Camera Position")]
    [Tooltip("Offset from the player in world space. " +
             "Default gives a 3/4 top-down Octopath-style view.")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 7f, -5f);

    [Tooltip("How snappily the camera follows the player. Higher = snappier.")]
    [SerializeField] private float smoothSpeed = 6f;

    [Header("Look Settings")]
    [Tooltip("Check to always look at the player. " +
             "Uncheck to use a fixed look-down angle set by the camera's initial rotation.")]
    [SerializeField] private bool lookAtTarget = true;

    // ── Unity Lifecycle ────────────────────────────────────────────────────
    private void LateUpdate()
    {
        if (target == null) return;

        // Smooth-follow the target
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        if (lookAtTarget)
            transform.LookAt(target.position);
    }

#if UNITY_EDITOR
    // Draw a wire-sphere in the Scene view to show where the camera will settle
    private void OnDrawGizmosSelected()
    {
        if (target == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(target.position + offset, 0.25f);
        Gizmos.DrawLine(target.position, target.position + offset);
    }
#endif
}
