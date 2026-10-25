using UnityEngine;

[DisallowMultipleComponent]
public class BlockVisual : MonoBehaviour
{
    [Tooltip("留空则在运行时自动收集本物体及子物体上的 Renderer（美术模型通常是多 Renderer）")]
    [SerializeField] private Renderer[] renderers = new Renderer[0];
    [SerializeField] private BlockPalette palette;
    [SerializeField] private bool warnWhenMissing = true;

    private bool _warned;

    public BlockPalette Palette
    {
        get => palette;
        set => palette = value;
    }

    private void Reset()
    {
        CollectRenderers();
    }

    public void CollectRenderers()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
    }

    public virtual void ApplyColor(BlockColor color)
    {
        if (palette == null)
        {
            if (warnWhenMissing && !_warned)
            {
                _warned = true;
                Debug.LogWarning($"[BlockVisual] {name} 没有指定 BlockPalette。", this);
            }

            return;
        }

        if (!palette.TryGetMaterial(color, out Material material))
        {
            if (warnWhenMissing && !_warned)
            {
                _warned = true;
                Debug.LogWarning($"[BlockVisual] 配色表里没有 {color} 的材质。", this);
            }

            return;
        }

        if (renderers == null || renderers.Length == 0)
        {
            CollectRenderers();
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].sharedMaterial = material;
            }
        }
    }
}
