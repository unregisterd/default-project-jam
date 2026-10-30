using Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PlayerPrefabExporter
{
    private const string ScenePath = "Assets/Scenes/Sandbox/ThirdPersonTest.unity";
    private const string PrefabFolder = "Assets/Prefabs/Player";
    private const string PlayerPrefabPath = PrefabFolder + "/Player_ThirdPerson.prefab";
    private const string CameraPrefabPath = PrefabFolder + "/PlayerFollowCamera_TP.prefab";

    [MenuItem("Tools/Sandbox/Save Player As Prefab")]
    public static void Save()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        if (EditorSceneManager.GetActiveScene().path != ScenePath)
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        GameObject player = FindPlayer();

        if (player == null)
        {
            Debug.LogError("[Prefab] 场景里找不到 PlayerArmature。");
            return;
        }

        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        if (!AssetDatabase.IsValidFolder(PrefabFolder))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "Player");
        }

        GameObject playerPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect(
            player, PlayerPrefabPath, InteractionMode.UserAction);

        CinemachineVirtualCamera virtualCamera = Object.FindObjectOfType<CinemachineVirtualCamera>();

        if (virtualCamera != null)
        {
            GameObject cameraObject = virtualCamera.gameObject;

            if (cameraObject.GetComponent<CameraTargetAutoBinder>() == null)
            {
                cameraObject.AddComponent<CameraTargetAutoBinder>();
            }

            virtualCamera.Follow = null;
            virtualCamera.LookAt = null;

            PrefabUtility.SaveAsPrefabAssetAndConnect(cameraObject, CameraPrefabPath, InteractionMode.UserAction);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
        AssetDatabase.SaveAssets();

        Debug.Log("[Prefab] 已导出：\n  " + PlayerPrefabPath + "\n  " + CameraPrefabPath
                  + "\n新场景里两个都拖进去即可（相机带自动绑定，会自己找 PlayerArmature/PlayerCameraRoot）。"
                  + "\n玩家上的 CharacterController 尺寸、移动参数、技能组件都已包含。");
    }

    private static GameObject FindPlayer()
    {
        GameObject player = GameObject.Find("PlayerArmature");

        if (player != null)
        {
            return player;
        }

        MonoBehaviour[] behaviours = Object.FindObjectsOfType<MonoBehaviour>();

        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];

            if (behaviour != null && behaviour.GetType().Name == "ThirdPersonController")
            {
                return behaviour.gameObject;
            }
        }

        return null;
    }
}
