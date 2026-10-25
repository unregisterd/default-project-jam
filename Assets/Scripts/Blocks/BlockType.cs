using UnityEngine;

[CreateAssetMenu(menuName = "Game/Block Type", fileName = "BlockType")]
public class BlockType : ScriptableObject
{
    [Tooltip("该类型使用的调色板；方块实例留空时回退到这里")]
    [SerializeField] private BlockPalette palette;

    [Tooltip("颜色序列：每次被成功踩中前进一格。顺序固定，不可回退")]
    [SerializeField] private string[] colors = { "Red", "Blue" };

    [Tooltip("勾选后序列循环；不勾选则走到最后一个颜色后永久锁定")]
    [SerializeField] private bool loop;

    [Tooltip("在编辑器里补充说明用途（如：单向红蓝、固定蓝、可升平台）")]
    [SerializeField] private string note;

    public BlockPalette Palette => palette;
    public string[] Colors => colors;
    public bool Loop => loop;
    public string Note => note;

    public bool IsValid => colors != null && colors.Length > 0;
}
