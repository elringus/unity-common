using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController))]
public class CharacterController3D : MonoBehaviour
{
    public event UnityAction OnStartedMoving;
    public event UnityAction OnStoppedMoving;
    public event UnityAction OnJumped;
    public event UnityAction OnLanded;

    public bool IsInputBlocked { get { return isInputBlocked; } set { isInputBlocked = value; } }
    public bool IsMoving { get { return Velocity.magnitude > 0; } }
    public bool IsSprinting { get { return isSprinting; } }
    public bool IsGrounded { get { return characterController.isGrounded; } }
    public bool IsMoveInputActive { get { return moveInputVelocity.magnitude > 0; } }
    public float TimeSinceLastMoveInputStart { get { return Time.time - lastMoveInputStartTime; } }
    public float TimeSinceLastMoveInputEnd { get { return Time.time - lastMoveInputEndTime; } }
    public Vector3 Velocity { get { return characterController.velocity; } }
    public CharacterController CharacterController { get { return characterController; } }

    [Header("Input")]
    [SerializeField] private string horizontalAxisName = "Horizontal";
    [SerializeField] private string verticalAxisName = "Vertical";
    [SerializeField] private string jumpButtonName = "Jump";
    [SerializeField] private string sprintButtonName = null;

    [Header("Movement")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private AnimationCurve accelerationCurve = AnimationCurve.Linear(0, 1, 1, 1);
    [SerializeField] private float jumpHeight = 15f;
    [SerializeField] private float sprintModifier = 2f;
    [SerializeField] private float gravity = .981f;

    [Header("Behaviour")]
    [SerializeField] private bool updateForwardDirection = true;

    private CharacterController characterController;
    private Vector3 moveVelocity;
    private Vector2 moveInputVelocity;
    private float lastMoveInputStartTime;
    private float lastMoveInputEndTime;
    private bool isSprinting;
    private bool wasGroundedLastFrame;
    private bool wasMovingLastFrame;
    private bool wasMoveInputActiveLastFrame;
    private bool isInputBlocked;

    private void Awake ()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update ()
    {
        HandleInput();
        HandleMovement();
        DetectLanding();
        DetectMovement();
        DetectMoveInput();

        if (updateForwardDirection)
            UpdateForwardDirection();
    }

    public bool Jump ()
    {
        if (!IsGrounded) return false;

        moveVelocity.y = jumpHeight;
        OnJumped.SafeInvoke();

        return true;
    }

    public void StartSprint ()
    {
        isSprinting = true;
    }

    public void EndSprint ()
    {
        isSprinting = false;
    }

    private void HandleInput ()
    {
        if (IsInputBlocked)
        {
            moveInputVelocity = Vector2.zero;
            return;
        }

        var inputHor = !string.IsNullOrEmpty(horizontalAxisName) ? Input.GetAxis(horizontalAxisName) : 0;
        var inputVer = !string.IsNullOrEmpty(verticalAxisName) ? Input.GetAxis(verticalAxisName) : 0;
        moveInputVelocity = new Vector2(inputHor, inputVer);
        if (moveInputVelocity.magnitude > 1) moveInputVelocity.Normalize();

        if (!string.IsNullOrEmpty(jumpButtonName) && Input.GetButtonDown(jumpButtonName)) Jump();

        if (!string.IsNullOrEmpty(sprintButtonName))
        {
            if (!IsSprinting && Input.GetButtonDown(sprintButtonName)) StartSprint();
            if (IsSprinting && Input.GetButtonUp(sprintButtonName)) EndSprint();
        }
    }

    private void HandleMovement ()
    {
        if (!IsGrounded) moveVelocity.y -= gravity;
        else moveVelocity.y = Mathf.Max(moveVelocity.y, -characterController.stepOffset);

        var velocityModifier = movementSpeed * (IsSprinting ? sprintModifier : accelerationCurve.Evaluate(TimeSinceLastMoveInputStart));
        moveVelocity = new Vector3(moveInputVelocity.x * velocityModifier, moveVelocity.y, moveInputVelocity.y * velocityModifier);

        characterController.Move(moveVelocity * Time.deltaTime);
    }

    private void DetectLanding ()
    {
        if (IsGrounded && !wasGroundedLastFrame)
            OnLanded.SafeInvoke();
        wasGroundedLastFrame = IsGrounded;
    }

    private void DetectMovement ()
    {
        if (!wasMovingLastFrame && IsMoving)
            OnStartedMoving.SafeInvoke();
        if (wasMovingLastFrame && !IsMoving)
            OnStoppedMoving.SafeInvoke();
        wasMovingLastFrame = IsMoving;
    }

    private void DetectMoveInput ()
    {
        if (!wasMoveInputActiveLastFrame && IsMoveInputActive)
            lastMoveInputStartTime = Time.time;
        if (wasMoveInputActiveLastFrame && !IsMoveInputActive)
            lastMoveInputEndTime = Time.time;
        wasMoveInputActiveLastFrame = IsMoveInputActive;
    }

    private void UpdateForwardDirection ()
    {
        var forwardVector = new Vector3(moveVelocity.x, 0, moveVelocity.z).normalized;
        if (forwardVector != Vector3.zero)
            transform.forward = new Vector3(moveVelocity.x, 0, moveVelocity.z);
    }
}
