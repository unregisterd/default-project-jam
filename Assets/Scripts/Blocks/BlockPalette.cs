using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Block Palette", fileName = "BlockPalette")]
public class BlockPalette : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public BlockColor color;
        public Material material;
    }

    [Tooltip("颜色 → 材质。换成美术模型后，只要在这里换材质即可")]
    [SerializeField] private Entry[] entries = Array.Empty<Entry>();

    public bool TryGetMaterial(BlockColor color, out Material material)
    {
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].color != color)
            {
                continue;
            }

            material = entries[i].material;
            return material != null;
        }

        material = null;
        return false;
    }

    public void SetEntries(Entry[] newEntries)
    {
        entries = newEntries ?? Array.Empty<Entry>();
    }
}
