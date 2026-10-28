using UnityEditor;
using UnityEngine;

public static class MeshColliderTool
{
    [MenuItem("Tools/Art/Fix Mesh Collider On Selected")]
    public static void FixSelected()
    {
        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogWarning("[Collider] 先在 Hierarchy 里选中出生点（或任意模型根节点）。");
            return;
        }

        foreach (GameObject root in Selection.gameObjects)
        {
            MeshFilter filter = root.GetComponentInChildren<MeshFilter>(true);

            if (filter == null)
            {
                Debug.LogWarning($"[Collider] {root.name} 下没有 MeshFilter");
                continue;
            }

            GameObject target = filter.gameObject;
            MeshCollider collider = target.GetComponent<MeshCollider>();

            if (collider == null)
            {
                collider = target.AddComponent<MeshCollider>();
            }

            collider.sharedMesh = filter.sharedMesh;
            collider.convex = false;
            EditorUtility.SetDirty(collider);

            Debug.Log($"[Collider] 已把 MeshCollider 加到 {target.name}（网格 {filter.sharedMesh.name}，convex=false）");
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }
}
