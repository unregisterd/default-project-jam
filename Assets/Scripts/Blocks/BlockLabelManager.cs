using System.Collections.Generic;
using UnityEngine;

public class BlockLabelManager : MonoBehaviour
{
    [SerializeField] private bool createOnStart = true;

    private void Start()
    {
        if (createOnStart)
        {
            Rebuild();
        }
    }

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

            if (block.GetComponent<BlockLabel>() == null)
            {
                block.gameObject.AddComponent<BlockLabel>();
            }
        }
    }

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
}
