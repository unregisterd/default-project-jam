using UnityEngine;

[DisallowMultipleComponent]
public class TimeController : MonoBehaviour
{
    private static TimeController _instance;
    private static bool _paused;
    private static bool _hitStopActive;

    private float _hitStopEnd;
    private float _hitStopScale = 1f;

    public static bool IsPaused => _paused;

    public static void SetPaused(bool paused)
    {
        _paused = paused;

        if (_instance != null)
        {
            _instance.Apply();
        }
        else
        {
            Time.timeScale = paused ? 0f : 1f;
        }
    }

    public static void HitStop(float duration, float scale)
    {
        TimeController instance = EnsureInstance();
        instance._hitStopEnd = Time.unscaledTime + Mathf.Max(0f, duration);
        instance._hitStopScale = Mathf.Clamp(scale, 0.01f, 1f);
        _hitStopActive = true;
        instance.Apply();
    }

    private static TimeController EnsureInstance()
    {
        if (_instance != null)
        {
            return _instance;
        }

        GameObject go = new GameObject("TimeController");
        _instance = go.AddComponent<TimeController>();
        DontDestroyOnLoad(go);
        return _instance;
    }

    private void Update()
    {
        if (_hitStopActive && Time.unscaledTime >= _hitStopEnd)
        {
            _hitStopActive = false;
            Apply();
        }
    }

    private void Apply()
    {
        Time.timeScale = _paused ? 0f : (_hitStopActive ? _hitStopScale : 1f);
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
            _paused = false;
            _hitStopActive = false;
        }

        Time.timeScale = 1f;
    }
}
