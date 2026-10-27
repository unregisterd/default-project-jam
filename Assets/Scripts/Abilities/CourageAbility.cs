using UnityEngine;

public class CourageAbility : MonoBehaviour, IAbility
{
    [SerializeField] private FirstPersonMotor motor;
    [Tooltip("跳跃高度（米）")]
    [SerializeField] private float jumpHeight = 3.3f;
    [Tooltip("额外水平速度（米/秒）")]
    [SerializeField] private float forwardSpeed = 2f;

    public AbilityType Type => AbilityType.Courage;
    public int Cost => 1;

    private void Awake()
    {
        if (motor == null)
        {
            motor = GetComponent<FirstPersonMotor>();
        }
    }

    public void Configure(float height, float speed)
    {
        jumpHeight = height;
        forwardSpeed = speed;
    }

    public bool CanUse(out string reason)
    {
        if (motor == null)
        {
            reason = "缺少 FirstPersonMotor";
            return false;
        }

        if (!motor.IsGrounded)
        {
            reason = "只能在地面使用";
            return false;
        }

        reason = null;
        return true;
    }

    public void Execute()
    {
        motor.Launch(jumpHeight, forwardSpeed);
    }
}
