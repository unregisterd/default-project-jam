using System.IO;
using UnityEditor;
using UnityEngine;

public static class BlockArtPipeline
{
    private const string BlocksRoot = "Assets/Art/Blocks";
    private const string MaterialFolder = "Assets/Art/Materials";
    private const string PrefabFolder = "Assets/Prefabs/Blocks";

    private const string PalettePath = "Assets/Settings/BlockPalette.asset";

    [MenuItem("Tools/Art/Build Art Materials")]
    public static void BuildArtMaterials()
    {
        EnsureFolder(MaterialFolder);

        foreach (string color in new[] { "Red", "Blue" })
        {
            BuildMaterial(color);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[ArtPipe] 材质生成完成：M_Block_Red_Art / M_Block_Blue_Art");
    }

    [MenuItem("Tools/Art/Build Block Art Prefabs")]
    public static void BuildArtPrefabs()
    {
        BuildArtPrefab("Blue");
        BuildArtPrefab("Red");
        AssetDatabase.SaveAssets();
        Debug.Log($"[ArtPipe] 已生成 {PrefabFolder}/Block_Art_Blue.prefab 与 Block_Art_Red.prefab");
    }

    private static void BuildArtPrefab(string color)
    {
        string modelPath = $"{BlocksRoot}/{color}/SM_Block_{color}.fbx";
        GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);

        if (modelAsset == null)
        {
            Debug.LogError($"[ArtPipe] 找不到模型 {modelPath}");
            return;
        }

        GameObject root = new GameObject($"Block_Art_{color}");

        GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
        model.name = "Model";
        model.transform.SetParent(root.transform, false);

        Bounds bounds = CalculateBounds(model);
        Vector3 size = bounds.size;

        model.transform.localScale = new Vector3(
            1f / Mathf.Max(size.x, 0.0001f),
            1f / Mathf.Max(size.y, 0.0001f),
            1f / Mathf.Max(size.z, 0.0001f));

        model.transform.localPosition = -bounds.center;

        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.center = Vector3.zero;
        collider.size = Vector3.one;

        Block block = root.AddComponent<Block>();
        BlockView_Whitebox view = root.AddComponent<BlockView_Whitebox>();
        view.CollectRenderers();
        block.SetView(view);

        BlockPalette palette = AssetDatabase.LoadAssetAtPath<BlockPalette>(PalettePath);

        if (palette != null)
        {
            block.SetPalette(palette);
        }

        Material artMaterial = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialFolder}/M_Block_{color}_Art.mat");
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);

        if (artMaterial != null)
        {
            foreach (Renderer renderer in renderers)
            {
                renderer.sharedMaterial = artMaterial;
            }
        }

        EnsureFolder(PrefabFolder);
        string prefabPath = $"{PrefabFolder}/Block_Art_{color}.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
    }
    [MenuItem("Tools/Art/Build Born Prefab")]
    public static void BuildBornPrefab()
    {
        const string modelPath = "Assets/Art/Props/Born/SM_Born.fbx";
        const string prefabPath = "Assets/Prefabs/Props/Born.prefab";

        GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);

        if (modelAsset == null)
        {
            Debug.LogError($"[ArtPipe] 找不到 {modelPath}");
            return;
        }

        GameObject root = new GameObject("Born");

        GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
        model.name = "Model";
        model.transform.SetParent(root.transform, false);

        Bounds bounds = CalculateBounds(model);
        Vector3 size = bounds.size;

        model.transform.position -= new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);

        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.center = new Vector3(0f, size.y * 0.5f, 0f);
        collider.size = size;

        Material material = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialFolder}/M_Born.mat");
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);

        if (material != null)
        {
            foreach (Renderer renderer in renderers)
            {
                renderer.sharedMaterial = material;
            }
        }

        EnsureFolder("Assets/Prefabs/Props");
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();

        Debug.Log($"[ArtPipe] 已生成 {prefabPath}：尺寸 {size.x:F2} × {size.y:F2} × {size.z:F2} m，pivot 在底面中心，碰撞体已贴合");
    }
    [MenuItem("Tools/Art/Build Prop Materials")]
    public static void BuildPropMaterials()
    {
        const string propsRoot = "Assets/Art/Props";

        if (!AssetDatabase.IsValidFolder(propsRoot))
        {
            Debug.LogWarning($"[ArtPipe] 没有 {propsRoot} 目录");
            return;
        }

        EnsureFolder(MaterialFolder);

        foreach (string folder in AssetDatabase.GetSubFolders(propsRoot))
        {
            string propName = Path.GetFileName(folder);
            Texture2D baseColor = FindTexture(folder, "basecolor");
            Texture2D normal = FindTexture(folder, "normal");
            Texture2D roughness = FindTexture(folder, "roughness");

            Material material = BuildPropMaterial($"M_{propName}", baseColor, normal, roughness);

            if (material != null)
            {
                Debug.Log($"[ArtPipe] 已生成 {AssetDatabase.GetAssetPath(material)}（baseColor={(baseColor != null)} normal={(normal != null)}）");
            }
        }

        AssetDatabase.SaveAssets();
    }

    private static Material BuildPropMaterial(string materialName, Texture2D baseColor, Texture2D normal, Texture2D roughness)
    {
        string path = $"{MaterialFolder}/{materialName}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            Debug.LogError("[ArtPipe] 找不到 URP Lit shader");
            return null;
        }

        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        if (baseColor != null)
        {
            material.SetTexture("_BaseMap", baseColor);
            material.SetColor("_BaseColor", Color.white);
        }

        if (normal != null)
        {
            material.SetTexture("_BumpMap", normal);
            material.EnableKeyword("_NORMALMAP");
        }

        float smoothness = roughness != null ? Mathf.Clamp01(1f - SampleAverage(roughness).r) : 0.4f;
        material.SetFloat("_Smoothness", smoothness);
        material.SetFloat("_Metallic", 0f);
        material.enableInstancing = true;
        EditorUtility.SetDirty(material);
        return material;
    }
    [MenuItem("Tools/Art/Enable Emission (Block Materials)")]
    public static void EnableEmissionOnMaterials()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { MaterialFolder });
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material == null || !material.HasProperty("_EmissionColor"))
            {
                continue;
            }

            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", Color.black);
            EditorUtility.SetDirty(material);
            count++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[ArtPipe] 已为 {count} 个材质开启自发光（颜色为黑，不影响外观）");
    }
    [MenuItem("Tools/Art/Palette -> Art Materials")]
    public static void SwitchPaletteToArt()
    {
        SetPaletteSuffix("_Art");
    }

    [MenuItem("Tools/Art/Palette -> Flat Materials")]
    public static void SwitchPaletteToFlat()
    {
        SetPaletteSuffix(string.Empty);
    }

    private static void SetPaletteSuffix(string suffix)
    {
        BlockPalette palette = AssetDatabase.LoadAssetAtPath<BlockPalette>(PalettePath);

        if (palette == null)
        {
            Debug.LogError($"[ArtPipe] 找不到 {PalettePath}");
            return;
        }

        BlockColorDefinition[] colors = palette.Colors;

        foreach (BlockColorDefinition definition in colors)
        {
            string materialPath = $"{MaterialFolder}/M_Block_{definition.id}{suffix}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

            if (material == null)
            {
                Debug.LogWarning($"[ArtPipe] 缺少材质 {materialPath}，{definition.id} 保持不变");
                continue;
            }

            definition.material = material;
        }

        palette.SetColors(colors);
        EditorUtility.SetDirty(palette);
        AssetDatabase.SaveAssets();
        Debug.Log($"[ArtPipe] 调色板已切换到 {(string.IsNullOrEmpty(suffix) ? "纯色" : "美术")} 材质");
    }

    private static void BuildMaterial(string color)
    {
        string folder = $"{BlocksRoot}/{color}";
        string materialPath = $"{MaterialFolder}/M_Block_{color}_Art.mat";

        Texture2D baseColor = FindTexture(folder, "basecolor");
        Texture2D normal = FindTexture(folder, "normal");
        Texture2D metallic = FindTexture(folder, "metallic");
        Texture2D roughness = FindTexture(folder, "roughness");

        float metallicValue = metallic != null ? SampleAverage(metallic).r : 0f;
        float roughnessValue = roughness != null ? SampleAverage(roughness).r : 0.6f;

        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");

            if (shader == null)
            {
                Debug.LogError("[ArtPipe] 找不到 URP Lit shader");
                return;
            }

            material = new Material(shader);
            AssetDatabase.CreateAsset(material, materialPath);
        }

        if (baseColor != null)
        {
            material.SetTexture("_BaseMap", baseColor);
            material.SetColor("_BaseColor", Color.white);
        }

        if (normal != null)
        {
            material.SetTexture("_BumpMap", normal);
            material.EnableKeyword("_NORMALMAP");
        }

        material.SetFloat("_Metallic", Mathf.Clamp01(metallicValue));
        material.SetFloat("_Smoothness", Mathf.Clamp01(1f - roughnessValue));
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", Color.black);
        material.enableInstancing = true;
        EditorUtility.SetDirty(material);

        Debug.Log(
            $"[ArtPipe] {materialPath}  baseColor={(baseColor != null)} normal={(normal != null)} "
            + $"metallic={metallicValue:F2} smoothness={(1f - roughnessValue):F2}");
    }

    private static Texture2D FindTexture(string folder, string keyword)
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (Path.GetFileNameWithoutExtension(path).ToLowerInvariant().Contains(keyword))
            {
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
        }

        return null;
    }

    private static Color SampleAverage(Texture2D texture)
    {
        string path = AssetDatabase.GetAssetPath(texture);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

        if (importer == null)
        {
            return new Color(0.5f, 0.5f, 0.5f, 1f);
        }

        bool wasReadable = importer.isReadable;

        if (!wasReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
            texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        Color average = new Color(0f, 0f, 0f, 0f);

        if (texture != null)
        {
            Color[] pixels = texture.GetPixels();

            for (int i = 0; i < pixels.Length; i++)
            {
                average += pixels[i];
            }

            average /= Mathf.Max(1, pixels.Length);
        }

        if (!wasReadable)
        {
            importer.isReadable = false;
            importer.SaveAndReimport();
        }

        return average;
    }

    private static Bounds CalculateBounds(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);

        if (renderers.Length == 0)
        {
            return new Bounds(Vector3.zero, Vector3.one);
        }

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        Vector3 center = bounds.center - root.transform.position;
        return new Bounds(center, bounds.size);
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
        string leaf = Path.GetFileName(folderPath);

        if (string.IsNullOrEmpty(parent))
        {
            return;
        }

        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, leaf);
    }
}
