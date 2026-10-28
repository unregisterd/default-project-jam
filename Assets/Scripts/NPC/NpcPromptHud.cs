using TMPro;
using UnityEngine;

public class NpcPromptHud : MonoBehaviour
{
    private static NpcPromptHud _instance;

    private string _speaker = string.Empty;
    private string _line = string.Empty;
    private float _visibleUntil;
    private GUIStyle _speakerStyle;
    private GUIStyle _lineStyle;

    public static NpcPromptHud Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("NpcPromptHud");
                _instance = go.AddComponent<NpcPromptHud>();
                DontDestroyOnLoad(go);
            }

            return _instance;
        }
    }

    public static void Show(string speaker, string line, float duration = 999f)
    {
        NpcPromptHud hud = Instance;
        hud._speaker = speaker;
        hud._line = line;
        hud._visibleUntil = Time.time + duration;
    }

    public static void Clear(string speaker)
    {
        if (_instance == null || _instance._speaker != speaker)
        {
            return;
        }

        _instance._visibleUntil = Mathf.Min(_instance._visibleUntil, Time.time);
    }

    private void OnGUI()
    {
        if (string.IsNullOrEmpty(_line) || Time.time > _visibleUntil)
        {
            return;
        }

        float scale = Mathf.Clamp(Screen.height / 1080f, 0.6f, 2.5f);

        if (_speakerStyle == null)
        {
            _speakerStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
            };

            _lineStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
            };
        }

        _speakerStyle.fontSize = Mathf.RoundToInt(20f * scale);
        _lineStyle.fontSize = Mathf.RoundToInt(24f * scale);
        _speakerStyle.normal.textColor = Color.black;
        _lineStyle.normal.textColor = Color.black;

        float width = Mathf.Min(Screen.width * 0.7f, 900f * scale);
        float x = (Screen.width - width) * 0.5f;
        float y = 40f * scale;

        Color previous = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, 0.72f);
        GUI.DrawTexture(new Rect(x - 16f * scale, y - 10f * scale, width + 32f * scale, 96f * scale), Texture2D.whiteTexture);
        GUI.color = previous;

        if (!string.IsNullOrEmpty(_speaker))
        {
            GUI.Label(new Rect(x, y, width, 28f * scale), _speaker, _speakerStyle);
        }

        GUI.Label(new Rect(x, y + 30f * scale, width, 60f * scale), _line, _lineStyle);
    }
}
