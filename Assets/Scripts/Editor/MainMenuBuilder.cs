using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MainMenuBuilder
{
    private const string ScenePath = "Assets/Scenes/MainMenu.unity";
    private const string GameScenePath = "Assets/Scenes/Main.unity";
    private const string FontPath = "Assets/Resources/Fonts/SourceHanSansCN-VF-2 SDF.asset";
    private const string KnightPath = "Assets/Resources/UI/T_KnightSilhouette.png";
    private const string GlowPath = "Assets/Resources/UI/T_GlowSoft.png";

    private static readonly Color WarmWhite = new Color(1f, 0.95f, 0.88f);
    private static readonly Color WarmMuted = new Color(1f, 0.86f, 0.74f);

    [MenuItem("Tools/Menu/Build Main Menu")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        Scene scene;

        if (System.IO.File.Exists(ScenePath))
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }
        else
        {
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Object.DestroyImmediate(root);
        }

        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        Texture2D knight = AssetDatabase.LoadAssetAtPath<Texture2D>(KnightPath);
        Texture2D glow = AssetDatabase.LoadAssetAtPath<Texture2D>(GlowPath);

        CreateCamera();

        GameObject canvasObject = new GameObject("Menu Canvas", typeof(RectTransform));
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();

        CreateHaze(canvasRect, glow);

        RectTransform glowRect = CreateImage(canvasRect, "Knight Glow", glow, WarmMuted, new Vector2(0.5f, 0f),
            new Vector2(0f, -12f), new Vector2(340f, 340f));
        CanvasGroup glowGroup = glowRect.gameObject.AddComponent<CanvasGroup>();
        glowGroup.alpha = 0.35f;
        glowGroup.blocksRaycasts = false;

        RectTransform knightRect = CreateImage(canvasRect, "Knight", knight, new Color(0.09f, 0.08f, 0.12f, 1f),
            new Vector2(0.5f, 0f), new Vector2(0f, 46f), new Vector2(104f, 104f));
        knightRect.gameObject.AddComponent<CanvasGroup>().blocksRaycasts = false;

        CanvasGroup titleGroup = CreateTitle(canvasRect, font);
        CanvasGroup buttonGroup = CreateButtons(canvasRect, font, out Button startButton, out Button quitButton);

        GameObject fadeObject = new GameObject("Fade", typeof(RectTransform));
        RectTransform fadeRect = fadeObject.GetComponent<RectTransform>();
        fadeRect.SetParent(canvasRect, false);
        fadeRect.anchorMin = Vector2.zero;
        fadeRect.anchorMax = Vector2.one;
        fadeRect.offsetMin = Vector2.zero;
        fadeRect.offsetMax = Vector2.zero;
        Image fadeImage = fadeObject.AddComponent<Image>();
        fadeImage.color = Color.black;
        fadeImage.raycastTarget = false;
        CanvasGroup fadeGroup = fadeObject.AddComponent<CanvasGroup>();
        fadeGroup.alpha = 1f;
        fadeGroup.blocksRaycasts = false;

        CreateDust(glow);

        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem));
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        MainMenuManager manager = canvasObject.AddComponent<MainMenuManager>();

        SerializedObject so = new SerializedObject(manager);
        so.FindProperty("titleGroup").objectReferenceValue = titleGroup;
        so.FindProperty("buttonGroup").objectReferenceValue = buttonGroup;
        so.FindProperty("fadeCanvasGroup").objectReferenceValue = fadeGroup;
        so.FindProperty("startButton").objectReferenceValue = startButton;
        so.FindProperty("quitButton").objectReferenceValue = quitButton;
        so.FindProperty("knightRect").objectReferenceValue = knightRect;
        so.FindProperty("glowGroup").objectReferenceValue = glowGroup;
        so.FindProperty("glowRect").objectReferenceValue = glowRect;
        so.FindProperty("gameSceneName").stringValue = "Main";
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        RegisterBuildScenes();

        Debug.Log("[MainMenu] 构建完成：" + ScenePath);
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.Skybox;
        camera.fieldOfView = 50f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 200f;
        cameraObject.transform.position = new Vector3(0f, 1.6f, -8f);
        cameraObject.transform.rotation = Quaternion.identity;

        cameraObject.AddComponent<AudioListener>();
    }

    private static void CreateHaze(RectTransform parent, Texture2D glow)
    {
        RectTransform haze = CreateImage(parent, "Warm Haze", glow, new Color(1f, 0.78f, 0.6f, 0.16f),
            new Vector2(0.5f, 0f), new Vector2(0f, -130f), new Vector2(2200f, 620f));
        haze.gameObject.AddComponent<CanvasGroup>().blocksRaycasts = false;
    }

    private static CanvasGroup CreateTitle(RectTransform parent, TMP_FontAsset font)
    {
        GameObject root = new GameObject("Title Root", typeof(RectTransform));
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 190f);
        rect.sizeDelta = new Vector2(1400f, 320f);

        CanvasGroup group = root.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;

        TMP_Text title = CreateText(rect, "Title", "逐星", font, 132f, WarmWhite);
        RectTransform titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.sizeDelta = new Vector2(1400f, 200f);
        titleRect.anchoredPosition = new Vector2(0f, 48f);
        title.characterSpacing = 12f;

        TMP_Text subtitle = CreateText(rect, "Subtitle", "CHASING THE STAR", font, 30f, new Color(1f, 0.9f, 0.82f, 0.72f));
        RectTransform subtitleRect = subtitle.rectTransform;
        subtitleRect.anchorMin = new Vector2(0.5f, 0.5f);
        subtitleRect.anchorMax = new Vector2(0.5f, 0.5f);
        subtitleRect.pivot = new Vector2(0.5f, 0.5f);
        subtitleRect.sizeDelta = new Vector2(1400f, 60f);
        subtitleRect.anchoredPosition = new Vector2(0f, -50f);
        subtitle.characterSpacing = 26f;

        return group;
    }

    private static CanvasGroup CreateButtons(RectTransform parent, TMP_FontAsset font, out Button start, out Button quit)
    {
        GameObject root = new GameObject("Buttons", typeof(RectTransform));
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 155f);
        rect.sizeDelta = new Vector2(520f, 230f);

        CanvasGroup group = root.AddComponent<CanvasGroup>();
        group.alpha = 0f;

        start = CreateTextButton(rect, "Start Button", "开始旅途", font, 46f, WarmWhite, new Vector2(0f, 138f));
        quit = CreateTextButton(rect, "Quit Button", "退出", font, 38f, new Color(1f, 0.92f, 0.86f, 0.72f), new Vector2(0f, 54f));

        return group;
    }

    private static Button CreateTextButton(RectTransform parent, string name, string text, TMP_FontAsset font,
        float size, Color color, Vector2 position)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(420f, 78f);
        rect.anchoredPosition = position;

        Image hit = go.AddComponent<Image>();
        hit.color = new Color(1f, 1f, 1f, 0f);
        hit.raycastTarget = true;

        Button button = go.AddComponent<Button>();
        button.targetGraphic = hit;
        button.transition = Selectable.Transition.None;

        TMP_Text label = CreateText(rect, "Label", text, font, size, color);
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        label.characterSpacing = 6f;
        label.raycastTarget = false;

        return button;
    }

    private static TMP_Text CreateText(RectTransform parent, string name, string text, TMP_FontAsset font,
        float size, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.font = font;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        tmp.raycastTarget = false;

        if (font != null && font.material != null)
        {
            tmp.fontSharedMaterial = font.material;
        }

        return tmp;
    }

    private static RectTransform CreateImage(RectTransform parent, string name, Texture2D texture, Color color,
        Vector2 anchor, Vector2 position, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, anchor.y);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        RawImage image = go.AddComponent<RawImage>();
        image.texture = texture;
        image.color = color;
        image.raycastTarget = false;

        return rect;
    }

    private static void CreateDust(Texture2D glow)
    {
        GameObject dustObject = new GameObject("Dust");
        dustObject.transform.position = new Vector3(0f, 2.2f, -1f);

        ParticleSystem particles = dustObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(10f, 16f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.02f, 0.12f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.07f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.85f, 0.7f, 0.55f),
            new Color(1f, 0.95f, 0.9f, 0.25f));
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0f;
        main.maxParticles = 120;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 6f;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(16f, 7f, 8f);

        ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.06f, 0.06f);
        velocity.y = new ParticleSystem.MinMaxCurve(-0.02f, 0.03f);

        ParticleSystem.ColorOverLifetimeModule fade = particles.colorOverLifetime;
        fade.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(new Color(1f, 0.9f, 0.8f), 0.6f),
                new GradientColorKey(new Color(0.75f, 0.7f, 0.85f), 1f),
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(1f, 0.25f),
                new GradientAlphaKey(0.9f, 0.75f),
                new GradientAlphaKey(0f, 1f),
            });
        fade.color = gradient;

        ParticleSystemRenderer renderer = dustObject.GetComponent<ParticleSystemRenderer>();

        Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Dust.mat");

        if (material == null)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            {
                AssetDatabase.CreateFolder("Assets", "Materials");
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");

            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            material = new Material(shader);
            material.SetTexture("_BaseMap", glow);
            material.SetTexture("_MainTex", glow);
            AssetDatabase.CreateAsset(material, "Assets/Materials/M_Dust.mat");
        }

        renderer.sharedMaterial = material;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.alignment = ParticleSystemRenderSpace.View;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        particles.Play();
    }

    private static void RegisterBuildScenes()
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes
            .Where(s => s.path != ScenePath && s.path != GameScenePath)
            .ToList();

        scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));

        if (System.IO.File.Exists(GameScenePath))
        {
            scenes.Insert(1, new EditorBuildSettingsScene(GameScenePath, true));
        }

        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
