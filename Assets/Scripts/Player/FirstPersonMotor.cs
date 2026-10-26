using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputReader))]
public class FirstPersonMotor : MonoBehaviour
{
    [Header("水平移动")]
    [Tooltip("行走速度（米/秒）")]
    [SerializeField] private float walkSpeed = 4.5f;
    [Tooltip("奔跑速度（米/秒）")]
    [SerializeField] private float sprintSpeed = 7.5f;
    [Tooltip("加速度，越大手感越硬")]
    [SerializeField] private float acceleration = 25f;
    [Tooltip("空中操作力度（0~1）")]
    [Range(0f, 1f)]
    [SerializeField] private float airControl = 0.35f;

    [Header("跳跃与重力")]
    [Tooltip("跳跃高度（米）")]
    [SerializeField] private float jumpHeight = 1.2f;
    [Tooltip("重力加速度，负值")]
    [SerializeField] private float gravity = -22f;
    [Tooltip("贴地时施加的下压力，避免下坡悬空")]
    [SerializeField] private float groundStickVelocity = -2f;

    private CharacterController _controller;
    private PlayerInputReader _input;
    private Vector3 _planarVelocity;
    private float _verticalVelocity;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<PlayerInputReader>();
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        bool grounded = _controller.isGrounded;

        Vector2 moveInput = Vector2.ClampMagnitude(_input.Move, 1f);
        Vector3 wishDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        float targetSpeed = _input.SprintHeld ? sprintSpeed : walkSpeed;
        float accel = grounded ? acceleration : acceleration * airControl;

        _planarVelocity = Vector3.MoveTowards(_planarVelocity, wishDirection * targetSpeed, accel * dt);

        if (grounded && _verticalVelocity < 0f)
        {
            _verticalVelocity = groundStickVelocity;
        }

        if (grounded && _input.JumpPressed)
        {
            _verticalVelocity = Mathf.Sqrt(2f * jumpHeight * -gravity);
        }

        _verticalVelocity += gravity * dt;

        Vector3 motion = _planarVelocity + Vector3.up * _verticalVelocity;
        _controller.Move(motion * dt);
    }


    public bool IsGrounded => _controller != null && _controller.isGrounded;

    public void Launch(float height, float forwardSpeed)
    {
        float safeHeight = Mathf.Max(0.05f, height);
        _verticalVelocity = Mathf.Sqrt(2f * safeHeight * -gravity);

        if (forwardSpeed > 0f)
        {
            Vector3 forward = transform.forward;
            _planarVelocity = new Vector3(forward.x * forwardSpeed, 0f, forward.z * forwardSpeed);
        }
    }

    public void Teleport(Vector3 position)
    {
        _planarVelocity = Vector3.zero;
        _verticalVelocity = 0f;
        _controller.enabled = false;
        transform.position = position;
        _controller.enabled = true;
    }
}
