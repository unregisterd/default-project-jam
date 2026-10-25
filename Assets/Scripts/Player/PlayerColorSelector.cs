using System;
using UnityEngine;

public class PlayerColorSelector : MonoBehaviour
{
    [SerializeField] private PlayerColorSet colorSet;
    [SerializeField] private int startIndex;

    public event Action<BlockColorDefinition> SelectionChanged;

    public int CurrentIndex { get; private set; }
    public int Count => colorSet != null ? colorSet.Count : 0;
    public BlockColorDefinition Current => colorSet != null ? colorSet.Get(CurrentIndex) : null;
    public string CurrentColorId => Current != null ? Current.id : null;

    private void Awake()
    {
        SetIndex(startIndex);
    }

    public void SetIndex(int index)
    {
        if (Count == 0)
        {
            return;
        }

        CurrentIndex = ((index % Count) + Count) % Count;
        SelectionChanged?.Invoke(Current);
    }

    public void RotateNext()
    {
        if (Count == 0)
        {
            return;
        }

        SetIndex(CurrentIndex + 1);
    }

    public void SetColorSet(PlayerColorSet set)
    {
        colorSet = set;
        SetIndex(CurrentIndex);
    }
}
