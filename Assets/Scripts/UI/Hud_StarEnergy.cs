using UnityEngine;

public enum HudAnchor
{
    TopLeft = 0,
    TopCenter = 1,
    TopRight = 2,
    BottomLeft = 3,
    BottomCenter = 4,
    BottomRight = 5,
}

public class Hud_StarEnergy : MonoBehaviour
{
    private const string HintText = "1 Courage  2 Wisdom  3 Patience  4 Hope";

    [SerializeField] private AbilityWallet wallet;

    [Header("位置")]
    [SerializeField] private HudAnchor anchor = HudAnchor.TopRight;
    [Tooltip("相对锚点的偏移（像素，按 1080p 计）")]
    [SerializeField] private Vector2 offset = new Vector2(32f, 32f);
    [Tooltip("在自动适配基础上再乘一个系数")]
    [Range(0.5f, 2f)]
    [SerializeField] private float scaleMultiplier = 1f;

    [Header("大小（1080p 下的像素）")]
    [SerializeField] private float orbSize = 26f;
    [SerializeField] private float orbGap = 10f;
    [SerializeField] private float padding = 14f;
    [SerializeField] private bool showHint = true;

    private GUIStyle _style;

    private static Hud_StarEnergy _active;

    private void Awake()
    {
        if (_active != null && _active != this)
        {
            Destroy(this);
            return;
        }

        _active = this;

        if (wallet == null)
        {
            wallet = GetComponentInParent<AbilityWallet>();
        }

        if (wallet == null)
        {
            wallet = FindObjectOfType<AbilityWallet>();
        }
    }

    private void OnDestroy()
    {
        if (_active == this)
        {
            _active = null;
        }
    }

    private void OnGUI()
    {
        if (wallet == null)
        {
            return;
        }

        float scale = Mathf.Clamp(Screen.height / 1080f, 0.6f, 2.5f) * scaleMultiplier;

        if (_style == null)
        {
            _style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                richText = false,
            };
        }

        _style.fontSize = Mathf.RoundToInt(18f * scale);

        float size = orbSize * scale;
        float gap = orbGap * scale;
        float pad = padding * scale;

        float orbsWidth = wallet.MaxPoints * size + Mathf.Max(0, wallet.MaxPoints - 1) * gap;
        float hintWidth = showHint ? _style.CalcSize(new GUIContent(HintText)).x + 12f * scale : 0f;

        float contentWidth = orbsWidth + hintWidth;
        float contentHeight = size;
        float panelWidth = contentWidth + pad * 2f;
        float panelHeight = contentHeight + pad * 2f;

        Vector2 panelPosition = ResolvePosition(panelWidth, panelHeight, scale);
        Rect panel = new Rect(panelPosition.x, panelPosition.y, panelWidth, panelHeight);

        Color previous = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.55f);
        GUI.DrawTexture(panel, Texture2D.whiteTexture);

        float orbX = panel.x + pad;
        float orbY = panel.y + pad;

        for (int i = 0; i < wallet.MaxPoints; i++)
        {
            bool filled = i < wallet.Points;
            GUI.color = filled ? new Color(1f, 0.85f, 0.38f) : new Color(0.28f, 0.28f, 0.32f);
            GUI.DrawTexture(new Rect(orbX, orbY, size, size), Texture2D.whiteTexture);
            orbX += size + gap;
        }

        GUI.color = previous;

        if (showHint)
        {
            GUI.Label(
                new Rect(orbX + 12f * scale, panel.y + pad - 2f * scale, hintWidth, size + 4f * scale),
                HintText,
                _style);
        }
    }

    private Vector2 ResolvePosition(float width, float height, float scale)
    {
        Vector2 margin = offset * scale;
        float x;
        float y;

        switch (anchor)
        {
            case HudAnchor.TopLeft:
                x = margin.x;
                y = margin.y;
                break;

            case HudAnchor.TopCenter:
                x = (Screen.width - width) * 0.5f;
                y = margin.y;
                break;

            case HudAnchor.TopRight:
                x = Screen.width - width - margin.x;
                y = margin.y;
                break;

            case HudAnchor.BottomLeft:
                x = margin.x;
                y = Screen.height - height - margin.y;
                break;

            case HudAnchor.BottomRight:
                x = Screen.width - width - margin.x;
                y = Screen.height - height - margin.y;
                break;

            default:
                x = (Screen.width - width) * 0.5f;
                y = Screen.height - height - margin.y;
                break;
        }

        x = Mathf.Clamp(x, 0f, Mathf.Max(0f, Screen.width - width));
        y = Mathf.Clamp(y, 0f, Mathf.Max(0f, Screen.height - height));
        return new Vector2(x, y);
    }
}
