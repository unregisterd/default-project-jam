using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerInputReader : MonoBehaviour
{
    [Tooltip("Assets/Settings/PlayerControls.inputactions")]
    [SerializeField] private InputActionAsset actions;
    [SerializeField] private string actionMapName = "Player";

    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool RaisePressed { get; private set; }
    public bool IsReady => _ready;

    private InputActionMap _map;
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _jumpAction;
    private InputAction _sprintAction;
    private InputAction _raiseAction;
    private bool _ready;

    private void Awake()
    {
        if (actions == null)
        {
            Debug.LogError("[PlayerInputReader] 没有指定 InputActionAsset，玩家将无法操作。", this);
            return;
        }

        _map = actions.FindActionMap(actionMapName, false);
        if (_map == null)
        {
            Debug.LogError($"[PlayerInputReader] 找不到 Action Map \"{actionMapName}\"。", this);
            return;
        }

        _moveAction = _map.FindAction("Move", false);
        _lookAction = _map.FindAction("Look", false);
        _jumpAction = _map.FindAction("Jump", false);
        _sprintAction = _map.FindAction("Sprint", false);
        _raiseAction = _map.FindAction("Raise", false);

        if (_moveAction == null || _lookAction == null)
        {
            Debug.LogError("[PlayerInputReader] Action Map 里缺少 Move 或 Look。", this);
            return;
        }

        _ready = true;
    }

    private void OnEnable()
    {
        if (_ready)
        {
            _map.Enable();
        }
    }

    private void OnDisable()
    {
        if (_ready)
        {
            _map.Disable();
        }
    }

    private void Update()
    {
        if (!_ready)
        {
            return;
        }

        Move = _moveAction.ReadValue<Vector2>();
        Look = _lookAction.ReadValue<Vector2>();
        JumpPressed = _jumpAction != null && _jumpAction.WasPressedThisFrame();
        SprintHeld = _sprintAction != null && _sprintAction.IsPressed();
        RaisePressed = _raiseAction != null && _raiseAction.WasPressedThisFrame();
    }

    public void SetActions(InputActionAsset asset)
    {
        actions = asset;
    }
}
