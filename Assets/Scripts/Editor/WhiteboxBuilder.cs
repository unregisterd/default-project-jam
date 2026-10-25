using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public static class WhiteboxBuilder
{
    private const string MaterialFolder = "Assets/Art/Materials";
    private const string PrefabFolder = "Assets/Prefabs/Blocks";
    private const string SettingsFolder = "Assets/Settings";
    private const string TypeFolder = "Assets/Settings/BlockTypes";
    private const string PalettePath = SettingsFolder + "/BlockPalette.asset";
    private const string ColorSetPath = SettingsFolder + "/PlayerColorSet.asset";
    private const string PrefabPath = PrefabFolder + "/Block_Whitebox.prefab";
    private const string InputPath = SettingsFolder + "/PlayerControls.inputactions";

    private const float Grid = 3f;
    private const float BlockSize = 2f;
    private const float BaseThickness = 0.5f;

    private struct BlockSpec
    {
        public string Name;
        public float X;
        public float Z;
        public float TopY;
        public string Type;
        public bool CanRaise;
    }

    [MenuItem("Tools/Whitebox/Build Playable Whitebox")]
    public static void Build()
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

        InputActionAsset input = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
        if (input == null)
        {
            Debug.LogError($"[Whitebox] 找不到 {InputPath}，等 Unity 导入后再执行。");
            return;
        }

        if (!EditorUtility.DisplayDialog(
                "Whitebox",
                $"在场景「{scene.name}」生成可试玩白盒？\n会替换已有的 WhiteboxRoot / Player / Main Camera。",
                "生成",
                "取消"))
        {
            return;
        }

        EnsureFolder(MaterialFolder);
        EnsureFolder(PrefabFolder);
        EnsureFolder(SettingsFolder);
        EnsureFolder(TypeFolder);

        BlockPalette palette = CreatePalette();
        Dictionary<string, BlockType> types = CreateTypes(palette);
        PlayerColorSet colorSet = CreateColorSet(palette);
        GameObject prefab = CreatePrefab();

        Cleanup();

        Transform root = new GameObject("WhiteboxRoot").transform;
        Transform level = new GameObject("Level_01").transform;
        level.SetParent(root, false);

        List<Block> blocks = new List<Block>();

        foreach (BlockSpec spec in GetSpecs())
        {
            Block block = SpawnBlock(level, prefab, spec, types, palette);

            if (block != null)
            {
                blocks.Add(block);
            }
        }

        GameObject player = GameObject.Find("Player");

        if (player == null)
        {
            player = BuildPlayer(input, colorSet);
        }

        BuildRevealLogic(root, blocks, player);
        BuildDemoBlock(root);
        EnsureLight();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Selection.activeGameObject = player;

        Debug.Log("[Whitebox] 第一关生成完成：灰白世界 → 第一次跳跃红色显现 → 同色连跳 → 落点翻蓝 → 锁死块 → 终点。");
    }

    private static BlockSpec[] GetSpecs()
    {
        return new[]
        {
            Spec("G0_Spawn", 0f, 0f, 0.25f, "Fixed_Red", false),
            Spec("G1", 3f, 0f, 0.25f, "Fixed_Red", false),
            Spec("G2", 6f, 0f, 0.25f, "Fixed_Red", false),
            Spec("P1_Reveal", 9f, 0f, 0.25f, "Fixed_Red", true),
            Spec("P2_Loop", 12f, 0f, 0.25f, "Loop_RedBlue", true),
            Spec("P3_Blue", 15f, 0f, 0.25f, "Fixed_Blue", true),
            Spec("P4_Lock", 18f, 0f, 0.25f, "Lock_BlueRed", true),
            Spec("Goal", 21f, 0f, 0.25f, "Fixed_Red", false),
        };
    }

    private static BlockSpec Spec(string name, float x, float z, float topY, string type, bool canRaise)
    {
        return new BlockSpec
        {
            Name = name,
            X = x,
            Z = z,
            TopY = topY,
            Type = type,
            CanRaise = canRaise,
        };
    }

    private static Block SpawnBlock(Transform parent, GameObject prefab, BlockSpec spec, Dictionary<string, BlockType> types, BlockPalette palette)
    {
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        instance.name = spec.Name;

        float thickness = Mathf.Max(BaseThickness, spec.TopY);
        instance.transform.position = new Vector3(spec.X, spec.TopY - thickness * 0.5f, spec.Z);
        instance.transform.localScale = new Vector3(BlockSize, thickness, BlockSize);

        BlockType type = types[spec.Type];
        Block block = instance.GetComponent<Block>();

        if (block == null)
        {
            Debug.LogError($"[Whitebox] 预制体 {PrefabPath} 上没有 Block 组件。");
            return null;
        }

        SerializedObject serializedObject = new SerializedObject(block);
        SetObject(serializedObject, "type", type);
        SetObject(serializedObject, "palette", palette);
        SetBool(serializedObject, "canRaise", spec.CanRaise);
        SetBool(serializedObject, "startGray", true);
        serializedObject.ApplyModifiedPropertiesWithoutUndo();

        BlockColorDefinition definition = palette.GetOrDefault("Gray");
        Renderer targetRenderer = instance.GetComponentInChildren<Renderer>();

        if (definition != null && definition.material != null && targetRenderer != null)
        {
            targetRenderer.sharedMaterial = definition.material;
        }

        return block;
    }

    private static void BuildRevealLogic(Transform root, List<Block> blocks, GameObject player)
    {
        GameObject logic = new GameObject("LevelLogic");
        logic.transform.SetParent(root, false);

        BlockRevealOnFirstJump reveal = logic.AddComponent<BlockRevealOnFirstJump>();
        reveal.SetTargets(blocks.ToArray());

        PlayerInputReader input = player != null ? player.GetComponent<PlayerInputReader>() : null;

        if (input != null)
        {
            SerializedObject serializedObject = new SerializedObject(reveal);
            SetObject(serializedObject, "input", input);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void BuildDemoBlock(Transform root)
    {
        Material gold = GetOrCreateTransparentMaterial("M_Demo_Gold", new Color(1f, 0.82f, 0.35f, 0.35f));

        GameObject demo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        demo.name = "Demo_GoldBlock";
        demo.transform.SetParent(root, false);
        demo.transform.position = new Vector3(4.5f, 1.6f, -4f);
        demo.transform.localScale = new Vector3(1f, 1f, 1f);

        Object.DestroyImmediate(demo.GetComponent<Collider>());

        Renderer renderer = demo.GetComponent<Renderer>();

        if (renderer != null && gold != null)
        {
            renderer.sharedMaterial = gold;
        }

        demo.AddComponent<DemoBlockBob>();
    }

    private static GameObject BuildPlayer(InputActionAsset input, PlayerColorSet colorSet)
    {
        GameObject player = new GameObject("Player");
        player.transform.position = new Vector3(0f, 0.3f, 0f);
        player.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 1.3f;
        controller.radius = 0.3f;
        controller.center = new Vector3(0f, 0.65f, 0f);
        controller.slopeLimit = 50f;
        controller.stepOffset = 0.35f;
        controller.skinWidth = 0.02f;

        PlayerInputReader reader = player.AddComponent<PlayerInputReader>();
        reader.SetActions(input);

        player.AddComponent<FirstPersonMotor>();
        PlayerLook look = player.AddComponent<PlayerLook>();

        PlayerColorSelector selector = player.AddComponent<PlayerColorSelector>();
        selector.SetColorSet(colorSet);

        player.AddComponent<PlayerBlockAgent>();
        player.AddComponent<PlayerRaiseAbility>();
        player.AddComponent<Hud_Whitebox>();

        GameObject pivot = new GameObject("CameraPivot");
        pivot.transform.SetParent(player.transform, false);
        pivot.transform.localPosition = new Vector3(0f, 1.2f, 0f);

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(pivot.transform, false);

        Camera cameraComponent = cameraObject.AddComponent<Camera>();
        cameraComponent.fieldOfView = 70f;
        cameraComponent.nearClipPlane = 0.05f;
        cameraComponent.farClipPlane = 250f;
        cameraObject.AddComponent<AudioListener>();

        SerializedObject lookObject = new SerializedObject(look);
        SetObject(lookObject, "input", reader);
        SetObject(lookObject, "cameraPivot", pivot.transform);
        lookObject.ApplyModifiedPropertiesWithoutUndo();

        return player;
    }

    private static BlockPalette CreatePalette()
    {
        Material red = GetOrCreateMaterial("M_Block_Red", new Color(0.75f, 0.16f, 0.16f));
        Material blue = GetOrCreateMaterial("M_Block_Blue", new Color(0.16f, 0.36f, 0.85f));
        Material gray = GetOrCreateMaterial("M_Block_Gray", new Color(0.55f, 0.56f, 0.58f));

        BlockPalette palette = AssetDatabase.LoadAssetAtPath<BlockPalette>(PalettePath);

        if (palette == null)
        {
            palette = ScriptableObject.CreateInstance<BlockPalette>();
            AssetDatabase.CreateAsset(palette, PalettePath);
        }

        palette.SetColors(new[]
        {
            new BlockColorDefinition
            {
                id = "Red",
                displayName = "红",
                uiColor = new Color(0.85f, 0.3f, 0.28f),
                material = red,
            },
            new BlockColorDefinition
            {
                id = "Blue",
                displayName = "蓝",
                uiColor = new Color(0.35f, 0.55f, 0.95f),
                material = blue,
            },
            new BlockColorDefinition
            {
                id = "Gray",
                displayName = "灰",
                uiColor = new Color(0.55f, 0.56f, 0.58f),
                material = gray,
            },
        });

        EditorUtility.SetDirty(palette);
        AssetDatabase.SaveAssets();
        return palette;
    }

    private static Dictionary<string, BlockType> CreateTypes(BlockPalette palette)
    {
        Dictionary<string, BlockType> types = new Dictionary<string, BlockType>
        {
            ["Fixed_Red"] = GetOrCreateType("T_Fixed_Red", palette, new[] { "Red" }, false, "固定红，被踩不翻色"),
            ["Fixed_Blue"] = GetOrCreateType("T_Fixed_Blue", palette, new[] { "Blue" }, false, "固定蓝，被踩不翻色"),
            ["Loop_RedBlue"] = GetOrCreateType("T_Loop_RedBlue", palette, new[] { "Red", "Blue" }, true, "红蓝循环，可无限翻"),
            ["Loop_BlueRed"] = GetOrCreateType("T_Loop_BlueRed", palette, new[] { "Blue", "Red" }, true, "蓝红循环"),
            ["Lock_BlueRed"] = GetOrCreateType("T_Lock_BlueRed", palette, new[] { "Blue", "Red" }, false, "蓝→红后锁定"),
        };

        return types;
    }

    private static BlockType GetOrCreateType(string assetName, BlockPalette palette, string[] colors, bool loop, string note)
    {
        string path = $"{TypeFolder}/{assetName}.asset";
        BlockType type = AssetDatabase.LoadAssetAtPath<BlockType>(path);

        if (type == null)
        {
            type = ScriptableObject.CreateInstance<BlockType>();
            AssetDatabase.CreateAsset(type, path);
        }

        type.Configure(palette, colors, loop, note);
        EditorUtility.SetDirty(type);
        return type;
    }

    private static PlayerColorSet CreateColorSet(BlockPalette palette)
    {
        PlayerColorSet colorSet = AssetDatabase.LoadAssetAtPath<PlayerColorSet>(ColorSetPath);

        if (colorSet == null)
        {
            colorSet = ScriptableObject.CreateInstance<PlayerColorSet>();
            AssetDatabase.CreateAsset(colorSet, ColorSetPath);
        }

        colorSet.Configure(palette, new[] { "Red", "Blue" });
        EditorUtility.SetDirty(colorSet);
        AssetDatabase.SaveAssets();
        return colorSet;
    }

    private static GameObject CreatePrefab()
    {
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);

        if (existing != null)
        {
            return existing;
        }

        GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        temp.name = "Block_Whitebox";
        temp.transform.localScale = new Vector3(BlockSize, BaseThickness, BlockSize);
        temp.AddComponent<BlockView_Whitebox>();
        temp.AddComponent<Block>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, PrefabPath);
        Object.DestroyImmediate(temp);

        AssetDatabase.SaveAssets();
        return prefab;
    }

    private static void Cleanup()
    {
        DestroyIfExists("WhiteboxRoot");
    }

    private static void DestroyIfExists(string objectName)
    {
        GameObject existing = GameObject.Find(objectName);

        if (existing != null)
        {
            Object.DestroyImmediate(existing);
        }
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

    private static Material GetOrCreateMaterial(string materialName, Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            Debug.LogError("[Whitebox] 找不到 URP Lit shader。");
            return null;
        }

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

    private static Material GetOrCreateTransparentMaterial(string materialName, Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            Debug.LogError("[Whitebox] 找不到 URP Lit shader。");
            return null;
        }

        string path = $"{MaterialFolder}/{materialName}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        material.SetFloat("_Surface", 1f);
        material.SetFloat("_Blend", 0f);
        material.SetFloat("_AlphaClip", 0f);
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.DisableKeyword("_ALPHATEST_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        material.SetColor("_BaseColor", color);
        material.SetFloat("_Smoothness", 0.5f);
        material.SetFloat("_Metallic", 0f);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void SetObject(SerializedObject serializedObject, string propertyName, Object value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        if (property == null)
        {
            Debug.LogError($"[Whitebox] 字段 {propertyName} 不存在于 {serializedObject.targetObject.GetType().Name}。");
            return;
        }

        property.objectReferenceValue = value;
    }

    private static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        if (property == null)
        {
            Debug.LogError($"[Whitebox] 字段 {propertyName} 不存在于 {serializedObject.targetObject.GetType().Name}。");
            return;
        }

        property.boolValue = value;
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string parent = System.IO.Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
        string leaf = System.IO.Path.GetFileName(folderPath);

        if (string.IsNullOrEmpty(parent))
        {
            return;
        }

        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, leaf);
    }
}
