using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Block : MonoBehaviour
{
    private static readonly List<Block> Registry = new List<Block>();

    public static IReadOnlyList<Block> All => Registry;

    [Tooltip("颜色序列资产；同一个 BlockType 可以被多个方块共用")]
    [SerializeField] private BlockType type;
    [SerializeField] private BlockPalette palette;
    [Tooltip("留空则自动在子物体上查找 BlockView")]
    [SerializeField] private BlockView view;
    [Tooltip("该方块是否允许被 F 键升高")]
    [SerializeField] private bool canRaise = true;
    [Tooltip("初始在序列里的位置，一般填 0")]
    [SerializeField] private int startColorIndex;
    [Tooltip("勾选后开局显示为灰色，等 Reveal() 才显示真实颜色（灰白世界 → 颜色显现）")]
    [SerializeField] private bool startGray;
    [Tooltip("灰色预览使用的颜色 ID")]
    [SerializeField] private string grayColorId = "Gray";

    private int _colorIndex;
    private bool _raised;
    private bool _warned;
    private bool _grayed;

    public event Action<Block> ColorChanged;

    public BlockType Type => type;
    public bool CanRaise => canRaise && !_raised;
    public bool HasRaised => _raised;
    public int ColorIndex => _colorIndex;

    public string CurrentColorId
    {
        get
        {
            BlockColorDefinition definition = CurrentDefinition;
            return definition != null ? definition.id : null;
        }
    }

    public BlockColorDefinition CurrentDefinition
    {
        get
        {
            if (palette == null)
            {
                return null;
            }

            if (_grayed || (!Application.isPlaying && startGray))
            {
                BlockColorDefinition gray = palette.GetOrDefault(grayColorId);

                if (gray != null)
                {
                    return gray;
                }
            }

            if (type == null || !type.IsValid)
            {
                return null;
            }

            int index = Mathf.Clamp(_colorIndex, 0, type.Colors.Length - 1);
            return palette.GetOrDefault(type.Colors[index]);
        }
    }

    public bool CanAdvance => type != null && type.IsValid && (type.Loop || _colorIndex < type.Colors.Length - 1);

    private void OnEnable()
    {
        if (!Registry.Contains(this))
        {
            Registry.Add(this);
        }
    }

    private void OnDisable()
    {
        Registry.Remove(this);
    }

    private void Awake()
    {
        if (view == null)
        {
            view = GetComponentInChildren<BlockView>(true);
        }

        if (palette == null && type != null)
        {
            palette = type.Palette;
        }

        _grayed = startGray;
        ResetState();
    }

    public bool TryAdvance()
    {
        if (_grayed || !CanAdvance)
        {
            return false;
        }

        int count = type.Colors.Length;
        _colorIndex = type.Loop ? (_colorIndex + 1) % count : _colorIndex + 1;

        ApplyColor(false);
        ColorChanged?.Invoke(this);
        return true;
    }

    public void ResetState()
    {
        int count = type != null && type.IsValid ? type.Colors.Length : 1;
        _colorIndex = Mathf.Clamp(startColorIndex, 0, count - 1);
        _raised = false;
        ApplyColor(true);
    }

    public void Reveal()
    {
        if (!_grayed)
        {
            return;
        }

        _grayed = false;
        ApplyColor(true);
    }

    public void SetStartGray(bool value)
    {
        startGray = value;
        _grayed = value;
    }

    public void MarkRaised()
    {
        _raised = true;
    }

    public float GetTopHeight()
    {
        Collider collider = GetComponentInChildren<Collider>();
        return collider != null ? collider.bounds.max.y : transform.position.y;
    }

    public void ApplyColor(bool instant)
    {
        if (view == null)
        {
            return;
        }

        BlockColorDefinition definition = CurrentDefinition;

        if (definition == null)
        {
            if (!_warned)
            {
                _warned = true;
                Debug.LogWarning($"[Block] {name} 缺少 BlockType 或颜色未在 BlockPalette 中定义。", this);
            }

            return;
        }

        view.ApplyColor(definition, instant);
    }

    public void SetType(BlockType blockType)
    {
        type = blockType;
    }

    public void SetPalette(BlockPalette blockPalette)
    {
        palette = blockPalette;
    }

    public void SetView(BlockView blockView)
    {
        view = blockView;
    }

    public void SetCanRaise(bool value)
    {
        canRaise = value;
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            return;
        }

        if (view == null)
        {
            view = GetComponentInChildren<BlockView>(true);
        }

        if (palette == null && type != null)
        {
            palette = type.Palette;
        }

        if (type != null)
        {
            ApplyColor(true);
        }
    }
}
