using UnityEngine;

public class CourageAbility : MonoBehaviour, IAbility
{
    [Tooltip("留空则自动查找同一物体上的 IMotor（第一人称或第三人称适配器）")]
    [SerializeField] private MonoBehaviour motorSource;
    private IMotor motor;
    [Tooltip("跳跃高度（米）")]
    [SerializeField] private float jumpHeight = 3.3f;
    [Tooltip("额外水平速度（米/秒）")]
    [SerializeField] private float forwardSpeed = 2f;

    public AbilityType Type => AbilityType.Courage;
    public int Cost => 1;

    private void Awake()
    {
        motor = motorSource as IMotor;

        if (motor == null)
        {
            motor = GetComponent<IMotor>();
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
            reason = "缺少 IMotor";
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
