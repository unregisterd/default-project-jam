using System.Collections.Generic;
using UnityEngine;

public class BlockIconManager : MonoBehaviour
{
    [SerializeField] private bool createOnStart = true;

    [Header("图集")]
    [Tooltip("图标图集：透明背景、白色图形、按 columns × rows 均分")]
    [SerializeField] private Texture2D atlas;
    [SerializeField] private int columns = 2;
    [SerializeField] private int rows = 2;

    [Header("图标索引（从左到右、从上到下）")]
    [SerializeField] private int lockIndex;
    [SerializeField] private int arrowIndex = 1;
    [SerializeField] private int loopIndex = 2;

    [Header("摆放")]
    [Tooltip("Billboard = 竖着面向玩家；FlatOnTop = 平贴顶面")]
    [SerializeField] private BlockIconMode mode = BlockIconMode.Billboard;
    [Tooltip("图标边长（米）")]
    [SerializeField] private float iconSize = 0.55f;
    [Tooltip("图标底部离方块顶面的高度（米）")]
    [SerializeField] private float heightAboveTop = 0.12f;

    private void Start()
    {
        if (createOnStart)
        {
            Rebuild();
        }
    }

    [ContextMenu("Rebuild Icons")]
    public void Rebuild()
    {
        if (atlas == null)
        {
            Debug.LogWarning("[BlockIcon] 没有指定图集，跳过。", this);
            return;
        }

        IReadOnlyList<Block> blocks = Block.All;

        for (int i = 0; i < blocks.Count; i++)
        {
            Block block = blocks[i];

            if (block == null)
            {
                continue;
            }

            BlockIconVisual visual = block.GetComponent<BlockIconVisual>();

            if (visual == null)
            {
                visual = block.gameObject.AddComponent<BlockIconVisual>();
            }

            visual.Apply(
                atlas,
                columns,
                rows,
                ResolveIconIndex(block.Type),
                mode,
                iconSize,
                heightAboveTop);
        }
    }

    [ContextMenu("Clear Icons")]
    public void Clear()
    {
        BlockIconVisual[] visuals = FindObjectsOfType<BlockIconVisual>();

        for (int i = 0; i < visuals.Length; i++)
        {
            if (visuals[i] != null)
            {
                Destroy(visuals[i]);
            }
        }
    }

    private int ResolveIconIndex(BlockType type)
    {
        if (type == null || !type.IsValid)
        {
            return lockIndex;
        }

        if (type.Colors.Length <= 1)
        {
            return lockIndex;
        }

        return type.Loop ? loopIndex : arrowIndex;
    }
}
