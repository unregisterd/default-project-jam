using UnityEngine;

public class Hud_Whitebox : MonoBehaviour
{
    [SerializeField] private PlayerColorSelector colorSelector;
    [SerializeField] private PlayerBlockAgent blockAgent;
    [SerializeField] private float width = 320f;

    private GUIStyle _labelStyle;

    private void Awake()
    {
        if (colorSelector == null)
        {
            colorSelector = GetComponentInParent<PlayerColorSelector>();
        }

        if (blockAgent == null)
        {
            blockAgent = GetComponentInParent<PlayerBlockAgent>();
        }
    }

    private void OnGUI()
    {
        if (_labelStyle == null)
        {
            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                alignment = TextAnchor.MiddleLeft,
            };
        }

        BlockColorDefinition selected = colorSelector != null ? colorSelector.Current : null;
        Block standing = blockAgent != null ? blockAgent.StandingBlock : null;
        BlockColorDefinition standingColor = standing != null ? standing.CurrentDefinition : null;

        Rect panel = new Rect(16f, 16f, width, 92f);
        Color previous = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.55f);
        GUI.DrawTexture(panel, Texture2D.whiteTexture);
        GUI.color = previous;

        DrawRow(panel.x + 12f, panel.y + 10f, "Selected (jump)", selected);
        DrawRow(panel.x + 12f, panel.y + 42f, "Standing", standingColor, standing != null ? standing.name : null);

        GUI.Label(new Rect(panel.x + 12f, panel.y + 66f, width - 24f, 20f), "Space jump / F raise / Esc cursor", _labelStyle);
    }

    private void DrawRow(float x, float y, string title, BlockColorDefinition definition, string blockName = null)
    {
        GUI.Label(new Rect(x, y, 130f, 20f), title, _labelStyle);

        Rect swatch = new Rect(x + 134f, y + 1f, 18f, 18f);
        Color previous = GUI.color;
        GUI.color = definition != null ? definition.uiColor : new Color(0.25f, 0.25f, 0.25f);
        GUI.DrawTexture(swatch, Texture2D.whiteTexture);
        GUI.color = previous;

        string text = definition != null ? definition.displayName : "none";

        if (!string.IsNullOrEmpty(blockName))
        {
            text += "  (" + blockName + ")";
        }
        GUI.Label(new Rect(x + 160f, y, 150f, 20f), text, _labelStyle);
    }
}
