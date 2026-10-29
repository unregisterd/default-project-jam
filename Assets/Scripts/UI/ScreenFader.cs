using System.Collections;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [SerializeField] private CanvasGroup group;

    public bool IsCovered => group != null && group.alpha > 0.99f;

    private void Awake()
    {
        Instance = this;

        if (group != null)
        {
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
        }
    }

    public IEnumerator Fade(float target, float duration)
    {
        if (group == null)
        {
            yield break;
        }

        float start = group.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        group.alpha = target;
    }
}
