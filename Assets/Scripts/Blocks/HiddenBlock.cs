using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Block))]
public class HiddenBlock : MonoBehaviour
{
    [SerializeField] private Renderer[] renderers = new Renderer[0];
    [SerializeField] private Collider[] colliders = new Collider[0];
    [SerializeField] private bool hiddenOnAwake = true;
    [SerializeField] private float revealDuration = 0.35f;

    public bool Revealed { get; private set; }

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<Renderer>(true);
        }

        if (colliders == null || colliders.Length == 0)
        {
            colliders = GetComponentsInChildren<Collider>(true);
        }

        if (hiddenOnAwake)
        {
            ApplyHidden();
        }
    }

    public void Reveal()
    {
        if (Revealed)
        {
            return;
        }

        Revealed = true;

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                colliders[i].enabled = true;
            }
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].enabled = true;
            }
        }

        StartCoroutine(PopIn());
    }

    private void ApplyHidden()
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                colliders[i].enabled = false;
            }
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].enabled = false;
            }
        }
    }

    private IEnumerator PopIn()
    {
        Vector3 targetScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < revealDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / revealDuration);
            transform.localScale = targetScale * (0.85f + 0.15f * progress);
            yield return null;
        }

        transform.localScale = targetScale;
    }
}
