using System.Collections.Generic;
using UnityEngine;

public class BlockLabelManager : MonoBehaviour
{
    [SerializeField] private bool createOnStart = true;

    [Header("标签参数（下发给所有方块）")]
    [Tooltip("文字比方块顶部高多少米")]
    [SerializeField] private float heightAboveTop = 0.9f;
    [SerializeField] private float fontSize = 2.2f;
    [Tooltip("超过这个距离自动隐藏，0 表示不限制")]
    [SerializeField] private float maxVisibleDistance = 30f;
    [Tooltip("文字颜色跟随方块当前颜色")]
    [SerializeField] private bool tintByCurrentColor;
    [Tooltip("手动挂在方块上的标签是否也套用这里的参数")]
    [SerializeField] private bool applyToExisting = true;

    private void Start()
    {
        if (createOnStart)
        {
            Rebuild();
        }
    }

    [ContextMenu("Rebuild Labels")]
    public void Rebuild()
    {
        IReadOnlyList<Block> blocks = Block.All;

        for (int i = 0; i < blocks.Count; i++)
        {
            Block block = blocks[i];

            if (block == null)
            {
                continue;
            }

            BlockLabel label = block.GetComponent<BlockLabel>();
            bool created = label == null;

            if (created)
            {
                label = block.gameObject.AddComponent<BlockLabel>();
            }

            if (created || applyToExisting)
            {
                label.Configure(heightAboveTop, fontSize, maxVisibleDistance, tintByCurrentColor);
            }
        }
    }

    [ContextMenu("Clear Labels")]
    public void Clear()
    {
        BlockLabel[] labels = FindObjectsOfType<BlockLabel>();

        for (int i = 0; i < labels.Length; i++)
        {
            if (labels[i] != null)
            {
                Destroy(labels[i]);
            }
        }
    }

    public void ApplyToAll()
    {
        BlockLabel[] labels = FindObjectsOfType<BlockLabel>();

        for (int i = 0; i < labels.Length; i++)
        {
            if (labels[i] != null)
            {
                labels[i].Configure(heightAboveTop, fontSize, maxVisibleDistance, tintByCurrentColor);
            }
        }
    }
}
