using System.Collections;
using UnityEngine;

public class HitStop : MonoBehaviour
{
    private static HitStop _instance;
    private Coroutine _routine;

    public static void Play(float duration = 0.05f, float scale = 0.05f)
    {
        if (_instance == null)
        {
            GameObject go = new GameObject("HitStop");
            _instance = go.AddComponent<HitStop>();
            DontDestroyOnLoad(go);
        }

        if (_instance._routine != null)
        {
            _instance.StopCoroutine(_instance._routine);
        }

        _instance._routine = _instance.StartCoroutine(_instance.Run(duration, scale));
    }

    private IEnumerator Run(float duration, float scale)
    {
        float previous = Time.timeScale;
        Time.timeScale = Mathf.Clamp(scale, 0.01f, 1f);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = previous <= 0f ? 1f : previous;
        _routine = null;
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }

        Time.timeScale = 1f;
    }
}
