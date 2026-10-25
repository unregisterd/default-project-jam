using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerLook : MonoBehaviour
{
    [SerializeField] private PlayerInputReader input;
    [Tooltip("上下俯仰的相机节点")]
    [SerializeField] private Transform cameraPivot;
    [Tooltip("鼠标灵敏度（度/像素）")]
    [SerializeField] private float sensitivity = 0.12f;
    [SerializeField] private float minPitch = -85f;
    [SerializeField] private float maxPitch = 85f;

    [Header("鼠标锁定")]
    [SerializeField] private bool lockCursorOnStart = true;
    [Tooltip("按 Esc 释放鼠标；再按一次重新锁定")]
    [SerializeField] private bool allowEscapeRelease = true;

    private float _pitch;
    private bool _cursorLocked;

    private void Awake()
    {
        if (input == null)
        {
            input = GetComponent<PlayerInputReader>();
        }
    }

    private void Start()
    {
        if (lockCursorOnStart)
        {
            SetCursorLocked(true);
        }
    }

    private void Update()
    {
        HandleCursorToggle();

        if (cameraPivot == null || input == null)
        {
            return;
        }

        if (Cursor.lockState != CursorLockMode.Locked)
        {
            return;
        }

        Vector2 look = input.Look * sensitivity;

        transform.Rotate(0f, look.x, 0f, Space.Self);

        _pitch = Mathf.Clamp(_pitch - look.y, minPitch, maxPitch);
        cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    private void HandleCursorToggle()
    {
        if (!allowEscapeRelease || Keyboard.current == null)
        {
            return;
        }

        if (!Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            return;
        }

        SetCursorLocked(!_cursorLocked);
    }

    public void SetCursorLocked(bool locked)
    {
        _cursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
