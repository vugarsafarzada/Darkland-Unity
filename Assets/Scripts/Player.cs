using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, PlayerInputActions.IPlayerActions
{
    private PlayerInputActions _input;
    private Animator _animator;
    private CharacterController _controller;
    private Vector2 _moveInput;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float terminalVelocity = -50f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private LayerMask groundMask;

    private Camera _camera;
    private float _verticalVelocity;
    private bool _isGrounded;
    private bool _wasGrounded;

    void Awake()
    {
        _input = new PlayerInputActions();
        _input.Player.SetCallbacks(this);
        _animator = GetComponentInChildren<Animator>();
        _controller = GetComponent<CharacterController>();
        _camera = Camera.main;
    }

    void OnEnable() => _input.Player.Enable();
    void OnDisable() => _input.Player.Disable();

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && _isGrounded)
        {
            _verticalVelocity = jumpForce;
            _animator.SetTrigger("jump");
        }
    }

    void Update()
    {
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
        if (_isGrounded && !_wasGrounded)
            _animator.SetTrigger("landed");
        _wasGrounded = _isGrounded;

        ApplyGravity();
        Vector3 moveDirection = Vector3.zero;
        if (_moveInput != Vector2.zero)
        {
            Vector3 camForward = _camera.transform.forward;
            Vector3 camRight = _camera.transform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            moveDirection = camForward * _moveInput.y + camRight * _moveInput.x;

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(moveDirection),
                rotationSpeed * Time.deltaTime
            );
        }

        _controller.Move((moveDirection * moveSpeed + Vector3.up * _verticalVelocity) * Time.deltaTime);
        _animator.SetFloat("Blend", _moveInput.magnitude);
    }

    void ApplyGravity()
    {
        if (_isGrounded)
        {
            if (_verticalVelocity < -2f)
                _verticalVelocity = -2f;
        }
        else
        {
            _verticalVelocity += gravity * Time.deltaTime;
            if (_verticalVelocity < terminalVelocity)
                _verticalVelocity = terminalVelocity;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
