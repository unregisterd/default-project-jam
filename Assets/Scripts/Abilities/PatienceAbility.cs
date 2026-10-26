using System.Collections;
using UnityEngine;

public class PatienceAbility : MonoBehaviour, IAbility
{
    [SerializeField] private PlayerBlockAgent agent;
    [SerializeField] private FirstPersonMotor motor;
    [SerializeField] private RaisePillarVisual pillarVisual;
    [Tooltip("抬升高度（米）")]
    [SerializeField] private float riseHeight = 2f;
    [SerializeField] private float duration = 1f;

    private bool _raising;

    public AbilityType Type => AbilityType.Patience;
    public int Cost => 1;
    public bool IsRaising => _raising;

    private void Awake()
    {
        if (agent == null)
        {
            agent = GetComponent<PlayerBlockAgent>();
        }

        if (motor == null)
        {
            motor = GetComponent<FirstPersonMotor>();
        }

        if (pillarVisual == null)
        {
            pillarVisual = GetComponent<RaisePillarVisual>();

            if (pillarVisual == null)
            {
                pillarVisual = gameObject.AddComponent<RaisePillarVisual>();
            }
        }
    }

    public void Configure(float height, float riseDuration)
    {
        riseHeight = height;
        duration = Mathf.Max(0.05f, riseDuration);
    }

    public bool CanUse(out string reason)
    {
        if (_raising)
        {
            reason = "正在抬升";
            return false;
        }

        if (agent == null)
        {
            reason = "缺少 PlayerBlockAgent";
            return false;
        }

        if (agent.StandingBlock == null)
        {
            reason = "脚下没有方块";
            return false;
        }

        reason = null;
        return true;
    }

    public void Execute()
    {
        Block block = agent != null ? agent.StandingBlock : null;

        if (block != null)
        {
            StartCoroutine(Run(block));
        }
    }

    private IEnumerator Run(Block block)
    {
        _raising = true;

        if (pillarVisual != null)
        {
            pillarVisual.Play(block, riseHeight);
        }

        if (motor != null)
        {
            motor.enabled = false;
        }

        float startY = block.transform.position.y;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float eased = progress * progress * (3f - 2f * progress);
            float targetY = startY + riseHeight * eased;
            float step = targetY - block.transform.position.y;

            block.transform.position += Vector3.up * step;
            transform.position += Vector3.up * step;

            yield return null;
        }

        block.transform.position = new Vector3(block.transform.position.x, startY + riseHeight, block.transform.position.z);

        if (motor != null)
        {
            motor.enabled = true;
        }

        if (agent != null)
        {
            agent.NotifyExternalMove();
        }

        _raising = false;
    }
}
