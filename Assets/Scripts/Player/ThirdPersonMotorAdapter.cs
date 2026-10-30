using System.Reflection;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
public class ThirdPersonMotorAdapter : MonoBehaviour, IMotor
{
    [Tooltip("留空则自动查找 ThirdPersonController（Starter Assets）")]
    [SerializeField] private MonoBehaviour controllerSource;

    private MonoBehaviour _controller;
    private CharacterController _characterController;
    private FieldInfo _verticalVelocityField;
    private FieldInfo _groundedField;
    private FieldInfo _gravityField;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();

        if (controllerSource == null)
        {
            MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();

            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] != null && behaviours[i].GetType().Name == "ThirdPersonController")
                {
                    controllerSource = behaviours[i];
                    break;
                }
            }
        }

        _controller = controllerSource;

        if (_controller == null)
        {
            Debug.LogWarning("[MotorAdapter] 没找到 ThirdPersonController，Launch/Teleport 会退化。", this);
            return;
        }

        System.Type type = _controller.GetType();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        _verticalVelocityField = type.GetField("_verticalVelocity", flags);
        _groundedField = type.GetField("Grounded", flags);
        _gravityField = type.GetField("Gravity", flags);
    }

    public bool IsGrounded
    {
        get
        {
            if (_controller != null && _groundedField != null)
            {
                return (bool)_groundedField.GetValue(_controller);
            }

            return _characterController != null && _characterController.isGrounded;
        }
    }

    public bool Enabled
    {
        get { return _controller != null && _controller.enabled; }
        set
        {
            if (_controller != null)
            {
                _controller.enabled = value;
            }
        }
    }

    public void Launch(float height, float forwardSpeed)
    {
        if (_controller == null || _verticalVelocityField == null)
        {
            return;
        }

        float gravity = _gravityField != null ? (float)_gravityField.GetValue(_controller) : -15f;
        float safeHeight = Mathf.Max(0.05f, height);
        _verticalVelocityField.SetValue(_controller, Mathf.Sqrt(2f * safeHeight * Mathf.Abs(gravity)));
    }

    public void Teleport(Vector3 position)
    {
        if (_verticalVelocityField != null && _controller != null)
        {
            _verticalVelocityField.SetValue(_controller, 0f);
        }

        if (_characterController == null)
        {
            transform.position = position;
            return;
        }

        _characterController.enabled = false;
        transform.position = position;
        _characterController.enabled = true;
    }
}
