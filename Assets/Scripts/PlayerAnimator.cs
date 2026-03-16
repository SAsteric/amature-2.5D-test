using UnityEngine;

/// <summary>
/// Drives the character's SpriteRenderer using manually sliced sprite arrays.
/// No Animator component needed — we control frame timing directly.
///
/// Sprite array layout expected (per animation clip):
///   Index = directionRow * 3 + frameIndex
///   Direction rows  →  0 = Down, 1 = Right, 2 = Left, 3 = Up   (see Direction.cs)
///   Frames per dir  →  0, 1, 2  (left-to-right in the sheet)
///
/// Idle behaviour: holds the MIDDLE frame (index 1) of the last facing direction,
/// giving the "neutral stance" pose rather than the first walk frame.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerAnimator : MonoBehaviour
{
    // ── Inspector — Sprite Arrays ──────────────────────────────────────────
    [Header("Sprite Arrays (sliced from sprite sheets)")]
    [Tooltip("All 12 walk sprites: 4 dirs x 3 frames, row-major order")]
    [SerializeField] private Sprite[] walkSprites;  // length 12

    [Tooltip("All 12 run sprites: 4 dirs x 3 frames, row-major order")]
    [SerializeField] private Sprite[] runSprites;   // length 12

    [Tooltip("All 12 jump sprites: 4 dirs x 3 frames, row-major order")]
    [SerializeField] private Sprite[] jumpSprites;  // length 12

    [Header("Animation Speed")]
    [SerializeField] private float walkFPS = 8f;
    [SerializeField] private float runFPS  = 12f;
    [SerializeField] private float jumpFPS = 6f;

    // ── Internals ──────────────────────────────────────────────────────────
    private SpriteRenderer   _sr;
    private PlayerController _ctrl;

    private AnimState _currentState = AnimState.Idle;
    private int       _currentFrame  = 1;   // start on middle frame
    private float     _frameTimer    = 0f;
    private float     _currentFPS    = 8f;

    private const int FRAMES      = 3;
    private const int IDLE_FRAME  = 1;   // middle frame = neutral stance

    // ── Unity Lifecycle ────────────────────────────────────────────────────
    private void Awake()
    {
        _sr   = GetComponent<SpriteRenderer>();
        // PlayerAnimator lives on the Sprite child; PlayerController is on the root
        _ctrl = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        AnimState nextState = DetermineState();
        if (nextState != _currentState)
        {
            _currentState = nextState;

            // When entering Idle, snap straight to the middle frame so the
            // character stands in a neutral pose instead of a walk-cycle extreme.
            // For any other state, reset to frame 0 so the clip starts cleanly.
            _currentFrame = (_currentState == AnimState.Idle) ? IDLE_FRAME : 0;
            _frameTimer   = 0f;
        }

        AdvanceFrame();
        ApplySprite();
    }

    // ── State Machine ──────────────────────────────────────────────────────
    private AnimState DetermineState()
    {
        if (!_ctrl.IsGrounded || _ctrl.IsJumping)
            return AnimState.Jump;

        if (_ctrl.IsMoving)
            return _ctrl.IsRunning ? AnimState.Run : AnimState.Walk;

        return AnimState.Idle;
    }

    // ── Frame Advance ──────────────────────────────────────────────────────
    private void AdvanceFrame()
    {
        _currentFPS = _currentState switch
        {
            AnimState.Walk => walkFPS,
            AnimState.Run  => runFPS,
            AnimState.Jump => jumpFPS,
            _              => walkFPS
        };

        // Idle: hold the middle frame — no timer needed
        if (_currentState == AnimState.Idle)
            return;

        _frameTimer += Time.deltaTime;
        float frameDuration = 1f / _currentFPS;

        if (_frameTimer >= frameDuration)
        {
            _frameTimer -= frameDuration;
            _currentFrame = (_currentFrame + 1) % FRAMES;
        }
    }

    // ── Sprite Selection ───────────────────────────────────────────────────
    private void ApplySprite()
    {
        int dirRow = (int)_ctrl.FacingDirection;  // Direction enum -> 0/1/2/3
        int index  = dirRow * FRAMES + _currentFrame;

        Sprite[] sheet = _currentState switch
        {
            AnimState.Walk => walkSprites,
            AnimState.Run  => runSprites,
            AnimState.Jump => jumpSprites,
            AnimState.Idle => walkSprites,  // idle reuses walk sheet, middle frame
            _              => walkSprites
        };

        if (sheet == null || sheet.Length == 0) return;
        index = Mathf.Clamp(index, 0, sheet.Length - 1);

        _sr.sprite = sheet[index];
    }

    // ── Enums ──────────────────────────────────────────────────────────────
    private enum AnimState { Idle, Walk, Run, Jump }
}