using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController))]
public class CharacterController3D : MonoBehaviour
{
    public readonly UnityEvent OnStartedMoving = new UnityEvent();
    public readonly UnityEvent OnStoppedMoving = new UnityEvent();
    public readonly UnityEvent OnJumped = new UnityEvent();
    public readonly UnityEvent OnLanded = new UnityEvent();

    public bool IsMoving { get { return Velocity.sqrMagnitude > .1f; } }
    public bool IsGrounded { get { return characterController.isGrounded; } }
    public Vector3 Velocity { get { return characterController.velocity; } }
    public CharacterController CharacterController { get { return characterController; } }

    [Header("Movement")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float jumpHeight = 15f;
    [SerializeField] private float gravity = .981f;
    [SerializeField] private bool updateForwardDirection = true;

    [Header("Input")]
    [SerializeField] private string horizontalAxisName = "Horizontal";
    [SerializeField] private string verticalAxisName = "Vertical";
    [SerializeField] private string jumpButtonName = "Jump";

    private CharacterController characterController;
    private Vector3 velocity;
    private bool wasGroundedLastFrame;
    private bool wasMovingLastFrame;

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

        if (updateForwardDirection)
            UpdateForwardDirection();
    }

    private void HandleInput ()
    {
        var inputHor = Input.GetAxis(horizontalAxisName) * movementSpeed;
        var inputVer = Input.GetAxis(verticalAxisName) * movementSpeed;
        velocity = new Vector3(inputHor, velocity.y, inputVer);

        if (IsGrounded && Input.GetButtonDown(jumpButtonName))
        {
            velocity.y = jumpHeight;
            OnJumped.Invoke();
        }
    }

    private void HandleMovement ()
    {
        if (!IsGrounded) velocity.y -= gravity;
        else velocity.y = Mathf.Max(velocity.y, -characterController.stepOffset);

        characterController.Move(velocity * Time.deltaTime);
    }

    private void DetectLanding ()
    {
        if (IsGrounded && !wasGroundedLastFrame)
            OnLanded.Invoke();
        wasGroundedLastFrame = IsGrounded;
    }

    private void DetectMovement ()
    {
        if (!wasMovingLastFrame && IsMoving)
            OnStartedMoving.Invoke();
        if (wasMovingLastFrame && !IsMoving)
            OnStoppedMoving.Invoke();
        wasMovingLastFrame = IsMoving;
    }

    private void UpdateForwardDirection ()
    {
        var forwardVector = new Vector3(velocity.x, 0, velocity.z).normalized;
        if (forwardVector != Vector3.zero)
            transform.forward = new Vector3(velocity.x, 0, velocity.z);
    }
}
