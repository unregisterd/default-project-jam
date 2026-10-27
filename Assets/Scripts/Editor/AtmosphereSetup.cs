using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class AtmosphereSetup
{
    private const string ProfilePath = "Assets/Settings/AtmosphereProfile.asset";
    private static readonly Color FogColor = new Color(0.23f, 0.19f, 0.29f, 1f);

    [MenuItem("Tools/Environment/Setup Atmosphere")]
    public static void Setup()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 15f;
        RenderSettings.fogEndDistance = 50f;
        RenderSettings.fogColor = FogColor;

        foreach (Camera camera in Object.FindObjectsOfType<Camera>())
        {
            camera.farClipPlane = 100f;
            UniversalAdditionalCameraData data = camera.GetUniversalAdditionalCameraData();

            if (data != null)
            {
                data.renderPostProcessing = true;
            }
        }

        VolumeProfile profile = SetupProfile();
        Volume volume = Object.FindObjectOfType<Volume>();

        if (volume == null)
        {
            GameObject volumeObject = new GameObject("Global Volume");
            volume = volumeObject.AddComponent<Volume>();
            Undo.RegisterCreatedObjectUndo(volumeObject, "Create Global Volume");
        }

        volume.isGlobal = true;
        volume.priority = 0;
        volume.sharedProfile = profile;

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        SceneView.RepaintAll();

        Debug.Log("[Atmosphere] 雾 15/50（紫灰）、相机远裁剪 100、Global Volume + Bloom/Tonemapping/Vignette 已配置");
    }

    private static VolumeProfile SetupProfile()
    {
        if (AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath) != null)
        {
            AssetDatabase.DeleteAsset(ProfilePath);
        }

        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
        AssetDatabase.CreateAsset(profile, ProfilePath);

        Bloom bloom = GetOrAdd<Bloom>(profile);
        bloom.intensity.Override(0.55f);
        bloom.threshold.Override(0.9f);
        bloom.scatter.Override(0.55f);

        Tonemapping tonemapping = GetOrAdd<Tonemapping>(profile);
        tonemapping.mode.Override(TonemappingMode.ACES);

        Vignette vignette = GetOrAdd<Vignette>(profile);
        vignette.intensity.Override(0.3f);
        vignette.smoothness.Override(0.45f);

        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();
        return profile;
    }

    private static T GetOrAdd<T>(VolumeProfile profile) where T : VolumeComponent
    {
        if (profile.TryGet(out T existing))
        {
            return existing;
        }

        T component = profile.Add<T>(true);
        component.name = typeof(T).Name;
        component.hideFlags = HideFlags.HideInHierarchy;
        AssetDatabase.AddObjectToAsset(component, profile);
        return component;
    }
}
