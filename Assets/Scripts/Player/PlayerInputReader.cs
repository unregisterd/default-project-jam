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
    public bool InteractPressed { get; private set; }
    public bool Ability1Pressed { get; private set; }
    public bool Ability2Pressed { get; private set; }
    public bool Ability3Pressed { get; private set; }
    public bool Ability4Pressed { get; private set; }
    public bool IsReady => _ready;

    private InputActionMap _map;
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _jumpAction;
    private InputAction _sprintAction;
    private InputAction _interactAction;
    private InputAction _ability1Action;
    private InputAction _ability2Action;
    private InputAction _ability3Action;
    private InputAction _ability4Action;
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
        _interactAction = _map.FindAction("Interact", false);
        _ability1Action = _map.FindAction("Ability1", false);
        _ability2Action = _map.FindAction("Ability2", false);
        _ability3Action = _map.FindAction("Ability3", false);
        _ability4Action = _map.FindAction("Ability4", false);

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
        InteractPressed = _interactAction != null && _interactAction.WasPressedThisFrame();
        Ability1Pressed = _ability1Action != null && _ability1Action.WasPressedThisFrame();
        Ability2Pressed = _ability2Action != null && _ability2Action.WasPressedThisFrame();
        Ability3Pressed = _ability3Action != null && _ability3Action.WasPressedThisFrame();
        Ability4Pressed = _ability4Action != null && _ability4Action.WasPressedThisFrame();
    }

    public void SetActions(InputActionAsset asset)
    {
        actions = asset;
    }
}
