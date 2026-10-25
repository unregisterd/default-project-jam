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

    [Header("图标索引（按从左到右、从上到下的顺序）")]
    [SerializeField] private int lockIndex;
    [SerializeField] private int arrowIndex = 1;
    [SerializeField] private int loopIndex = 2;

    [Header("贴片")]
    [Tooltip("图标占方块顶面的比例")]
    [Range(0.1f, 1f)]
    [SerializeField] private float sizeScale = 0.62f;
    [Tooltip("贴片离顶面的高度，避免 z-fighting")]
    [SerializeField] private float heightOffset = 0.012f;

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
                sizeScale,
                heightOffset);
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
