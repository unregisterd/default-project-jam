using UnityEngine;

public class Hud_StarEnergy : MonoBehaviour
{
    // 【修复报错】将枚举放在类内部，完美解决找不到或重复定义的问题
    public enum HudAnchor
    {
        TopLeft = 0,
        TopCenter = 1,
        TopRight = 2,
        BottomLeft = 3,
        BottomCenter = 4,
        BottomRight = 5,
    }

    // 【还原】完全保留你原本的文字和顺序！
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

    private GUIStyle _style;
    private bool _visible = true;
    private Texture2D _starTexture;
    private static Hud_StarEnergy _active;

    private void Awake()
    {
        if (_active != null && _active != this)
        {
            // 【还原】只销毁脚本，不销毁整个物体
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

        _starTexture = Resources.Load<Texture2D>("UI/T_StarEnergy");
    }

    private void Start()
    {
        if (wallet == null)
        {
            wallet = FindObjectOfType<AbilityWallet>();
        }

        if (wallet == null)
        {
            Debug.LogError("[Hud_StarEnergy] 找不到 AbilityWallet，星光条无法显示。", this);
        }
    }

    private void OnDestroy()
    {
        if (_active == this)
        {
            _active = null;
        }
    }

    public void SetVisible(bool visible)
    {
        _visible = visible;
    }

    private void OnGUI()
    {
        if (!_visible)
        {
            return;
        }

        if (wallet == null)
        {
            wallet = FindObjectOfType<AbilityWallet>();
        }

        if (wallet == null)
        {
            return;
        }

        float screenScale = Mathf.Min(Screen.height / 1080f, Screen.width / 1920f);
        float scale = Mathf.Clamp(screenScale * scaleMultiplier, 0.01f, 2.5f);

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
        float contentWidth = orbsWidth;
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
            GUI.color = filled ? Color.white : new Color(0.22f, 0.22f, 0.26f, 0.55f);

            if (_starTexture != null)
            {
                GUI.DrawTexture(new Rect(orbX, orbY, size, size), _starTexture, ScaleMode.ScaleToFit, true);
            }
            else
            {
                GUI.DrawTexture(new Rect(orbX, orbY, size, size), Texture2D.whiteTexture);
            }

            orbX += size + gap;
        }

        GUI.color = previous;
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