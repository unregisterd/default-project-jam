using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public static class WhiteboxBuilder
{
    private const string MaterialFolder = "Assets/Art/Materials";
    private const string SettingsFolder = "Assets/Settings";
    private const string PalettePath = SettingsFolder + "/BlockPalette.asset";
    private const string InputAssetPath = SettingsFolder + "/PlayerControls.inputactions";
    private const string RootName = "WhiteboxRoot";

    private static readonly Color RedColor = new Color(0.78f, 0.18f, 0.18f);
    private static readonly Color BlueColor = new Color(0.16f, 0.36f, 0.85f);
    private static readonly Color GrayColor = new Color(0.55f, 0.56f, 0.58f);

    [MenuItem("Tools/Whitebox/Build Playable Whitebox")]
    public static void BuildWhitebox()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.isLoaded)
        {
            EditorUtility.DisplayDialog("Whitebox", "没有已打开的场景。", "好");
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        InputActionAsset inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputAssetPath);
        if (inputAsset == null)
        {
            Debug.LogError($"[Whitebox] 找不到 {InputAssetPath}，请等 Unity 导入后再执行。");
            return;
        }

        BlockPalette palette = EnsurePalette();
        if (palette == null)
        {
            return;
        }

        if (!EditorUtility.DisplayDialog(
                "Whitebox",
                $"将在当前场景「{scene.name}」生成白盒，已有的 WhiteboxRoot / Player 会被替换。",
                "生成",
                "取消"))
        {
            return;
        }

        Cleanup();

        Transform root = new GameObject(RootName).transform;
        BuildArena(root);
        BuildBlockArea(root, palette);
        GameObject player = BuildPlayer(inputAsset);
        EnsureLight();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Selection.activeGameObject = player;
        Debug.Log($"[Whitebox] 已生成到 {scene.name}。WASD 移动 / Shift 跑 / 空格跳 / 左键或 E 翻转方块 / Esc 释放鼠标。");
    }

    private static void Cleanup()
    {
        DestroyIfExists(RootName);
        DestroyIfExists("Player");
        DestroyIfExists("Main Camera");
    }

    private static void DestroyIfExists(string objectName)
    {
        GameObject existing = GameObject.Find(objectName);
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
        }
    }

    private static void BuildArena(Transform root)
    {
        Material gray = LoadMaterial("Whitebox_Gray");

        CreateBox(root, "Floor", new Vector3(0f, -0.5f, 0f), new Vector3(32f, 1f, 32f), gray, true);
        CreateBox(root, "Wall_North", new Vector3(0f, 2.5f, 16f), new Vector3(33f, 5f, 1f), gray, true);
        CreateBox(root, "Wall_South", new Vector3(0f, 2.5f, -16f), new Vector3(33f, 5f, 1f), gray, true);
        CreateBox(root, "Wall_East", new Vector3(16f, 2.5f, 0f), new Vector3(1f, 5f, 33f), gray, true);
        CreateBox(root, "Wall_West", new Vector3(-16f, 2.5f, 0f), new Vector3(1f, 5f, 33f), gray, true);

        CreateBox(root, "Step_1", new Vector3(-6f, 0.25f, -6f), new Vector3(4f, 0.5f, 3f), gray, true);
        CreateBox(root, "Step_2", new Vector3(-6f, 0.75f, -3.5f), new Vector3(4f, 1.5f, 2f), gray, true);
        CreateBox(root, "Step_3", new Vector3(-6f, 1.25f, -1.5f), new Vector3(4f, 2.5f, 1.5f), gray, true);

        CreateBox(root, "Platform_Low", new Vector3(7f, 1.2f, 6f), new Vector3(6f, 0.4f, 6f), gray, false);
        CreateBox(root, "Platform_High", new Vector3(-8f, 3.2f, 8f), new Vector3(5f, 0.4f, 5f), gray, false);
    }

    private static void BuildBlockArea(Transform root, BlockPalette palette)
    {
        Transform area = new GameObject("BlockArea").transform;
        area.SetParent(root, false);

        const int size = 8;
        const float spacing = 1.05f;
        Vector3 origin = new Vector3(-(size - 1) * spacing * 0.5f, 0.5f, 2f);

        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                BlockColor color = (x + z) % 2 == 0 ? BlockColor.Red : BlockColor.Blue;
                Vector3 position = origin + new Vector3(x * spacing, 0f, z * spacing);
                CreateBlock(area, $"Block_{x}_{z}", position, Vector3.one, color, palette);
            }
        }

        for (int i = 0; i < 3; i++)
        {
            BlockColor color = i % 2 == 0 ? BlockColor.Blue : BlockColor.Red;
            Vector3 position = new Vector3(9f, 0.5f + i * 1.02f, -6f);
            CreateBlock(area, $"Pillar_{i}", position, Vector3.one, color, palette);
        }
    }

    private static GameObject BuildPlayer(InputActionAsset inputAsset)
    {
        GameObject player = new GameObject("Player");
        player.transform.position = new Vector3(0f, 0.2f, -8f);

        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 1.8f;
        controller.radius = 0.3f;
        controller.center = new Vector3(0f, 0.9f, 0f);
        controller.slopeLimit = 50f;
        controller.stepOffset = 0.35f;

        PlayerInputReader input = player.AddComponent<PlayerInputReader>();
        input.SetActions(inputAsset);

        player.AddComponent<FirstPersonMotor>();
        PlayerLook look = player.AddComponent<PlayerLook>();
        BlockFlipper flipper = player.AddComponent<BlockFlipper>();

        GameObject pivot = new GameObject("CameraPivot");
        pivot.transform.SetParent(player.transform, false);
        pivot.transform.localPosition = new Vector3(0f, 1.65f, 0f);

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(pivot.transform, false);

        Camera cameraComponent = cameraObject.AddComponent<Camera>();
        cameraComponent.fieldOfView = 70f;
        cameraComponent.nearClipPlane = 0.05f;
        cameraComponent.farClipPlane = 250f;
        cameraObject.AddComponent<AudioListener>();

        SetObjectReference(look, "input", input);
        SetObjectReference(look, "cameraPivot", pivot.transform);
        SetObjectReference(flipper, "input", input);
        SetObjectReference(flipper, "viewCamera", cameraComponent);

        return player;
    }

    private static void EnsureLight()
    {
        if (Object.FindObjectOfType<Light>() != null)
        {
            return;
        }

        GameObject lightObject = new GameObject("Directional Light");
        Light lightComponent = lightObject.AddComponent<Light>();
        lightComponent.type = LightType.Directional;
        lightComponent.intensity = 1.1f;
        lightComponent.shadows = LightShadows.Soft;
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    private static BlockPalette EnsurePalette()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            Debug.LogError("[Whitebox] 找不到 Universal Render Pipeline/Lit shader。");
            return null;
        }

        EnsureFolder(MaterialFolder);
        EnsureFolder(SettingsFolder);

        Material red = GetOrCreateMaterial("Whitebox_Red", RedColor, shader);
        Material blue = GetOrCreateMaterial("Whitebox_Blue", BlueColor, shader);
        GetOrCreateMaterial("Whitebox_Gray", GrayColor, shader);

        BlockPalette palette = AssetDatabase.LoadAssetAtPath<BlockPalette>(PalettePath);
        if (palette == null)
        {
            palette = ScriptableObject.CreateInstance<BlockPalette>();
            AssetDatabase.CreateAsset(palette, PalettePath);
        }

        palette.SetEntries(new[]
        {
            new BlockPalette.Entry { color = BlockColor.Red, material = red },
            new BlockPalette.Entry { color = BlockColor.Blue, material = blue },
        });

        EditorUtility.SetDirty(palette);
        AssetDatabase.SaveAssets();
        return palette;
    }

    private static Material GetOrCreateMaterial(string materialName, Color color, Shader shader)
    {
        string path = $"{MaterialFolder}/{materialName}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }
        else if (material.shader != shader)
        {
            material.shader = shader;
        }

        material.SetColor("_BaseColor", color);
        material.SetFloat("_Smoothness", 0.2f);
        material.SetFloat("_Metallic", 0f);
        material.enableInstancing = true;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material LoadMaterial(string materialName)
    {
        return AssetDatabase.LoadAssetAtPath<Material>($"{MaterialFolder}/{materialName}.mat");
    }

    private static GameObject CreateBox(Transform parent, string objectName, Vector3 position, Vector3 scale, Material material, bool markStatic)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = objectName;
        box.transform.SetParent(parent, false);
        box.transform.position = position;
        box.transform.localScale = scale;

        if (material != null)
        {
            box.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        box.isStatic = markStatic;
        return box;
    }

    private static Block CreateBlock(Transform parent, string blockName, Vector3 position, Vector3 scale, BlockColor color, BlockPalette palette)
    {
        GameObject box = CreateBox(parent, blockName, position, scale, null, false);

        Block block = box.AddComponent<Block>();
        BlockVisual visual = box.AddComponent<BlockVisual>();
        visual.Palette = palette;
        visual.CollectRenderers();

        block.SetVisual(visual);
        block.SetInitialColor(color);

        if (palette.TryGetMaterial(color, out Material material))
        {
            box.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        return block;
    }

    private static void SetObjectReference(Object target, string fieldName, Object value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(fieldName);

        if (property == null)
        {
            Debug.LogError($"[Whitebox] {target.GetType().Name} 上找不到字段 {fieldName}。");
            return;
        }

        property.objectReferenceValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string parent = System.IO.Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
        string leaf = System.IO.Path.GetFileName(folderPath);

        if (!string.IsNullOrEmpty(parent))
        {
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
