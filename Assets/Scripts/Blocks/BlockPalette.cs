using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Block Palette", fileName = "BlockPalette")]
public class BlockPalette : ScriptableObject
{
    [SerializeField] private BlockColorDefinition[] colors = Array.Empty<BlockColorDefinition>();

    public BlockColorDefinition[] Colors => colors;
    public int Count => colors != null ? colors.Length : 0;

    public bool TryGet(string id, out BlockColorDefinition definition)
    {
        if (string.IsNullOrEmpty(id) || colors == null)
        {
            definition = null;
            return false;
        }

        for (int i = 0; i < colors.Length; i++)
        {
            if (string.Equals(colors[i].id, id, StringComparison.OrdinalIgnoreCase))
            {
                definition = colors[i];
                return true;
            }
        }

        definition = null;
        return false;
    }

    public void SetColors(BlockColorDefinition[] newColors)
    {
        colors = newColors ?? Array.Empty<BlockColorDefinition>();
    }

    public BlockColorDefinition GetOrDefault(string id)
    {
        return TryGet(id, out BlockColorDefinition definition) ? definition : null;
    }
}
