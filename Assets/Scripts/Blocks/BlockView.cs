using UnityEngine;

public abstract class BlockView : MonoBehaviour
{
    public abstract void ApplyColor(BlockColorDefinition definition, bool instant);

    public virtual void OnRaised()
    {
    }
}
