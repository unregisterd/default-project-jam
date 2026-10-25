using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRaiseAbility : MonoBehaviour
{
    [SerializeField] private PlayerInputReader input;
    [SerializeField] private PlayerBlockAgent blockAgent;
    [SerializeField] private PlayerColorSelector colorSelector;
    [SerializeField] private FirstPersonMotor motor;
    [Tooltip("升高动画时长（秒），0 为瞬间完成")]
    [SerializeField] private float raiseDuration = 0.45f;
    [Tooltip("目标高度差小于该值时视为无需升高")]
    [SerializeField] private float minHeightDelta = 0.05f;
    [SerializeField] private bool logRejections = true;

    private bool _raising;

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

        Block target = FindNearestColorBlock(colorSelector.CurrentColorId, standing);

        if (target == null)
        {
            LogReject($"附近没有颜色为 {colorSelector.CurrentColorId} 的方块");
            return false;
        }

        float delta = target.GetTopHeight() - standing.GetTopHeight();

        if (delta <= minHeightDelta)
        {
            LogReject("目标高度不高于当前方块");
            return false;
        }

        StartCoroutine(RaiseRoutine(standing, delta));
        return true;
    }

    private Block FindNearestColorBlock(string colorId, Block exclude)
    {
        Block nearest = null;
        float nearestDistance = float.MaxValue;
        Vector3 position = transform.position;

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

            Vector3 delta = candidate.transform.position - position;
            float distance = delta.x * delta.x + delta.z * delta.z;

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = candidate;
            }
        }

        return nearest;
    }

    private IEnumerator RaiseRoutine(Block block, float delta)
    {
        _raising = true;
        block.MarkRaised();

        bool carriesPlayer = blockAgent != null && blockAgent.StandingBlock == block;

        if (carriesPlayer && motor != null)
        {
            motor.enabled = false;
        }

        Vector3 startPosition = block.transform.position;
        float elapsed = 0f;

        while (elapsed < raiseDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / raiseDuration);
            float eased = progress * progress * (3f - 2f * progress);
            Vector3 targetPosition = startPosition + Vector3.up * (delta * eased);
            float step = targetPosition.y - block.transform.position.y;

            block.transform.position += Vector3.up * step;

            if (carriesPlayer)
            {
                transform.position += Vector3.up * step;
            }

            yield return null;
        }

        block.transform.position = startPosition + Vector3.up * delta;

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

        _raising = false;
        Raised?.Invoke(block);
    }

    private void LogReject(string reason)
    {
        if (logRejections)
        {
            Debug.Log($"[Raise] 无法长高：{reason}");
        }
    }
}
