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
        Material sparkMaterial = GetOrCreateMaterial("M_FX_Spark", soft, new Color(1f, 0.86f, 0.55f, 0.85f), true);

        CreateCloudSea(cloudMaterial);
        CreateDust(sparkMaterial);
        CreateLandingBurst(sparkMaterial);
        AttachLandingComponent();

        AssetDatabase.SaveAssets();
        Debug.Log("[FX] 已创建：云海、漂浮微尘（随高度变色）、落地爆发");
    }

    private static void CreateCloudSea(Material material)
    {
        GameObject existing = GameObject.Find("FX_CloudSea");

        if (existing != null)
        {
            Object.DestroyImmediate(existing);
        }

        GameObject go = new GameObject("FX_CloudSea");
        go.transform.position = new Vector3(0f, -8f, 0f);

        ParticleSystem particles = go.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.loop = true;
        main.startLifetime = 45f;
        main.startSpeed = 0.08f;
        main.startSize = new ParticleSystem.MinMaxCurve(12f, 26f);
        main.startColor = new Color(0.85f, 0.87f, 0.95f, 0.18f);
        main.maxParticles = 90;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 2.5f;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(160f, 4f, 160f);

        ParticleSystem.NoiseModule noise = particles.noise;
        noise.enabled = true;
        noise.strength = 0.35f;
        noise.frequency = 0.08f;
        noise.scrollSpeed = 0.03f;

        ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.velocityScale = 4f;
        renderer.material = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        particles.Play();
    }

    private static void CreateDust(Material material)
    {
        GameObject existing = GameObject.Find("FX_Dust");

        if (existing != null)
        {
            Object.DestroyImmediate(existing);
        }

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
        main.startSpeed = 0.12f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.13f);
        main.startColor = new Color(1f, 0.75f, 0.4f, 0.9f);
        main.maxParticles = 180;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0f;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 14f;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 10f;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f),
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(1f, 0.2f),
                new GradientAlphaKey(1f, 0.8f),
                new GradientAlphaKey(0f, 1f),
            });
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        go.AddComponent<ParticleHeightTint>();
        particles.Play();
    }

    private static void CreateLandingBurst(Material material)
    {
        GameObject existing = GameObject.Find("FX_LandingBurst");

        if (existing != null)
        {
            Object.DestroyImmediate(existing);
        }

        GameObject go = new GameObject("FX_LandingBurst");

        ParticleSystem particles = go.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = 0.45f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.4f, 2.8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.16f);
        main.startColor = new Color(1f, 0.9f, 0.7f, 0.9f);
        main.maxParticles = 60;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.35f;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 14) });

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 32f;
        shape.radius = 0.12f;
        shape.rotation = new Vector3(-90f, 0f, 0f);

        ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = material;
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
                alpha = alpha * alpha;
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
