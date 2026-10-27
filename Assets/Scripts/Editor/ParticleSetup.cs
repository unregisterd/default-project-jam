using System.IO;
using UnityEditor;
using UnityEngine;

public static class ParticleSetup
{
    private const string FxFolder = "Assets/Art/FX";
    private const string MaterialFolder = "Assets/Art/Materials";
    private const string TexturePath = FxFolder + "/T_SoftCircle.png";

    [MenuItem("Tools/Environment/Add Particle Effects")]
    public static void Setup()
    {
        Texture2D soft = GetOrCreateSoftCircle();

        Material cloudMaterial = GetOrCreateMaterial("M_FX_Cloud", soft, new Color(0.82f, 0.84f, 0.92f, 0.10f), false);
        Material dustMaterial = GetOrCreateMaterial("M_FX_Dust", soft, new Color(0.9f, 0.94f, 1f, 0.75f), true);
        Material burstMaterial = GetOrCreateMaterial("M_FX_Burst", soft, new Color(0.92f, 0.94f, 1f, 0.8f), true);

        CreateCloudSea(cloudMaterial);
        CreateDust(dustMaterial);
        CreateLandingBurst(burstMaterial);
        AttachLandingComponent();

        AssetDatabase.SaveAssets();
        Debug.Log("[FX] 已创建：云海（圆形软雾）、漂浮微尘（冷白）、落地爆发（小颗粒）");
    }

    private static void CreateCloudSea(Material material)
    {
        RemoveIfExists("FX_CloudSea");

        GameObject go = new GameObject("FX_CloudSea");
        go.transform.position = new Vector3(0f, -22f, 0f);

        ParticleSystem particles = go.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.loop = true;
        main.startLifetime = 50f;
        main.startSpeed = 0.05f;
        main.startSize = new ParticleSystem.MinMaxCurve(7f, 16f);
        main.startColor = new Color(0.85f, 0.87f, 0.95f, 0.10f);
        main.maxParticles = 120;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 3.5f;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(160f, 4f, 160f);

        ParticleSystem.NoiseModule noise = particles.noise;
        noise.enabled = true;
        noise.strength = 0.3f;
        noise.frequency = 0.06f;
        noise.scrollSpeed = 0.02f;

        ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        particles.Play();
    }

    private static void CreateDust(Material material)
    {
        RemoveIfExists("FX_Dust");

        GameObject go = new GameObject("FX_Dust");
        GameObject pivot = GameObject.Find("CameraPivot");

        if (pivot != null)
        {
            go.transform.SetParent(pivot.transform, true);
            go.transform.position = pivot.transform.position;
        }

        ParticleSystem particles = go.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.loop = true;
        main.prewarm = true;
        main.startLifetime = 9f;
        main.startSpeed = 0.1f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.08f);
        main.startColor = new Color(0.92f, 0.95f, 1f, 0.85f);
        main.maxParticles = 160;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0f;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 16f;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 10f;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(1f, 0.25f),
                new GradientAlphaKey(1f, 0.75f),
                new GradientAlphaKey(0f, 1f),
            });
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        ParticleHeightTint tint = go.AddComponent<ParticleHeightTint>();
        SetTintColors(tint, new Color(1f, 0.95f, 0.85f), new Color(0.55f, 0.7f, 1f));

        particles.Play();
    }

    private static void CreateLandingBurst(Material material)
    {
        RemoveIfExists("FX_LandingBurst");

        GameObject go = new GameObject("FX_LandingBurst");

        ParticleSystem particles = go.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = 0.35f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 1.6f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.06f);
        main.startColor = new Color(0.9f, 0.93f, 1f, 0.85f);
        main.maxParticles = 40;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.4f;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 10) });

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 30f;
        shape.radius = 0.08f;
        shape.rotation = new Vector3(-90f, 0f, 0f);

        ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        particles.Stop();
    }

    private static void AttachLandingComponent()
    {
        GameObject player = GameObject.Find("Player");

        if (player == null)
        {
            return;
        }

        if (player.GetComponent<PlayerLandingParticles>() == null)
        {
            player.AddComponent<PlayerLandingParticles>();
        }
    }

    private static void SetTintColors(ParticleHeightTint tint, Color low, Color high)
    {
        SerializedObject serialized = new SerializedObject(tint);
        serialized.FindProperty("lowColor").colorValue = low;
        serialized.FindProperty("highColor").colorValue = high;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void RemoveIfExists(string objectName)
    {
        GameObject existing = GameObject.Find(objectName);

        if (existing != null)
        {
            Object.DestroyImmediate(existing);
        }
    }

    private static Texture2D GetOrCreateSoftCircle()
    {
        EnsureFolder(FxFolder);

        Texture2D existing = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);

        if (existing != null)
        {
            return existing;
        }

        const int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f) / size * 2f - 1f;
                float dy = (y + 0.5f) / size * 2f - 1f;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01(1f - distance);
                alpha = alpha * alpha * alpha;
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

    private static Material GetOrCreateMaterial(string name, Texture2D texture, Color color, bool additive)
    {
        EnsureFolder(MaterialFolder);
        string path = $"{MaterialFolder}/{name}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");

        if (shader == null)
        {
            Debug.LogError("[FX] 找不到 URP Particles/Unlit shader");
            return null;
        }

        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        material.SetTexture("_BaseMap", texture);
        material.SetTexture("_MainTex", texture);
        material.SetColor("_BaseColor", color);
        material.SetFloat("_Surface", 1f);
        material.SetFloat("_Blend", additive ? 1f : 0f);
        material.SetFloat("_ZWrite", 0f);
        material.SetFloat("_AlphaClip", 0f);
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", additive
            ? (int)UnityEngine.Rendering.BlendMode.One
            : (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
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
