using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LanguageSwitchBuilder
{
    private const string ScenePath = "Assets/Scenes/MainMenu.unity";
    private const string LeftSprite = "Assets/Resources/UI/T_JoyLeft.png";
    private const string RightSprite = "Assets/Resources/UI/T_JoyRight.png";

    [MenuItem("Tools/Menu/Add Language Switch")]
    public static void Add()
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

        GameObject canvasObject = GameObject.Find("Menu Canvas");

        if (canvasObject == null)
        {
            Debug.LogError("[Language] 找不到 Menu Canvas，先跑 Tools/Menu/Build Main Menu。");
            return;
        }

        Transform canvas = canvasObject.transform;
        Transform old = canvas.Find("Language Switch");

        if (old != null)
        {
            Object.DestroyImmediate(old.gameObject);
        }

        Texture2D left = AssetDatabase.LoadAssetAtPath<Texture2D>(LeftSprite);
        Texture2D right = AssetDatabase.LoadAssetAtPath<Texture2D>(RightSprite);

        GameObject container = new GameObject("Language Switch", typeof(RectTransform));
        RectTransform containerRect = container.GetComponent<RectTransform>();
        containerRect.SetParent(canvas, false);
        containerRect.anchorMin = new Vector2(1f, 0f);
        containerRect.anchorMax = new Vector2(1f, 0f);
        containerRect.pivot = new Vector2(1f, 0f);
        containerRect.anchoredPosition = new Vector2(-120f, 70f);
        containerRect.sizeDelta = new Vector2(140f, 56f);

        Button zhButton = CreateButton(containerRect, "Lang ZH", left, new Vector2(-74f, 0f));
        Button enButton = CreateButton(containerRect, "Lang EN", right, new Vector2(0f, 0f));

        MainMenuManager manager = canvasObject.GetComponent<MainMenuManager>();

        if (manager != null)
        {
            SerializedObject so = new SerializedObject(manager);
            so.FindProperty("languageZhButton").objectReferenceValue = zhButton;
            so.FindProperty("languageEnButton").objectReferenceValue = enButton;

            TMP_Text title = FindText("Title");
            TMP_Text subtitle = FindText("Subtitle");
            TMP_Text start = FindText("Label", "Start Button");
            TMP_Text quit = FindText("Label", "Quit Button");

            if (title != null) so.FindProperty("titleLabel").objectReferenceValue = title;
            if (subtitle != null) so.FindProperty("subtitleLabel").objectReferenceValue = subtitle;
            if (start != null) so.FindProperty("startLabel").objectReferenceValue = start;
            if (quit != null) so.FindProperty("quitLabel").objectReferenceValue = quit;

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        Selection.activeGameObject = container;

        Debug.Log("[Language] 主菜单右下角已加语言切换：◀ 中文 / ▶ English。选择会通过 PlayerPrefs 记住。");
    }

    private static Button CreateButton(RectTransform parent, string name, Texture2D icon, Vector2 position)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(1f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(1f, 0.5f);
        rect.sizeDelta = new Vector2(56f, 56f);
        rect.anchoredPosition = position;

        RawImage image = go.AddComponent<RawImage>();
        image.texture = icon;
        image.color = new Color(1f, 0.95f, 0.88f, 0.75f);
        image.raycastTarget = true;

        Button button = go.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;

        return button;
    }

    private static TMP_Text FindText(string name, string parentName = null)
    {
        if (!string.IsNullOrEmpty(parentName))
        {
            GameObject parent = GameObject.Find(parentName);

            if (parent != null)
            {
                Transform child = parent.transform.Find(name);

                if (child != null)
                {
                    return child.GetComponent<TMP_Text>();
                }
            }
        }

        GameObject go = GameObject.Find(name);
        return go != null ? go.GetComponent<TMP_Text>() : null;
    }
}
