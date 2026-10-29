using System.IO;
using UnityEditor;
using UnityEngine;

public static class RippleSetup
{
    private const string ResourceFolder = "Assets/Resources/FX";
    private const string TexturePath = ResourceFolder + "/T_Ring.png";
    private const string MaterialPath = ResourceFolder + "/M_Ripple.mat";
    private const string InkTexturePath = ResourceFolder + "/T_RippleInk.png";

    [MenuItem("Tools/FX/Use Ink Ripple Texture")]
    public static void UseInkTexture()
    {
        Texture2D ink = AssetDatabase.LoadAssetAtPath<Texture2D>(InkTexturePath);

        if (ink == null)
        {
            Debug.LogError($"[Ripple] 找不到 {InkTexturePath}，先把水墨图放进去。");
            return;
        }

        TextureImporter importer = AssetImporter.GetAtPath(InkTexturePath) as TextureImporter;

        if (importer != null)
        {
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = true;
            importer.SaveAndReimport();
        }

        Material material = GetOrCreateMaterial(ink);
        material.SetTexture("_BaseMap", ink);
        material.SetColor("_BaseColor", Color.white);
        EditorUtility.SetDirty(material);
        AssetDatabase.SaveAssets();

        Debug.Log("[Ripple] 已把水墨圈贴图应用到 M_Ripple（加法混合，白墨发光）");
    }
    [MenuItem("Tools/FX/Build Crystal Ripple Materials")]
    public static void BuildCrystalRippleMaterials()
    {
        BuildCrystalMaterial("T_RipplePlayer.png", "M_RipplePlayer");
        BuildCrystalMaterial("T_RippleNpc.png", "M_RippleNpc");
        AssetDatabase.SaveAssets();
        Debug.Log("[Ripple] 已生成 M_RipplePlayer / M_RippleNpc（Alpha 混合）");
    }

    private static void BuildCrystalMaterial(string textureName, string materialName)
    {
        string texturePath = $"{ResourceFolder}/{textureName}";
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

        if (texture == null)
        {
            Debug.LogError($"[Ripple] 找不到 {texturePath}");
            return;
        }

        TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;

        if (importer != null)
        {
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = true;
            importer.SaveAndReimport();
        }

        string path = $"{ResourceFolder}/{materialName}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");

        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }
        else
        {
            material.shader = shader;
        }

        material.SetTexture("_BaseMap", texture);
        material.SetTexture("_MainTex", texture);
        material.SetColor("_BaseColor", Color.white);
        material.SetFloat("_Surface", 1f);
        material.SetFloat("_Blend", 0f);
        material.SetFloat("_ZWrite", 0f);
        material.SetFloat("_AlphaClip", 0f);
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        material.enableInstancing = true;
        EditorUtility.SetDirty(material);
    }
    [MenuItem("Tools/FX/Setup Ripple")]
    public static void Setup()
    {
        EnsureFolder(ResourceFolder);
        Texture2D ring = GetOrCreateRing();
        GetOrCreateMaterial(ring);

        AssetDatabase.SaveAssets();
        Debug.Log("[Ripple] 已生成 Resources/FX/T_Ring.png 与 M_Ripple.mat（波纹特效可直接使用）");
    }

    private static Texture2D GetOrCreateRing()
    {
        Texture2D existing = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);

        if (existing != null)
        {
            return existing;
        }

        const int size = 256;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f) / size * 2f - 1f;
                float dy = (y + 0.5f) / size * 2f - 1f;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);

                float inner = 0.72f;
                float thickness = 0.16f;
                float band = 1f - Mathf.Clamp01(Mathf.Abs(distance - inner) / thickness);
                float alpha = Mathf.Pow(band, 1.6f);

                if (distance > 0.98f)
                {
                    alpha = 0f;
                }

                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        File.WriteAllBytes(TexturePath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);

        AssetDatabase.ImportAsset(TexturePath);

        TextureImporter importer = AssetImporter.GetAtPath(TexturePath) as TextureImporter;

        if (importer != null)
        {
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = true;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
    }

    private static Material GetOrCreateMaterial(Texture2D ring)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");

        if (shader == null)
        {
            Debug.LogError("[Ripple] 找不到 URP Unlit shader");
            return null;
        }

        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, MaterialPath);
        }

        material.SetTexture("_BaseMap", ring);
        material.SetColor("_BaseColor", new Color(1f, 1f, 1f, 0.5f));
        material.SetFloat("_Surface", 1f);
        material.SetFloat("_Blend", 0f);
        material.SetFloat("_ZWrite", 0f);
        material.SetFloat("_AlphaClip", 0f);
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        material.enableInstancing = true;

        EditorUtility.SetDirty(material);
        return material;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        string leaf = Path.GetFileName(path);

        if (!string.IsNullOrEmpty(parent))
        {
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
