using UnityEngine;

public class WisdomAbility : MonoBehaviour, IAbility
{
    [SerializeField] private float radius = 12f;

    private HiddenBlock[] _blocks;

    public AbilityType Type => AbilityType.Wisdom;
    public int Cost => 1;

    private void Start()
    {
        RefreshBlocks();
    }

    public bool CanUse(out string reason)
    {
        if (_blocks == null || _blocks.Length == 0)
        {
            RefreshBlocks();
        }

        for (int i = 0; i < _blocks.Length; i++)
        {
            HiddenBlock block = _blocks[i];

            if (block == null || block.Revealed)
            {
                continue;
            }

            if (IsInRange(block))
            {
                reason = null;
                return true;
            }
        }

        reason = "附近没有隐藏道路";
        return false;
    }

    public void Execute()
    {
        for (int i = 0; i < _blocks.Length; i++)
        {
            HiddenBlock block = _blocks[i];

            if (block == null || block.Revealed)
            {
                continue;
            }

            if (IsInRange(block))
            {
                block.Reveal();
            }
        }
    }

    private bool IsInRange(HiddenBlock block)
    {
        return Vector3.Distance(block.transform.position, transform.position) <= radius;
    }

    private void RefreshBlocks()
    {
        _blocks = FindObjectsOfType<HiddenBlock>(true);
    }
}
