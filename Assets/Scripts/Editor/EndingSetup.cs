using UnityEditor;
using UnityEngine;

public static class EndingSetup
{
    private const string NightSkyPath = "Assets/Sky/SkySeries Freebie/CoriolisNight4k.mat";

    [MenuItem("Tools/Ending/Create Ending Director")]
    public static void Create()
    {
        GameObject existing = GameObject.Find("EndingDirector");
        GameObject go = existing != null ? existing : new GameObject("EndingDirector");

        EndingDirector director = go.GetComponent<EndingDirector>();

        if (director == null)
        {
            director = go.AddComponent<EndingDirector>();
        }

        Camera camera = Camera.main;
        FirstPersonMotor motor = Object.FindObjectOfType<FirstPersonMotor>();
        PlayerLook look = Object.FindObjectOfType<PlayerLook>();
        Material nightSky = AssetDatabase.LoadAssetAtPath<Material>(NightSkyPath);

        SerializedObject serialized = new SerializedObject(director);
        SetReference(serialized, "viewCamera", camera);
        SetReference(serialized, "motor", motor);
        SetReference(serialized, "look", look);

        if (motor != null)
        {
            SetReference(serialized, "playerRoot", motor.transform);
        }

        if (nightSky != null)
        {
            SetReference(serialized, "endingSkybox", nightSky);
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
        Selection.activeGameObject = go;

        Debug.Log("[Ending] 已创建 EndingDirector：F10 播放占位结局运镜，F9 还原"
                  + (nightSky != null ? "，天空盒使用 CoriolisNight4k" : "，但没找到夜景天空盒"));
    }

    private static void SetReference(SerializedObject serialized, string field, Object value)
    {
        SerializedProperty property = serialized.FindProperty(field);

        if (property != null && value != null)
        {
            property.objectReferenceValue = value;
        }
    }
}
