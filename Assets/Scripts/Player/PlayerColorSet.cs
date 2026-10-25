using UnityEngine;

[CreateAssetMenu(menuName = "Game/Player Color Set", fileName = "PlayerColorSet")]
public class PlayerColorSet : ScriptableObject
{
    [Tooltip("颜色定义来源")]
    [SerializeField] private BlockPalette palette;

    [Tooltip("玩家跳跃时可轮转到的颜色 ID，按顺序循环（就是那“固定一小撮”）")]
    [SerializeField] private string[] colorIds = new string[0];

    public BlockPalette Palette => palette;
    public int Count => colorIds != null ? colorIds.Length : 0;

    public BlockColorDefinition Get(int index)
    {
        if (palette == null || Count == 0)
        {
            return null;
        }

        int safeIndex = ((index % Count) + Count) % Count;
        return palette.GetOrDefault(colorIds[safeIndex]);
    }
}
