using UnityEngine;

public class BlockView_Whitebox : BlockView
{
    [SerializeField] private Renderer[] renderers = new Renderer[0];

    private void Reset()
    {
        CollectRenderers();
    }

    public void CollectRenderers()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
    }

    public override void ApplyColor(BlockColorDefinition definition, bool instant)
    {
        if (definition == null || definition.material == null)
        {
            return;
        }

        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<Renderer>(true);
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].sharedMaterial = definition.material;
            }
        }
    }
}
