using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ProjectCleanup
{
    [MenuItem("Tools/Cleanup/Remove Missing Scripts")]
    public static void RemoveMissingScripts()
    {
        Scene scene = SceneManager.GetActiveScene();

        if (!scene.isLoaded)
        {
            Debug.LogWarning("[Cleanup] 没有已打开的场景。");
            return;
        }

        int removed = 0;
        GameObject[] roots = scene.GetRootGameObjects();

        foreach (GameObject root in roots)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);

            foreach (Transform transform in transforms)
            {
                removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transform.gameObject);
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log($"[Cleanup] 移除 {removed} 个 Missing Script 组件，记得 Ctrl+S 保存场景。");
    }
}
