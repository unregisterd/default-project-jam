using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class HintUiSetup
{
    private const string FontResourcePath = "Fonts/SourceHanSansCN-VF-2 SDF";
    private static readonly Color HintColor = new Color(0.95f, 0.91f, 0.85f, 1f);
    private static readonly Color ControlsColor = new Color(0.85f, 0.81f, 0.75f, 0.9f);

    [MenuItem("Tools/UI/Create Hint Canvas")]
    public static void CreateHintCanvas()
    {
        GameObject existing = GameObject.Find("HintCanvas");

        if (existing != null)
        {
            Object.DestroyImmediate(existing);
        }

        GameObject canvasObject = new GameObject("HintCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        TMP_FontAsset font = Resources.Load<TMP_FontAsset>(FontResourcePath);

        if (font == null)
        {
            font = TMP_Settings.defaultFontAsset;
        }

        TextMeshProUGUI hint = CreateText(
            canvasObject.transform,
            "HintText",
            font,
            new Vector2(0.5f, 0f),
            new Vector2(0f, 150f),
            new Vector2(1400f, 240f),
            34f,
            TextAlignmentOptions.Center,
            HintColor);

        TextMeshProUGUI controls = CreateText(
            canvasObject.transform,
            "ControlsText",
            font,
            new Vector2(0f, 0f),
            new Vector2(48f, 44f),
            new Vector2(1100f, 40f),
            22f,
            TextAlignmentOptions.Left,
            ControlsColor);

        GameObject fadeObject = new GameObject("FadePanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        fadeObject.transform.SetParent(canvasObject.transform, false);
        fadeObject.transform.SetAsFirstSibling();

        RectTransform fadeRect = fadeObject.GetComponent<RectTransform>();
        fadeRect.anchorMin = Vector2.zero;
        fadeRect.anchorMax = Vector2.one;
        fadeRect.offsetMin = Vector2.zero;
        fadeRect.offsetMax = Vector2.zero;

        Image fadeImage = fadeObject.GetComponent<Image>();
        fadeImage.color = Color.black;
        fadeImage.raycastTarget = false;

        CanvasGroup fadeGroup = fadeObject.GetComponent<CanvasGroup>();
        fadeGroup.alpha = 0f;
        fadeGroup.blocksRaycasts = false;
        fadeGroup.interactable = false;

        ScreenFader fader = canvasObject.AddComponent<ScreenFader>();
        SerializedObject faderSerialized = new SerializedObject(fader);
        faderSerialized.FindProperty("group").objectReferenceValue = fadeGroup;
        faderSerialized.ApplyModifiedPropertiesWithoutUndo();
        UITextManager manager = canvasObject.AddComponent<UITextManager>();

        SerializedObject serialized = new SerializedObject(manager);
        serialized.FindProperty("hintText").objectReferenceValue = hint;
        serialized.FindProperty("controlsText").objectReferenceValue = controls;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        Selection.activeGameObject = canvasObject;
        Debug.Log("[UI] 已创建 HintCanvas（1920×1080 自适应，TMP 打字机 + 淡入淡出，含按键提示）");
    }

    private static TextMeshProUGUI CreateText(
        Transform parent,
        string name,
        TMP_FontAsset font,
        Vector2 anchor,
        Vector2 anchoredPosition,
        Vector2 size,
        float fontSize,
        TextAlignmentOptions alignment,
        Color color)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(anchor.x, anchor.y);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
        text.text = string.Empty;

        Material material = text.fontMaterial;
        material.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0f, 0f, 0f, 0.5f));
        material.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 1f);
        material.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -1f);
        material.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.15f);
        material.EnableKeyword("UNDERLAY_ON");

        return text;
    }
}
