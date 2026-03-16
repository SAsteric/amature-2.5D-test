using UnityEngine;

/// <summary>
/// Moves the player on the XZ plane using a CharacterController.
/// Exposes locomotion state for PlayerAnimator to read.
///
/// Controls:
///   WASD / Arrow Keys  →  Move
///   Left Shift (hold)  →  Run
///   Space              →  Jump (works while moving)
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────────────────
    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed  = 6f;

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravity   = -20f;

    // ── Public State (read by PlayerAnimator) ─────────────────────────────
    public bool      IsGrounded      => _cc.isGrounded;
    public bool      IsJumping       { get; private set; }
    public bool      IsMoving        { get; private set; }
    public bool      IsRunning       { get; private set; }
    public Direction FacingDirection { get; private set; } = Direction.Down;

    // ── Internals ─────────────────────────────────────────────────────────
    private CharacterController _cc;
    private float               _velocityY = 0f;

    // ── Unity Lifecycle ────────────────────────────────────────────────────
    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleMovement();
    }

    // ── Movement ───────────────────────────────────────────────────────────
    private void HandleMovement()
    {
        // Raw input — no smoothing so direction snaps cleanly for sprite flipping
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = new Vector3(h, 0f, v);

        IsMoving  = moveDir.sqrMagnitude > 0.01f;
        IsRunning = IsMoving && Input.GetKey(KeyCode.LeftShift);

        if (IsMoving)
        {
            moveDir.Normalize();
            UpdateFacingDirection(h, v);
        }

        // ── Gravity & Jump ─────────────────────────────────────────────────
        // IMPORTANT: isGrounded is read BEFORE Move(), then everything is
        // folded into ONE Move() at the bottom.
        //
        // Previously there were two separate _cc.Move() calls — one for XZ,
        // one for Y. The first call nudged the player off the surface, so
        // isGrounded was already false by the time the jump check ran.
        // That caused the "floating / jump animation always playing" bug
        // and "can only jump while standing still" bug simultaneously.

        if (_cc.isGrounded)
        {
            IsJumping = false;

            if (_velocityY < 0f)
                _velocityY = -2f;   // small negative keeps isGrounded reliable next frame

            if (Input.GetButtonDown("Jump"))  // Space by default
            {
                _velocityY = jumpForce;
                IsJumping  = true;
            }
        }
        else
        {
            IsJumping = true;   // in the air until we land
        }

        _velocityY += gravity * Time.deltaTime;

        // ── Single Move() call ─────────────────────────────────────────────
        // XZ and Y combined — CharacterController runs collision/grounded
        // resolution only once per frame, giving reliable isGrounded next tick.
        float   speed         = IsRunning ? runSpeed : walkSpeed;
        Vector3 horizontal    = IsMoving ? moveDir * speed : Vector3.zero;
        Vector3 finalVelocity = new Vector3(horizontal.x, _velocityY, horizontal.z);
        _cc.Move(finalVelocity * Time.deltaTime);
    }

    // ── Facing Direction ───────────────────────────────────────────────────
    /// <summary>
    /// Determines which sprite row to use based on dominant input axis.
    /// Horizontal input takes priority when h and v are equal (diagonal).
    /// </summary>
    private void UpdateFacingDirection(float h, float v)
    {
        if (Mathf.Abs(h) >= Mathf.Abs(v))
        {
            FacingDirection = h > 0f ? Direction.Right : Direction.Left;
        }
        else
        {
            FacingDirection = v > 0f ? Direction.Up : Direction.Down;
        }
    }
}