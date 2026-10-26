using UnityEngine;

public class Hud_StarEnergy : MonoBehaviour
{
    [SerializeField] private AbilityWallet wallet;
    [SerializeField] private AbilityController controller;

    private GUIStyle _style;

    private void Awake()
    {
        if (wallet == null)
        {
            wallet = GetComponentInParent<AbilityWallet>();
        }

        if (controller == null)
        {
            controller = GetComponentInParent<AbilityController>();
        }
    }

    private void OnGUI()
    {
        if (wallet == null)
        {
            return;
        }

        if (_style == null)
        {
            _style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                alignment = TextAnchor.MiddleLeft,
            };
        }

        float size = 20f;
        float gap = 8f;
        float x = 16f;
        float y = Screen.height - 52f;

        Color previous = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.5f);
        GUI.DrawTexture(new Rect(8f, y - 10f, 16f + wallet.MaxPoints * (size + gap) + 330f, 42f), Texture2D.whiteTexture);
        GUI.color = previous;

        for (int i = 0; i < wallet.MaxPoints; i++)
        {
            bool filled = i < wallet.Points;
            GUI.color = filled ? new Color(1f, 0.85f, 0.38f) : new Color(0.28f, 0.28f, 0.32f);
            GUI.DrawTexture(new Rect(x, y, size, size), Texture2D.whiteTexture);
            x += size + gap;
        }

        GUI.color = previous;
        GUI.Label(new Rect(x + 6f, y - 2f, 320f, 24f), "1 Courage  2 Patience  3 Hope  4 Wisdom", _style);
    }
}
