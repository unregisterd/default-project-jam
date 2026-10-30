using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class EscapeMenuBuilder
{
    private const string ScenePath = "Assets/Scenes/Main.unity";
    private const string FontPath = "Assets/Resources/Fonts/SourceHanSansCN-VF-2 SDF.asset";

    private static readonly Color DimColor = new Color(0.09f, 0.11f, 0.17f, 0.86f);
    private static readonly Color TextColor = new Color(1f, 1f, 1f, 0.8f);
    private static readonly Color BodyColor = new Color(1f, 0.96f, 0.9f, 0.9f);

    [MenuItem("Tools/Menu/Build Escape Menu")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        Scene scene = EditorSceneManager.GetActiveScene();

        if (scene.path != ScenePath)
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        GameObject existing = GameObject.Find("EscapeMenu");

        if (existing != null)
        {
            Object.DestroyImmediate(existing);
        }

        EnsureEventSystem();

        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

        GameObject canvasObject = new GameObject("EscapeMenu", typeof(RectTransform));
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();

        GameObject rootObject = new GameObject("Menu Root", typeof(RectTransform));
        RectTransform rootRect = rootObject.GetComponent<RectTransform>();
        rootRect.SetParent(canvasRect, false);
        Stretch(rootRect);
        CanvasGroup menuGroup = rootObject.AddComponent<CanvasGroup>();
        menuGroup.alpha = 0f;

        GameObject dimObject = new GameObject("Dim", typeof(RectTransform));
        RectTransform dimRect = dimObject.GetComponent<RectTransform>();
        dimRect.SetParent(rootRect, false);
        Stretch(dimRect);
        Image dim = dimObject.AddComponent<Image>();
        dim.color = DimColor;
        dim.raycastTarget = true;

        TMP_Text detail = CreateText(rootRect, "Detail", "把鼠标移到技能上，看看它能为骑士做什么。\n\n<b>Shift 冲刺</b>　奔跑时速度提升，起跳更远。",
            font, 30f, BodyColor, TextAlignmentOptions.TopLeft);
        RectTransform detailRect = detail.rectTransform;
        detailRect.anchorMin = new Vector2(0f, 0.5f);
        detailRect.anchorMax = new Vector2(0f, 0.5f);
        detailRect.pivot = new Vector2(0f, 0.5f);
        detailRect.sizeDelta = new Vector2(820f, 560f);
        detailRect.anchoredPosition = new Vector2(150f, 30f);
        detail.lineSpacing = 18f;

        RectTransform navRect = CreateContainer(rootRect, "Nav", new Vector2(1f, 0.5f), new Vector2(-150f, 30f), new Vector2(460f, 520f));

        string[] navLabels = { Loc.Text("esc_resume"), Loc.Text("esc_skills"), Loc.Text("esc_menu"), Loc.Text("esc_quit") };
        Button[] navButtons = new Button[4];
        Image[] navDiamonds = new Image[4];

        for (int i = 0; i < navLabels.Length; i++)
        {
            navButtons[i] = CreateItem(navRect, "Nav " + i, navLabels[i], font, 34f, TextColor,
                new Vector2(0f, 150f - i * 84f), new Vector2(420f, 64f), out navDiamonds[i]);
        }

        RectTransform skillsRect = CreateContainer(rootRect, "Skills", new Vector2(1f, 0.5f), new Vector2(-640f, 30f), new Vector2(290f, 420f));
        CanvasGroup skillGroup = skillsRect.gameObject.AddComponent<CanvasGroup>();
        skillGroup.alpha = 0f;
        skillGroup.blocksRaycasts = false;

        string[] skillLabels =
        {
            Loc.Text("esc_skill_courage"),
            Loc.Text("esc_skill_wisdom"),
            Loc.Text("esc_skill_patience"),
            Loc.Text("esc_skill_hope"),
            Loc.Text("esc_skill_shift"),
        };

        Button[] skillButtons = new Button[5];
        Image[] skillDiamonds = new Image[5];

        for (int i = 0; i < skillLabels.Length; i++)
        {
            skillButtons[i] = CreateItem(skillsRect, "Skill " + i, skillLabels[i], font, 24f,
                new Color(1f, 1f, 1f, 0.62f), new Vector2(0f, 150f - i * 72f), new Vector2(290f, 52f),
                out skillDiamonds[i]);
        }

        EscapeMenuController controller = canvasObject.AddComponent<EscapeMenuController>();

        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("menuGroup").objectReferenceValue = menuGroup;
        so.FindProperty("detailText").objectReferenceValue = detail;
        so.FindProperty("skillGroup").objectReferenceValue = skillGroup;
        so.FindProperty("resumeButton").objectReferenceValue = navButtons[0];
        so.FindProperty("skillsButton").objectReferenceValue = navButtons[1];
        so.FindProperty("mainMenuButton").objectReferenceValue = navButtons[2];
        so.FindProperty("quitButton").objectReferenceValue = navButtons[3];
        FillArray(so.FindProperty("navDiamonds"), navDiamonds);
        FillArray(so.FindProperty("skillButtons"), skillButtons);
        FillArray(so.FindProperty("skillDiamonds"), skillDiamonds);
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        Selection.activeGameObject = canvasObject;

        Debug.Log("[EscapeMenu] 已生成：Esc 打开/关闭，悬停“技能提示”展开四个技能，悬停技能在左侧显示说明。"
                  + "\n暂停会冻结时间、解锁鼠标、压低音乐并把 HUD 淡出。");
    }

    private static Button CreateItem(RectTransform parent, string name, string label, TMP_FontAsset font,
        float size, Color color, Vector2 position, Vector2 itemSize, out Image diamond)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(1f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(1f, 0.5f);
        rect.sizeDelta = itemSize;
        rect.anchoredPosition = position;

        Image hit = go.AddComponent<Image>();
        hit.color = new Color(1f, 1f, 1f, 0f);
        hit.raycastTarget = true;

        Button button = go.AddComponent<Button>();
        button.targetGraphic = hit;
        button.transition = Selectable.Transition.None;

        GameObject diamondObject = new GameObject("Diamond", typeof(RectTransform));
        RectTransform diamondRect = diamondObject.GetComponent<RectTransform>();
        diamondRect.SetParent(rect, false);
        diamondRect.anchorMin = new Vector2(0f, 0.5f);
        diamondRect.anchorMax = new Vector2(0f, 0.5f);
        diamondRect.pivot = new Vector2(0.5f, 0.5f);
        diamondRect.sizeDelta = new Vector2(10f, 10f);
        diamondRect.anchoredPosition = new Vector2(10f, 0f);
        diamondRect.localRotation = Quaternion.Euler(0f, 0f, 45f);
        diamond = diamondObject.AddComponent<Image>();
        diamond.color = new Color(1f, 0.85f, 0.63f, 0f);
        diamond.raycastTarget = false;

        TMP_Text text = CreateText(rect, "Label", label, font, size, color, TextAlignmentOptions.Left);
        RectTransform textRect = text.rectTransform;
        Stretch(textRect);
        textRect.offsetMin = new Vector2(34f, 0f);
        textRect.offsetMax = new Vector2(-8f, 0f);
        text.characterSpacing = 4f;

        return button;
    }

    private static RectTransform CreateContainer(RectTransform parent, string name, Vector2 anchor, Vector2 position, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private static TMP_Text CreateText(RectTransform parent, string name, string text, TMP_FontAsset font,
        float size, Color color, TextAlignmentOptions alignment)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.font = font;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.enableWordWrapping = true;
        tmp.raycastTarget = false;

        if (font != null && font.material != null)
        {
            tmp.fontSharedMaterial = font.material;
        }

        return tmp;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void FillArray<T>(SerializedProperty property, T[] values) where T : Object
    {
        if (property == null)
        {
            return;
        }

        property.arraySize = values.Length;

        for (int i = 0; i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem));
        eventSystem.AddComponent<StandaloneInputModule>();
    }
}
