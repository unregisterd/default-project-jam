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

    [MenuItem("Tools/Menu/Add Language Toggle")]
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

        foreach (string name in new[] { "Language Switch", "Language Toggle" })
        {
            Transform old = canvas.Find(name);

            if (old != null)
            {
                Object.DestroyImmediate(old.gameObject);
            }
        }

        Texture2D left = AssetDatabase.LoadAssetAtPath<Texture2D>(LeftSprite);
        Texture2D right = AssetDatabase.LoadAssetAtPath<Texture2D>(RightSprite);

        GameObject go = new GameObject("Language Toggle", typeof(RectTransform));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(canvas, false);
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = new Vector2(-90f, 70f);
        rect.sizeDelta = new Vector2(64f, 64f);

        RawImage icon = go.AddComponent<RawImage>();
        icon.texture = Loc.Language == "en" ? right : left;
        icon.color = new Color(1f, 0.95f, 0.88f, 0.8f);
        icon.raycastTarget = true;

        Button button = go.AddComponent<Button>();
        button.targetGraphic = icon;
        button.transition = Selectable.Transition.None;

        MainMenuManager manager = canvasObject.GetComponent<MainMenuManager>();

        if (manager != null)
        {
            SerializedObject so = new SerializedObject(manager);
            so.FindProperty("languageToggleButton").objectReferenceValue = button;
            so.FindProperty("languageToggleIcon").objectReferenceValue = icon;
            so.FindProperty("languageEnIcon").objectReferenceValue = right;
            so.FindProperty("languageZhIcon").objectReferenceValue = left;

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
        Selection.activeGameObject = go;

        Debug.Log("[Language] 主菜单右下角已加语言开关：默认 ▶（English），点击翻转为 ◀（中文），再点翻回。选择会记住。");
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
