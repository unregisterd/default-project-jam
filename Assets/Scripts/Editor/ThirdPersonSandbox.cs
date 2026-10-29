using Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ThirdPersonSandbox
{
    private const string ScenePath = "Assets/Scenes/Sandbox/ThirdPersonTest.unity";
    private const string PlayerPrefabPath = "Assets/StarterAssets/ThirdPersonController/Prefabs/PlayerArmature.prefab";
    private const string CameraPrefabPath = "Assets/StarterAssets/ThirdPersonController/Prefabs/PlayerFollowCamera.prefab";
    private const string BlueBlockPath = "Assets/Prefabs/Blocks/Block_Art_Blue.prefab";
    private const string RedBlockPath = "Assets/Prefabs/Blocks/Block_Art_Red.prefab";
    private const string SkyMaterialPath = "Assets/Materials/M_Sky_Dusk.mat";

    [MenuItem("Tools/Sandbox/Build Third Person Test")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        Scene scene = System.IO.File.Exists(ScenePath)
            ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Object.DestroyImmediate(root);
        }

        SetupSky();
        CreateLight();
        CreateGround();

        GameObject player = InstantiatePrefab(PlayerPrefabPath, "PlayerArmature", new Vector3(0f, 0.2f, -6f), Quaternion.identity);

        if (player == null)
        {
            Debug.LogError("[Sandbox] 找不到 PlayerArmature 预制体，请确认 Starter Assets 已完整导入。");
            return;
        }

        Transform cameraRoot = FindDeepChild(player.transform, "PlayerCameraRoot");
        GameObject cameraObject = CreateFollowCamera(cameraRoot);

        if (cameraObject == null)
        {
            Debug.LogWarning("[Sandbox] 找不到 PlayerFollowCamera 预制体，请手动放一个 Cinemachine 相机。");
        }

        BuildCourse();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);

        Selection.activeGameObject = player;

        Debug.Log("[Sandbox] 第三人称测试场景已生成：" + ScenePath
                  + "\n1m 方块 / 1.5m 网格 / 台阶 / 相机遮挡柱都已摆好，数值和正式关卡一致。"
                  + "\n如果材质发紫，检查 Starter Assets 的 URP 材质版本。");
    }

    private static void SetupSky()
    {
        Shader shader = Shader.Find("Sky/Gradient");

        if (shader == null)
        {
            return;
        }

        Material sky = AssetDatabase.LoadAssetAtPath<Material>(SkyMaterialPath);

        if (sky == null)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            {
                AssetDatabase.CreateFolder("Assets", "Materials");
            }

            sky = new Material(shader);
            sky.SetColor("_TopColor", new Color(0.42f, 0.34f, 0.58f));
            sky.SetColor("_HorizonColor", new Color(1.00f, 0.79f, 0.64f));
            sky.SetColor("_GroundColor", new Color(0.22f, 0.18f, 0.26f));
            AssetDatabase.CreateAsset(sky, SkyMaterialPath);
        }

        RenderSettings.skybox = sky;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.62f, 0.55f, 0.62f);
        RenderSettings.fog = false;
    }

    private static void CreateLight()
    {
        GameObject lightObject = new GameObject("Directional Light");
        lightObject.transform.rotation = Quaternion.Euler(48f, -35f, 0f);

        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.94f, 0.86f);
        light.intensity = 1.25f;
        light.shadows = LightShadows.Soft;
    }

    private static void CreateGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(6f, 1f, 6f);

        Renderer renderer = ground.GetComponent<Renderer>();
        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.color = new Color(0.55f, 0.5f, 0.58f);

        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }

        const string path = "Assets/Materials/M_SandboxGround.mat";

        if (AssetDatabase.LoadAssetAtPath<Material>(path) == null)
        {
            AssetDatabase.CreateAsset(material, path);
        }
        else
        {
            material = AssetDatabase.LoadAssetAtPath<Material>(path);
        }

        renderer.sharedMaterial = material;
    }

    private static GameObject CreateFollowCamera(Transform target)
    {
        GameObject cameraObject = InstantiatePrefab(CameraPrefabPath, "PlayerFollowCamera", Vector3.zero, Quaternion.identity);

        if (cameraObject == null)
        {
            return null;
        }

        CinemachineVirtualCamera virtualCamera = cameraObject.GetComponent<CinemachineVirtualCamera>();

        if (virtualCamera != null && target != null)
        {
            virtualCamera.Follow = target;
            virtualCamera.LookAt = target;
        }
        else
        {
            CinemachineFreeLook freeLook = cameraObject.GetComponent<CinemachineFreeLook>();

            if (freeLook != null && target != null)
            {
                freeLook.Follow = target;
                freeLook.LookAt = target;
            }
        }

        return cameraObject;
    }

    private static void BuildCourse()
    {
        Transform course = new GameObject("Course").transform;

        for (int i = 0; i < 5; i++)
        {
            SpawnBlock(course, new Vector3(0f, 0.5f, 2f + i * 1.5f), i % 2 == 0);
        }

        for (int i = 0; i < 4; i++)
        {
            SpawnBlock(course, new Vector3(2.6f, 0.5f + i, 2f + i * 1.5f), true);
        }

        for (int i = 0; i < 3; i++)
        {
            SpawnBlock(course, new Vector3(-2.6f, 0.5f + i, 5f), false);
        }

        for (int x = -1; x <= 1; x++)
        {
            for (int z = 0; z <= 1; z++)
            {
                SpawnBlock(course, new Vector3(x * 1.5f, 0.5f, 11f + z * 1.5f), true);
            }
        }
    }

    private static void SpawnBlock(Transform parent, Vector3 position, bool blue)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(blue ? BlueBlockPath : RedBlockPath);

        if (prefab == null)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "Block";
            cube.transform.SetParent(parent, false);
            cube.transform.position = position;
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.transform.SetParent(parent, true);
        instance.transform.position = position;
        instance.transform.rotation = Quaternion.identity;
    }

    private static GameObject InstantiatePrefab(string path, string name, Vector3 position, Quaternion rotation)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

        if (prefab == null)
        {
            return null;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = name;
        instance.transform.position = position;
        instance.transform.rotation = rotation;
        return instance;
    }

    private static Transform FindDeepChild(Transform parent, string name)
    {
        if (parent.name == name)
        {
            return parent;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindDeepChild(parent.GetChild(i), name);

            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
