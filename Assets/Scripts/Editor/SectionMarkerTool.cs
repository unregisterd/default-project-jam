using UnityEditor;
using UnityEngine;

public static class SectionMarkerTool
{
    private const string LevelFolder = "Assets/Prefabs/Level";

    [MenuItem("Tools/Level/Add Section Markers to All")]
    public static void AddMarkersToAll()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { LevelFolder });
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (!path.StartsWith(LevelFolder))
            {
                continue;
            }

            if (AddMarkers(path))
            {
                count++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[Section] 处理完成，共 {count} 个预制体。");
    }

    private static bool AddMarkers(string path)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(path);

        try
        {
            Block[] blocks = root.GetComponentsInChildren<Block>(true);

            if (blocks.Length == 0)
            {
                Debug.LogWarning($"[Section] {path} 里没有 Block，跳过。");
                return false;
            }

            Block lowest = null;
            Block highest = null;
            float lowestTop = float.MaxValue;
            float highestTop = float.MinValue;

            foreach (Block block in blocks)
            {
                float top = block.GetTopHeight();

                if (top < lowestTop)
                {
                    lowestTop = top;
                    lowest = block;
                }

                if (top > highestTop)
                {
                    highestTop = top;
                    highest = block;
                }
            }

            TowerSection section = root.GetComponent<TowerSection>();

            if (section == null)
            {
                section = root.AddComponent<TowerSection>();
            }

            SerializedObject serialized = new SerializedObject(section);
            SerializedProperty entryProperty = serialized.FindProperty("entry");
            SerializedProperty exitProperty = serialized.FindProperty("exit");

            Transform entry = entryProperty.objectReferenceValue as Transform;
            Transform exit = exitProperty.objectReferenceValue as Transform;

            if (entry == null)
            {
                entry = CreateMarker(root.transform, "Entry");
            }

            if (exit == null)
            {
                exit = CreateMarker(root.transform, "Exit");
            }

            entry.position = new Vector3(lowest.transform.position.x, lowestTop, lowest.transform.position.z);
            exit.position = new Vector3(highest.transform.position.x, highestTop, highest.transform.position.z);

            entryProperty.objectReferenceValue = entry;
            exitProperty.objectReferenceValue = exit;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, path);

            Debug.Log(
                $"[Section] {System.IO.Path.GetFileName(path)}：Entry y={lowestTop:F1}（{lowest.name}），"
                + $"Exit y={highestTop:F1}（{highest.name}），高差 {highestTop - lowestTop:F1}m");

            return true;
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static Transform CreateMarker(Transform parent, string markerName)
    {
        Transform existing = parent.Find(markerName);

        if (existing != null)
        {
            return existing;
        }

        GameObject marker = new GameObject(markerName);
        marker.transform.SetParent(parent, false);
        marker.transform.localRotation = Quaternion.identity;
        marker.transform.localScale = Vector3.one;
        return marker.transform;
    }
}
