using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SandboxAbilities
{
    private const string ScenePath = "Assets/Scenes/Sandbox/ThirdPersonTest.unity";
    private const string ActionsPath = "Assets/Settings/PlayerControls.inputactions";

    [MenuItem("Tools/Sandbox/Add Abilities To Player")]
    public static void Add()
    {
        Scene scene = EditorSceneManager.GetActiveScene();

        if (scene.path != ScenePath)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        GameObject player = GameObject.Find("PlayerArmature");

        if (player == null)
        {
            MonoBehaviour[] all = Object.FindObjectsOfType<MonoBehaviour>();
            System.Type type = System.Type.GetType("ThirdPersonController, Assembly-CSharp");

            if (type != null)
            {
                MonoBehaviour found = System.Array.Find(all, m => m != null && m.GetType() == type);

                if (found != null)
                {
                    player = found.gameObject;
                }
            }
        }

        if (player == null)
        {
            Debug.LogError("[Sandbox] 场景里找不到 PlayerArmature，先跑 Tools/Sandbox/Build Third Person Test。");
            return;
        }

        Ensure<ThirdPersonMotorAdapter>(player);
        Ensure<PlayerBlockAgent>(player);
        Ensure<PlayerInputReader>(player);

        InputAssetBinder.Bind(player);

        AbilityController controller = Ensure<AbilityController>(player);
        SerializedObject so = new SerializedObject(controller);
        SerializedProperty startPoints = so.FindProperty("startPoints");

        if (startPoints != null && startPoints.intValue < 6)
        {
            startPoints.intValue = 6;
        }

        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);

        Selection.activeGameObject = player;

        Debug.Log("[Sandbox] 技能已挂好：IMotor 适配器 + AbilityController(6 点) + PlayerBlockAgent + PlayerInputReader。"
                  + "\n按键 1 勇气 / 2 智慧 / 3 耐心 / 4 希望，技能成功会触发全局顿帧 0.05s。");
    }

    private static T Ensure<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();

        if (component == null)
        {
            component = target.AddComponent<T>();
        }

        return component;
    }

    private static class InputAssetBinder
    {
        public static void Bind(GameObject player)
        {
            PlayerInputReader reader = player.GetComponent<PlayerInputReader>();
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(ActionsPath);

            if (reader == null || asset == null)
            {
                return;
            }

            SerializedObject so = new SerializedObject(reader);
            SerializedProperty actions = so.FindProperty("actions");

            if (actions != null)
            {
                actions.objectReferenceValue = asset;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
