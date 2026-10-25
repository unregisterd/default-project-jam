using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerRaiseAbility : MonoBehaviour
{
    [SerializeField] private PlayerInputReader input;
    [SerializeField] private PlayerBlockAgent blockAgent;
    [SerializeField] private PlayerColorSelector colorSelector;
    [SerializeField] private FirstPersonMotor motor;

    [Header("升高")]
    [Tooltip("升高动画时长（秒）")]
    [SerializeField] private float raiseDuration = 0.45f;
    [Tooltip("柱段高度，也是升高的最小单位；delta 会吸附到它的整数倍")]
    [SerializeField] private float raiseStep = 2f;
    [Tooltip("目标高度差小于该值时视为无需升高")]
    [SerializeField] private float minHeightDelta = 0.5f;
    [Tooltip("目标高度差大于该值时视为太远，防止瞬间飞天")]
    [SerializeField] private float maxHeightDelta = 3f;
    [Tooltip("搜索同色参照方块的最大水平距离，0 表示不限制")]
    [SerializeField] private float maxTargetRange = 10f;
    [Tooltip("柱段表现（留空则自动获取）")]
    [SerializeField] private RaisePillarVisual pillarVisual;
    [Tooltip("阻挡检测层；设为 Nothing 则不检测")]
    [SerializeField] private LayerMask obstacleMask;

    [Header("调试")]
    [SerializeField] private bool logRejections = true;

    private bool _raising;
    private float _lastLogTime;

    public event Action<Block, float> RaiseStarted;
    public event Action<Block> Raised;

    private void Awake()
    {
        if (input == null)
        {
            input = GetComponent<PlayerInputReader>();
        }

        if (blockAgent == null)
        {
            blockAgent = GetComponent<PlayerBlockAgent>();
        }

        if (colorSelector == null)
        {
            colorSelector = GetComponent<PlayerColorSelector>();
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

    private void Update()
    {
        if (_raising || input == null || !input.RaisePressed)
        {
            return;
        }

        TryRaise();
    }

    public bool TryRaise()
    {
        Block standing = blockAgent != null ? blockAgent.StandingBlock : null;

        if (standing == null)
        {
            LogReject("脚下没有方块");
            return false;
        }

        if (!standing.CanRaise)
        {
            LogReject("该方块不能长高（未开启或已升过一次）");
            return false;
        }

        if (colorSelector == null || string.IsNullOrEmpty(colorSelector.CurrentColorId))
        {
            LogReject("没有选中的颜色");
            return false;
        }

        float standingTop = standing.GetTopHeight();
        Block target = FindHigherColorBlock(colorSelector.CurrentColorId, standing, standingTop);

        if (target == null)
        {
            LogReject($"附近没有比当前更高且合适的 {colorSelector.CurrentColorId} 方块");
            return false;
        }

        float delta = target.GetTopHeight() - standingTop;
        float quantized = QuantizeDelta(delta);

        if (quantized < minHeightDelta)
        {
            LogReject($"高度差 {delta:0.00}m 不足一个柱段");
            return false;
        }

        StartRaise(standing, quantized);
        return true;
    }

    private float QuantizeDelta(float delta)
    {
        if (raiseStep <= 0f)
        {
            return delta;
        }

        return Mathf.Floor(delta / raiseStep + 0.01f) * raiseStep;
    }

    private Block FindHigherColorBlock(string colorId, Block exclude, float standingTop)
    {
        Block nearest = null;
        float nearestDistance = float.MaxValue;
        float maxDistanceSqr = maxTargetRange > 0f ? maxTargetRange * maxTargetRange : float.MaxValue;
        Vector3 origin = exclude.transform.position;

        IReadOnlyList<Block> all = Block.All;

        for (int i = 0; i < all.Count; i++)
        {
            Block candidate = all[i];

            if (candidate == null || candidate == exclude)
            {
                continue;
            }

            if (!string.Equals(candidate.CurrentColorId, colorId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            float heightDelta = candidate.GetTopHeight() - standingTop;

            if (heightDelta < minHeightDelta || heightDelta > maxHeightDelta)
            {
                continue;
            }

            Vector3 offset = candidate.transform.position - origin;
            float distance = offset.x * offset.x + offset.z * offset.z;

            if (distance > maxDistanceSqr || distance >= nearestDistance)
            {
                continue;
            }

            if (IsBlocked(exclude, candidate))
            {
                continue;
            }

            nearestDistance = distance;
            nearest = candidate;
        }

        return nearest;
    }

    private bool IsBlocked(Block standing, Block target)
    {
        if (obstacleMask.value == 0)
        {
            return false;
        }

        Vector3 start = standing.transform.position + Vector3.up * 1.5f;
        Vector3 direction = target.transform.position - start;
        float distance = direction.magnitude;

        if (distance <= 0.01f)
        {
            return false;
        }

        RaycastHit[] hits = Physics.RaycastAll(start, direction / distance, distance, obstacleMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            Block hitBlock = hits[i].collider.GetComponentInParent<Block>();

            if (hitBlock == null || hitBlock == standing || hitBlock == target)
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private void StartRaise(Block block, float delta)
    {
        _raising = true;
        block.MarkRaised();

        bool carriesPlayer = blockAgent != null && blockAgent.StandingBlock == block;

        if (carriesPlayer && motor != null)
        {
            motor.enabled = false;
        }

        RaiseStarted?.Invoke(block, delta);

        if (pillarVisual != null)
        {
            pillarVisual.Play(block, delta);
        }

        float startY = block.transform.position.y;
        float targetY = startY + delta;
        float lastY = startY;

        block.transform
            .DOMoveY(targetY, raiseDuration)
            .SetEase(Ease.InOutQuad)
            .SetLink(block.gameObject)
            .OnUpdate(() =>
            {
                if (!carriesPlayer)
                {
                    return;
                }

                float currentY = block.transform.position.y;
                float step = currentY - lastY;

                if (Mathf.Abs(step) < 0.00001f)
                {
                    return;
                }

                transform.position += Vector3.up * step;
                lastY = currentY;
            })
            .OnComplete(() => FinishRaise(block, targetY, carriesPlayer))
            .OnKill(() =>
            {
                if (!_raising)
                {
                    return;
                }

                _raising = false;

                if (motor != null)
                {
                    motor.enabled = true;
                }
            });
    }

    private void FinishRaise(Block block, float targetY, bool carriesPlayer)
    {
        if (!_raising)
        {
            return;
        }

        _raising = false;
        block.transform.position = new Vector3(block.transform.position.x, targetY, block.transform.position.z);

        if (carriesPlayer)
        {
            if (motor != null)
            {
                motor.enabled = true;
            }

            if (blockAgent != null)
            {
                blockAgent.NotifyExternalMove();
            }
        }

        Raised?.Invoke(block);
    }

    private void LogReject(string reason)
    {
        if (!logRejections || Time.time - _lastLogTime < 1f)
        {
            return;
        }

        _lastLogTime = Time.time;
        Debug.Log($"[Raise] 无法长高：{reason}");
    }
}
