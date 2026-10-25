using System;
using UnityEngine;

[DisallowMultipleComponent]
public class Block : MonoBehaviour
{
    [SerializeField] private BlockColor initialColor = BlockColor.Red;
    [SerializeField] private BlockVisual visual;
    [SerializeField] private bool canFlip = true;

    public event Action<Block, BlockColor> ColorChanged;

    public BlockColor Color { get; private set; } = BlockColor.None;
    public bool CanFlip => canFlip;

    private void Awake()
    {
        if (visual == null)
        {
            visual = GetComponentInChildren<BlockVisual>(true);
        }

        SetColor(initialColor);
    }

    public void SetColor(BlockColor color)
    {
        if (color == BlockColor.None || color == Color)
        {
            return;
        }

        Color = color;

        if (visual != null)
        {
            visual.ApplyColor(color);
        }

        ColorChanged?.Invoke(this, color);
    }

    public void Flip()
    {
        if (!canFlip)
        {
            return;
        }

        SetColor(Color.Opposite());
    }

    public void SetVisual(BlockVisual newVisual)
    {
        visual = newVisual;
    }

    public void SetInitialColor(BlockColor color)
    {
        initialColor = color;
    }
}
